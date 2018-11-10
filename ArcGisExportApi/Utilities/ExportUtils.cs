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
        // TODO - remove this link to server
        static string serverExport = "https://gdiportal.gdi.net/server/rest/services/PGZ/PGZ_UI_QUERY_DATA/MapServer/";

        // size in pixels (by calculation 553 x 930):
        static int paperWidthPixels = 550;
        static int paperHeightPixels = 900;

        // get info (with the url and other info) by export:
        async public static Task<ExportResultList> getInfo(MapImageList response, List<string> PlanIdList,
            Extent extent, string uriLayer)
        {
            ExportResultList results = new ExportResultList();
            results.MapPlans = new List<ExportResult>();

            // get each component by exporting geometry:
            // CHECK = queryResult.Features == null
            for (int i = 0; i < PlanIdList.Count; i++)
            {
                string kartaSifra = PlanIdList[i];

                string linkMap = "export?f=json"
                    + "&format=png"
                    + AddBoundingBox(extent)
                    + "&size=" + paperWidthPixels + "," + paperHeightPixels
                    + "&mapScale=" + response.GetById(kartaSifra).MapScale
                    + AddLayer(uriLayer)
                    + AddLayerDefs(uriLayer, kartaSifra);

                ExportResult result = await ExportRepo.getImageInfo(serverExport + linkMap);
                result.Karta_Sifra = kartaSifra;
                results.MapPlans.Add(result);

                // TODO - add info and result (img) to output object
            }

            return results;
        }

        private static string AddLayerDefs(string uriLayer, string kartaSifra)
        {
            string query = getLayerFromUri(uriLayer) + ":" + "karta_sifra='" + kartaSifra + "'";
            return "&layerDefs=" + QueryUtils.encodeUrl(query);
        }
        
        public static string getImageUrl(Geometry geometry, string uriLayer)
        {
            Extent extent = FindPoints(geometry);

            // format PNG by default:
            string linkMap = "?f=image"
                    + "&format=png"
                    + AddBoundingBox(extent)
                    + ScaleSizeToCrop(extent)
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
        private static string ScaleSizeToCrop(Extent extent)
        {
            // size of the image (in coo):
            double xSize = extent.Xmax - extent.Xmin;
            double ySize = extent.Ymax - extent.Ymin;

            // scale by smaller size:
            double widthScale = paperWidthPixels / xSize;
            double heightScale = paperHeightPixels / ySize;
            double scale = widthScale < heightScale ? widthScale : heightScale;

            xSize *= scale;
            ySize *= scale;

            string str = "&size=" + (int)xSize + "," + (int)ySize;
            return str;
        }
    }
}
