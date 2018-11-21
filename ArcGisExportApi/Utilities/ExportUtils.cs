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
        // get info by export (from extent)
        async public static Task getRasterInfo(UrbanPlan urbanPlan, Extent extent, QueryResult rasterInfo)
        {
            /*
            ExportResultList results = new ExportResultList()
            {
                MapPlans = new List<ExportResult>()
            };
            */

            // get each component by exporting geometry:
            // CHECK = queryResult.Features == null
            foreach (Map map in urbanPlan.Maps)
            {
                // map scale provjeriti i izračunati:
                string mapScale = null;
                string boundingBox = null;
                if (map.MapScale.Contains("SPATIAL_CONDITION_EXTENT"))
                {
                    mapScale = null;
                    boundingBox = AddBoundingBox(extent);
                }
                else if (map.MapScale.Contains("PLAN_MAP_EXTENT"))
                {
                    mapScale = null;
                    boundingBox = AddBoundingBox(FindPoints(rasterInfo.GetGeometryByMapId(map.Id)));
                }
                else
                {
                    // parse to number:
                    mapScale = map.MapScale;
                    // try parse = throw error, value not recognized (for map scale)
                    boundingBox = AddBoundingBox(extent);
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

        private static string AddLayerDefs(string uriLayer, string byField, string kartaSifra)
        {
            string query = getLayerFromUri(uriLayer) + ":" + byField + "='" + kartaSifra + "'";
            return "&layerDefs=" + QueryUtils.encodeUrl(query);
        }
        
        public static string getImageUrl(Geometry geometry, Size paperSize, string uriLayer)
        {
            Extent extent = FindPoints(geometry);

            // format PNG by default:
            string linkMap = "?f=image"
                    + "&format=png"
                    + AddBoundingBox(extent)
                    + ScaleSizeToCrop(extent, paperSize)
                    + AddLayer(uriLayer);

            return linkMap;
        }



        /* FUNCTIONS FOR URL */

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
            // global - search term should be name of the server
            string search = "/";
            link = link.Substring(link.LastIndexOf(search) + search.Length);

            return link;
        }



        /* ADDITIONAL FUNCTIONS FOR MAP SIZE CALCS */

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

            // features (particles) x,y points:
            List<List<double>> points = geometry.Rings[0];

            // min/max x,y:
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

        // crop legend/component layer (size scaled to fit the paper size):
        private static string ScaleSizeToCrop(Extent extent, Size paperSize)
        {
            // size of the image (in coo):
            double xSize = extent.Xmax - extent.Xmin;
            double ySize = extent.Ymax - extent.Ymin;

            // scale by smaller size:
            double widthScale = paperSize.Width / xSize;
            double heightScale = paperSize.Height / ySize;
            double scale = widthScale < heightScale ? widthScale : heightScale;

            xSize *= scale;
            ySize *= scale;

            string str = "&size=" + (int)xSize + "," + (int)ySize;
            return str;
        }
    }
}
