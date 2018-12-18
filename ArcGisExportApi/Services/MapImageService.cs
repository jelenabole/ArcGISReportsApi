﻿using PGZ.UI.PrintService.Models;
using PGZ.UI.PrintService.Utilities;
using System.Collections.Generic;
using System.Threading.Tasks;
using Novacode;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;

namespace PGZ.UI.PrintService.Services
{
    public class MapImageService
    {
        async public static Task AddExportedData(DocX doc, DataResponse response)
        {
            // urbanistic plans:
            foreach (UrbanPlan urbanPlan in response.UrbanPlans)
            {
                if (urbanPlan.Maps.Count != 0 && urbanPlan.Maps != null)
                {
                    var queryTasks = new List<Task>
                {
                    AddRaster(urbanPlan, response.PolygonsExtent),
                    AddLegends(urbanPlan),
                    AddComponents(urbanPlan)

                };

                await Task.WhenAll(queryTasks);

                // images of this urban plan:
                   
                var imageTasks = new List<Task>();
                for (int i = 0; i < urbanPlan.Maps.Count; i++)
                {
                    imageTasks.Add(DownloadRaster(urbanPlan.Maps[i],
                        response.Polygons, response.HighlightColor));
                    imageTasks.Add(DownloadLegend(urbanPlan.Maps[i]));
                    imageTasks.Add(DownloadComponent(urbanPlan.Maps[i]));
                }

                await Task.WhenAll(imageTasks);

                //baseMaps:

                if (response.BaseMaps != null && response.BaseMaps.Count != 0)
                {
                    foreach (BaseMap baseMap in response.BaseMaps)
                    {
                            foreach (Map map in urbanPlan.Maps)
                            {
                                    var queryBaseMapTasks = new List<Task>
                            {
                                AddBasemap(baseMap, response.PolygonsExtent, urbanPlan, map)
                            };

                                await Task.WhenAll(queryBaseMapTasks);

                            }
                    }
                }



                if (response.BaseMaps != null && response.BaseMaps.Count != 0)
                {
                    var queryBaseMapImageTasks = new List<Task>();
                    foreach (BaseMap baseMap in response.BaseMaps)
                    {
                        queryBaseMapImageTasks.Add(DownloadBasemap(baseMap,
                            response.Polygons, response.HighlightColor));
                    }
                    await Task.WhenAll(queryBaseMapImageTasks);
                }
            }
        }

            
        }

        async public static Task DownloadRaster(Map map, List<MapPolygon> polygons, string color)
        {
            map.RasterImage = await StreamService.getImageFromUrlAsync(map.Raster.Href);

            map.RasterImage = DrawLines(map.RasterImage, map.Raster, polygons, color);
        }

        async public static Task DownloadBasemap(BaseMap baseMap, List<MapPolygon> polygons, string color)
        {
            baseMap.ResultFeatures[0].BaseMapImage = await StreamService.getImageFromUrlAsync(baseMap.ResultFeatures[0].BaseMap.Href);

            baseMap.ResultFeatures[0].BaseMapImage = DrawLines(baseMap.ResultFeatures[0].BaseMapImage, baseMap.ResultFeatures[0].BaseMap, polygons, color);
        }

    public static byte[] DrawLines(byte[] imageBytes, MapImage rasterInfo, List<MapPolygon> polygons,
            string color)
        {
            Bitmap newBitmap = new Bitmap(rasterInfo.Width, rasterInfo.Height);
            using (var ms = new MemoryStream(imageBytes))
            {
                List<ScaledPolygon> scaledPolyList = calculatePiP(rasterInfo, polygons);

                // draw lines:
                Graphics graphics = Graphics.FromImage(newBitmap);
                graphics.DrawImage(new Bitmap(ms), 0, 0);

                // set color:
                Color penColor = Color.Cyan;
                if (color != null)
                {
                    // correct format "#xxxxxx"
                    penColor = ColorTranslator.FromHtml(color);
                }
                Pen colorPen = new Pen(penColor, 4);

                foreach (ScaledPolygon polygon in scaledPolyList)
                {
                    for (int i = 1; i < polygon.Points.Count; i++)
                    {
                        graphics.DrawLine(colorPen,
                            polygon.Points[i - 1].XPoint, polygon.Points[i - 1].YPoint,
                            polygon.Points[i].XPoint, polygon.Points[i].YPoint);
                    }
                }

                // back to byte array:
                using (var savingMs = new MemoryStream())
                {
                    newBitmap.Save(savingMs, ImageFormat.Png);
                    return savingMs.ToArray();
                }
            }
        }

        public static List<ScaledPolygon> calculatePiP(MapImage rasterInfo, List<MapPolygon> polygons)
        {
            List<ScaledPolygon> scaledPolygons = new List<ScaledPolygon>();

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

                    poly.AddPoint(x, y);
                }
                scaledPolygons.Add(poly);
            }

            return scaledPolygons;
        }

        async public static Task DownloadLegend(Map map)
        {
            map.LegendImage = await StreamService.getImageFromUrlAsync(map.LegendUrl);
        }

        async public static Task DownloadComponent(Map map)
        {
            map.ComponentImage = await StreamService.getImageFromUrlAsync(map.ComponentUrl);
        }



        async public static Task AddRaster(UrbanPlan urbanPlan, Extent polygonsExtent)
        {
            QueryResult rasterInfo = await QueryUtils.createQueryForAll(urbanPlan, urbanPlan.RasterRestURL);
            if (urbanPlan.Maps != null && urbanPlan.Maps.Count != 0)
            {
                foreach (Map map in urbanPlan.Maps)
                {
                    map.FullMapExtent = ExportUtils.FindPoints(rasterInfo.GetGeometryByMapId(map.Id));
                }
            }

            await ExportUtils.getRasterInfo(urbanPlan, polygonsExtent);
        }

        async public static Task AddLegends(UrbanPlan urbanPlan)
        {
            QueryResult legendsInfo = await QueryUtils.createQueryForAll(urbanPlan, urbanPlan.LegendRestURL);

            string exportUrl = urbanPlan.ServerPath + "/export";
            
            foreach (Map map in urbanPlan.Maps)
            {
                map.LegendUrl = exportUrl + ExportUtils.getImageUrl(
                    legendsInfo.GetGeometryByMapId(map.Id),
                    urbanPlan.PaperSize, urbanPlan.LegendRestURL) + ExportUtils.AddLayerDefs(urbanPlan.LegendRestURL, urbanPlan.RasterIdAttribute, map.Id);
            }
            
        }

        async public static Task AddComponents(UrbanPlan urbanPlan)
        {
            QueryResult componentsInfo = await QueryUtils.createQueryForAll(urbanPlan, urbanPlan.ComponentRestURL);

            string exportUrl = urbanPlan.ServerPath + "/export";
            foreach (Map map in urbanPlan.Maps)
            {
                map.ComponentUrl = exportUrl + ExportUtils.getImageUrl(
                    componentsInfo.GetGeometryByMapId(map.Id),
                    urbanPlan.PaperSize, urbanPlan.ComponentRestURL) + ExportUtils.AddLayerDefs(urbanPlan.ComponentRestURL, urbanPlan.RasterIdAttribute, map.Id);
            }
            
        }
        
        async public static Task AddBasemap(BaseMap baseMap, Extent polygonsExtent, UrbanPlan urbanPlan, Map map)
        {
            QueryResult basemapInfo = await QueryUtils.createQueryForAll(urbanPlan, urbanPlan.RasterRestURL, baseMap, map);

            baseMap.ResultFeatures[0].FullMapExtent = ExportUtils.FindPoints(basemapInfo.GetGeometryByMapId(map.Id));

            await ExportUtils.getBasemapInfo(baseMap, polygonsExtent);
        }
    }
}