using PGZ.UI.PrintService.Inputs;
using PGZ.UI.PrintService.Models;
using PGZ.UI.PrintService.Utilities;
using System.Collections.Generic;
using System.Threading.Tasks;
using static PGZ.UI.PrintService.Inputs.UrbanisticPlansResults;
using Novacode;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;

namespace PGZ.UI.PrintService.Services
{
    public class MapImageService
    {
        async public static Task<DataResponse> mapToReponse(DocX doc, DataRequest request)
        {
            if (request.UrbanisticPlansResults == null || request.UrbanisticPlansResults.Count == 0)
            {
                return new DataResponse();
            }

            // create a list of maps:
            DataResponse response = new DataResponse();

            // go through all urbanistic plans
            foreach (UrbanisticPlansResults urbanisticPlan in request.UrbanisticPlansResults)
            {
                List<string> mapPlanIdList = new List<string>();

                MapImageList planResults = new MapImageList()
                {
                    Status = urbanisticPlan.Status,
                    Type = urbanisticPlan.Type,
                    GisCode = urbanisticPlan.GisCode,
                    Name = urbanisticPlan.Name,
                    ServerPath = GetServerName(request.UrbanisticPlansResults[0].RasterRestURL),
                    PaperSize = new Models.Size()
                    {
                        Width = (int) doc.PageWidth,
                        Height = (int) doc.PageHeight
                    }
                };

                string rasterIdAttribute = urbanisticPlan.RasterIdAttribute;

                foreach (PlanMap planMap in urbanisticPlan.PlanMaps)
                {
                    // create map plan, with id and scales:
                    MapPlans map = new MapPlans
                    {
                        Id = planMap.Id,
                        Name = planMap.Name,
                        MapScale = planMap.MapScale,
                        OriginalScale = planMap.OriginalScale,
                        RasterIdAttribute = rasterIdAttribute
                    };
                    planResults.Maps.Add(map);
                    mapPlanIdList.Add(planMap.Id);
                }
                response.UrbanPlansImages.Add(planResults);

                // copy polygons (to draw later):
                foreach (SpatialCondition spatialCondition in request.SpatialConditionList)
                {
                    planResults.MapPolygons.Add(new MapPolygon(spatialCondition.Geometry));
                }

                var queryTasks = new List<Task>
                {
                    AddRaster(planResults, urbanisticPlan.RasterRestURL, mapPlanIdList),
                    AddLegends(planResults, urbanisticPlan.LegendRestURL, mapPlanIdList),
                    AddComponents(planResults, urbanisticPlan.ComponentRestURL, mapPlanIdList)
                };
                await Task.WhenAll(queryTasks);

                // get images of this urban plan:
                var imageTasks = new List<Task>();
                for (int i = 0; i < planResults.Maps.Count; i++)
                {
                    imageTasks.Add(DownloadRaster(planResults.Maps[i], planResults.MapPolygons));
                    imageTasks.Add(DownloadLegend(planResults.Maps[i]));
                    imageTasks.Add(DownloadComponent(planResults.Maps[i]));
                }
                await Task.WhenAll(imageTasks);
            }

            return response;
        }

        async public static Task DownloadRaster(MapPlans map, List<MapPolygon> polygons)
        {
            map.RasterImage = await StreamService.getImageFromUrlAsync(map.Raster.Href);

            // draw on the picture:
            map.RasterImage = DrawLines(map.RasterImage, map.Raster, polygons);
        }

        public static byte[] DrawLines(byte[] imageBytes, MapImage rasterInfo, List<MapPolygon> polygons)
        {
            Bitmap newBitmap = new Bitmap(rasterInfo.Width, rasterInfo.Height);
            using (var ms = new MemoryStream(imageBytes))
            {
                List<ScaledPolygon> scaledPolyList = calculatePiP(rasterInfo, polygons);

                // draw lines:
                Graphics graphics = Graphics.FromImage(newBitmap);
                graphics.DrawImage(new Bitmap(ms), 0, 0);

                Pen blackPen = new Pen(Color.Fuchsia, 4);
                foreach (ScaledPolygon polygon in scaledPolyList)
                {
                    for (int i = 1; i < polygon.Points.Count; i++)
                    {
                        graphics.DrawLine(blackPen,
                            polygon.Points[i - 1].XPoint, polygon.Points[i - 1].YPoint,
                            polygon.Points[i].XPoint, polygon.Points[i].YPoint);
                    }
                }

                // back to byte array:
                using (var savingMs = new MemoryStream())
                {
                    /*
                    newBitmap.Save(Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
                        + "\\image_" + number + ".png");
                    */
                    newBitmap.Save(savingMs, ImageFormat.Png);
                    return savingMs.ToArray();
                }
            }
        }

        public static List<ScaledPolygon> calculatePiP(MapImage rasterInfo, List<MapPolygon> polygons)
        {
            List<ScaledPolygon> scaledPoly = new List<ScaledPolygon>();

            // extent:
            double Xmin = rasterInfo.Extent.Xmin;
            double Ymin = rasterInfo.Extent.Ymin;
            double Xmax = rasterInfo.Extent.Xmax;
            double Ymax = rasterInfo.Extent.Ymax;

            // scaled points of the map (start is at 0):
            double endX = Xmax - Xmin;
            double endY = Ymax - Ymin;

            // re-write polygons points:
            foreach (MapPolygon polygon in polygons)
            {
                ScaledPolygon poly = new ScaledPolygon();
                foreach (MapPoint point in polygon.Points)
                {
                    // scale and calculate:
                    double x = point.XPoint - Xmin;
                    x = x / endX * rasterInfo.Width;

                    double y = point.YPoint - Ymin;
                    y = y / endY * rasterInfo.Height;

                    poly.Points.Add(new ScaledPoint(x, y));
                }
                scaledPoly.Add(poly);
            }

            return scaledPoly;
        }

        async public static Task DownloadLegend(MapPlans map)
        {
            map.LegendImage = await StreamService.getImageFromUrlAsync(map.LegendUrl);
        }

        async public static Task DownloadComponent(MapPlans map)
        {
            map.ComponentImage = await StreamService.getImageFromUrlAsync(map.ComponentUrl);
        }


        public static string GetServerName(string layerURL)
        {
            return layerURL.Substring(0, layerURL.LastIndexOf("/"));
        }

        // spatial condition added for geometry:
        async public static Task AddRaster(MapImageList response, string restUrl, List<string> mapPlanIds)
        {
            QueryResult rasterInfo = await QueryUtils.queryAll(restUrl, mapPlanIds);
            Extent extent = ExportUtils.FindPoints(response.MapPolygons);

            ExportResultList rasterImages = await ExportUtils.getImageInfo(response, restUrl, mapPlanIds,
                extent, rasterInfo);

            // response, add maps to that
            for (int i = 0; i < response.Maps.Count; i++)
            {
                for (int j = 0; j < rasterImages.MapPlans.Count; j++)
                {
                    if (response.Maps[i].Id == rasterImages.MapPlans[j].Karta_Sifra)
                    {
                        response.Maps[i].Raster = new MapImage
                        {
                            Href = rasterImages.MapPlans[j].Href,
                            Scale = rasterImages.MapPlans[j].Scale,
                            Extent = rasterImages.MapPlans[j].Extent,
                            Width = rasterImages.MapPlans[j].Width,
                            Height = rasterImages.MapPlans[j].Height
                        };

                        break;
                    }
                }
            }
        }

        async public static Task AddLegends(MapImageList response, string restUrl, List<string> mapPlanIds)
        {
            QueryResult legendsInfo = await QueryUtils.queryAll(restUrl, mapPlanIds);

            for (int i = 0; i < response.Maps.Count; i++)
            {
                response.Maps[i].LegendUrl = response.ServerPath + "/export" + ExportUtils
                    .getImageUrl(legendsInfo.Features[i].Geometry, response.PaperSize, restUrl);
            }
        }

        async public static Task AddComponents(MapImageList response, string restUrl, List<string> mapPlanIds)
        {
            QueryResult componentsInfo = await QueryUtils.queryAll(restUrl, mapPlanIds);

            for (int i = 0; i < response.Maps.Count; i++)
            {
                response.Maps[i].ComponentUrl = response.ServerPath + "/export" + ExportUtils
                    .getImageUrl(componentsInfo.Features[i].Geometry, response.PaperSize, restUrl);
            }
        }
    }
}