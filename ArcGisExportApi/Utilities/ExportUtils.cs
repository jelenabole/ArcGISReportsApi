using PGZ.UI.PrintService.Services;
using System.Threading.Tasks;
using System;
using System.Globalization;
using System.Collections.Generic;
using PGZ.UI.PrintService.Models;

namespace PGZ.UI.PrintService.Utilities
{
    public class ExportUtils
    {
        async public static Task getRasterInfo(UrbanPlan urbanPlan, Extent polygonExtent)
        {
            foreach (Map map in urbanPlan.Maps)
            {
                string mapScale = null;
                string boundingBox = null;
                if (map.MapScale.Contains("SPATIAL_CONDITION_EXTENT"))
                {
                    boundingBox = AddBoundingBox(AddPaddingToExtent(polygonExtent));
                }
                else if (map.MapScale.Contains("PLAN_MAP_EXTENT"))
                {
                    boundingBox = AddBoundingBox(map.FullMapExtent);
                }
                else
                {
                    // parse to number:
                    mapScale = map.MapScale;
                    // try parse = throw error, value not recognized (for map scale)
                    boundingBox = AddBoundingBox(polygonExtent);
                }

                string linkMap = "export?f=json"
                    + "&format=png"
                    + boundingBox
                    + "&size=" + urbanPlan.PaperSize.Width + "," + urbanPlan.PaperSize.Height
                    + "&mapScale=" + mapScale
                    + AddLayer(urbanPlan.RasterRestURL)
                    + AddLayerDefs(urbanPlan.RasterRestURL, urbanPlan.RasterIdAttribute, map.Id);

                ExportResult result = await ExportRepo.getImageInfo(urbanPlan.ServerPath + "/" + linkMap);

                map.Raster = new MapImage
                {
                    Href = result.Href,
                    Scale = result.Scale,
                    Extent = result.Extent,
                    Width = result.Width,
                    Height = result.Height
                };

            }
        }

        async public static Task getBasemapInfo(BaseMap baseMap, Extent polygonExtent)
        {
            foreach (BaseMap.ResultFeature map in baseMap.ResultFeatures)
            {
                string mapScale = null;
                string boundingBox = null;
                if(map.MapScale == null)
                {
                    boundingBox = AddBoundingBox(map.FullMapExtent);
                }
                else if (map.MapScale.Contains("SPATIAL_CONDITION_EXTENT"))
                {
                    boundingBox = AddBoundingBox(AddPaddingToExtent(polygonExtent));
                }
                else if (map.MapScale.Contains("PLAN_MAP_EXTENT"))
                {
                    boundingBox = AddBoundingBox(map.FullMapExtent);
                }
                else
                {
                    // parse to number:
                    mapScale = map.MapScale;
                    // try parse = throw error, value not recognized (for map scale)
                    boundingBox = AddBoundingBox(polygonExtent);
                }

                string linkMap = "export?f=json"
                    + "&format=png"
                    + boundingBox
                    + "&size=" + baseMap.PaperSize.Width + "," + baseMap.PaperSize.Height
                    + "&mapScale=" + mapScale
                    + AddLayer(baseMap.RestUrl)
                    + AddBaseMapLayerDefs(baseMap.RestUrl, "KARTA_SIFRA", map.Id);

                ExportResult result = await ExportRepo.getImageInfo(baseMap.ServerPath + "/" + linkMap);

                map.BaseMap = new MapImage
                {
                    Href = result.Href,
                    Scale = result.Scale,
                    Extent = result.Extent,
                    Width = result.Width,
                    Height = result.Height
                };

            }
        }

        public static string AddLayerDefs(string uriLayer, string byField, string kartaSifra)
        {
            string query = getLayerFromUri(uriLayer) + ":" + byField + "='" + kartaSifra + "'";
            return "&layerDefs=" + QueryUtils.encodeUrl(query);
        }

        public static string AddBaseMapLayerDefs(string uriLayer, string byField, string kartaSifra)
        {
            string query = getLayerFromUri(uriLayer);
            return "&layerDefs=" + QueryUtils.encodeUrl(query);
        }

        public static string getImageUrl(Geometry geometry, Size paperSize, string uriLayer)
        {
            Extent extent = FindPoints(geometry);

            // TODO - format PNG:
            string linkMap = "?f=image"
                    + "&format=png"
                    + AddBoundingBox(extent)
                    + ScaleSizeToCrop(extent, paperSize)
                    + AddLayer(uriLayer);

            return linkMap;
        }

        private static string AddBoundingBox(Extent extent)
        {
            return string.Format("&bbox={0},{1},{2},{3}",
                extent.Xmin.ToString(CultureInfo.InvariantCulture),
                extent.Ymin.ToString(CultureInfo.InvariantCulture),
                extent.Xmax.ToString(CultureInfo.InvariantCulture),
                extent.Ymax.ToString(CultureInfo.InvariantCulture));
        }

        private static string AddLayer(string link)
        {
            return "&layers=show:" + getLayerFromUri(link);
        }

        private static string getLayerFromUri(string link)
        {
            string search = "/";
            link = link.Substring(link.LastIndexOf(search) + search.Length);
            return link;
        }



        public static Extent FindPoints(List<MapPolygon> polygons)
        {
            Extent borders = new Extent
            {
                Xmin = Double.MaxValue,
                Xmax = 0,
                Ymin = Double.MaxValue,
                Ymax = 0
            };

            foreach (MapPolygon polygon in polygons)
            {
                foreach (MapPoint point in polygon.Points)
                {
                    if (point.XPoint < borders.Xmin)
                        borders.Xmin = point.XPoint;
                    if (point.XPoint > borders.Xmax)
                        borders.Xmax = point.XPoint;

                    if (point.YPoint < borders.Ymin)
                        borders.Ymin = point.YPoint;
                    if (point.YPoint > borders.Ymax)
                        borders.Ymax = point.YPoint;
                }
            }

            return borders;
        }

        public static Extent FindPoints(Geometry geometry)
        {
            Extent borders = new Extent
            {
                Xmin = Double.MaxValue,
                Xmax = 0,
                Ymin = Double.MaxValue,
                Ymax = 0
            };

            List<List<double>> points = geometry.Rings[0];
            for (int i = 0; i < points.Count; i++)
            {
                if (points[i][0] < borders.Xmin)
                    borders.Xmin = points[i][0];
                if (points[i][0] > borders.Xmax)
                    borders.Xmax = points[i][0];

                if (points[i][1] < borders.Ymin)
                    borders.Ymin = points[i][1];
                if (points[i][1] > borders.Ymax)
                    borders.Ymax = points[i][1];
            }

            return borders;
        }

        private static string ScaleSizeToCrop(Extent extent, Size paperSize)
        {
            double xSize = extent.Xmax - extent.Xmin;
            double ySize = extent.Ymax - extent.Ymin;

            double widthScale = paperSize.Width / xSize;
            double heightScale = paperSize.Height / ySize;
            double scale = widthScale < heightScale ? widthScale : heightScale;

            xSize *= scale;
            ySize *= scale;

            string str = "&size=" + (int)xSize + "," + (int)ySize;
            return str;
        }

        private static Extent AddPaddingToExtent(Extent extent)
        {
            double paddingPercent = 0.3;

            double xSize = extent.Xmax - extent.Xmin;
            double ySize = extent.Ymax - extent.Ymin;

            double paddingHorizontal = xSize * paddingPercent;
            double paddingVertical = ySize * paddingPercent;

            Extent newExtent = new Extent();
            newExtent.Xmax = extent.Xmax + paddingHorizontal;
            newExtent.Xmin = extent.Xmin - paddingHorizontal;
            newExtent.Ymax = extent.Ymax + paddingVertical;
            newExtent.Ymin = extent.Ymin - paddingVertical;

            return newExtent;
        }
    }
}