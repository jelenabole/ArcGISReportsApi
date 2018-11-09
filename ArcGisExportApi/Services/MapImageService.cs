using PGZ.UI.PrintService.Inputs;
using PGZ.UI.PrintService.Models;
using PGZ.UI.PrintService.Utilities;
using System.Collections.Generic;
using System.Threading.Tasks;
using static PGZ.UI.PrintService.Inputs.UrbanisticPlansResults;
using Novacode;
using System.Threading;
using System.IO;
using System.Drawing;
using System;
using System.Drawing.Imaging;

namespace PGZ.UI.PrintService.Services
{
    public class MapImageService
    {
        async public static Task<MapImageList> mapToReponse(DocX doc, DataRequest request)
        {
            // CHECK - if there are no urban.plan. results checked - return null:
            if (request.UrbanisticPlansResults == null || request.UrbanisticPlansResults.Count == 0)
            {
                return null;
            }

            // copy polygons (to draw later):
            List<MapPolygon> polygons = new List<MapPolygon>();
            foreach (SpatialCondition spatialCondition in request.SpatialConditionList)
            {
                polygons.Add(new MapPolygon(spatialCondition.Geometry));
            }

            // create a list of maps:
            MapImageList response = new MapImageList();
            response.ServerPath = GetServerName(request.UrbanisticPlansResults[0].RasterRestURL);
            List<string> mapPlanIdList = new List<string>();

            foreach (PlanMap planMap in request.UrbanisticPlansResults[0].PlanMaps)
            {
                // create map plan, with id and scales:
                MapPlans map = new MapPlans
                {
                    Id = planMap.Id,
                    MapScale = planMap.MapScale,
                    OriginalScale = planMap.OriginalScale
                };
                response.Maps.Add(map);

                mapPlanIdList.Add(planMap.Id);
            }

            // TODO - +polygon
            // TODO - do this for all urban plans:
            var queryTasks = new List<Task>
            {
                AddRaster(response, request.UrbanisticPlansResults[0].RasterRestURL, mapPlanIdList),
                AddLegends(response, request.UrbanisticPlansResults[0].LegendRestURL, mapPlanIdList),
                AddComponents(response, request.UrbanisticPlansResults[0].ComponentRestURL, mapPlanIdList)
            };
            await Task.WhenAll(queryTasks);
            // get images:
            var imageTasks = new List<Task>();
            for (int i = 0; i < response.Maps.Count; i++)
            {
                imageTasks.Add(DownloadRaster(response.Maps[i], polygons, i, i + " - rast"));
                imageTasks.Add(DownloadLegend(response.Maps[i], i, i + " - leg"));
                imageTasks.Add(DownloadComponent(response.Maps[i], i, i + " - comp"));
            }
            await Task.WhenAll(imageTasks);

            return response;
        }

        async public static Task DownloadRaster(MapPlans map, List<MapPolygon> polygons, int i, string name)
        {
            map.RasterImage = await StreamService.getImageFromUrlAsync(map.Raster.Href);

            // draw on the picture:
            map.RasterImage = DrawLines(map.RasterImage, map.Raster, polygons, i);
        }

        public static byte[] DrawLines(byte[] imageBytes, MapImage rasterInfo, List<MapPolygon> polygons, int number)
        {
            Bitmap newBitmap = new Bitmap(rasterInfo.Width, rasterInfo.Height);
            using (var ms = new MemoryStream(imageBytes))
            {
                List<ScaledPolygon> scaledPolyList = calculatePiP(rasterInfo, polygons);

                // draw lines:
                Graphics graphics = Graphics.FromImage(newBitmap);
                graphics.DrawImage(new Bitmap(ms), 0, 0);

                Pen blackPen = new Pen(Color.OrangeRed, 3);
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



        async public static Task DownloadLegend(MapPlans map, int i, string name)
        {
            map.LegendImage = await StreamService.getImageFromUrlAsync(map.LegendUrl);
        }

        async public static Task DownloadComponent(MapPlans map, int i, string name)
        {
            map.ComponentImage = await StreamService.getImageFromUrlAsync(map.ComponentUrl);
        }


        public static string GetServerName(string layerURL)
        {
            return layerURL.Substring(0, layerURL.LastIndexOf("/"));
        }

        async public static Task AddRaster(MapImageList response, string restUrl, List<string> mapPlanIds)
        {
            QueryResult rasterInfo = await QueryUtils.queryAll(restUrl, mapPlanIds);

            // response (for scales):
            ExportResultList rasterImages = await ExportUtils.getInfo(response,
                rasterInfo, restUrl);

            // response, add maps to that
            // TODO - put data in output object (by id) (...)
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
                response.Maps[i].LegendUrl = response.ServerPath + "/export" 
                    + ExportUtils.getImageUrl(legendsInfo.Features[i].Geometry, restUrl);
            }
        }

        async public static Task AddComponents(MapImageList response, string restUrl, List<string> mapPlanIds)
        {
            QueryResult componentsInfo = await QueryUtils.queryAll(restUrl, mapPlanIds);

            for (int i = 0; i < response.Maps.Count; i++)
            {
                response.Maps[i].ComponentUrl = response.ServerPath + "/export"
                    + ExportUtils.getImageUrl(componentsInfo.Features[i].Geometry, restUrl);
            }
        }
    }
}