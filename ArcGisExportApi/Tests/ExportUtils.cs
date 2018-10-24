using System.Diagnostics;
using ArcGisExportApi.Tests;
using ArcGisExportApi.Services;
using System.Threading.Tasks;
using System;
using System.Globalization;
using System.Collections.Generic;

namespace ArcGisExportApi.TestUtils
{
    public class ExportUtils
    {
        // TODO - remove:
        private static ExportService exportService = new ExportService();
        
        // dpi (image = 96, vector = 300):
        // paper size in inches (without margins):
        static Decimal DPI = 96;
        static Decimal PAPER_WIDTH = 8.27M - (1.25M * 2.0M);
        static Decimal PAPER_HEIGHT = 11.69M - 2;


        async public static Task<ExportResultList> getAll(QueryResult queryResult, string uriLayer)
        {
            ExportResultList results = new ExportResultList();
            results.MapPlans = new List<ExportResult>();

            // get each component by exporting geometry:
            for (int i = 0; i < queryResult.Features.Count; i++)
            {
                Extent extent = FindPoints(queryResult.Features[i].Geometry);

                string linkMap = "?f=json" + AddBoundingBox(extent)
                    + ScaleSizeToCrop(extent)
                    // Layers group - green screen:
                    + AddLayerGroup(uriLayer);

                Trace.WriteLine("link for component: " + linkMap);

                ExportResult result = await exportService.getImage(linkMap);
                result.Id = queryResult.Features[i].Attributes.ObjectId;
                results.MapPlans.Add(result);

                // TODO - add info and result (img) to output object
            }

            return results;
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

        private static string AddLayerGroup(string link)
        {
            string layer = getLayerFromUri(link);
            return "&layers=show:" + getLayerGroup(layer);
        }

        // replace special characters from url:
        private static int getLayerGroup(string layerStr)
        {
            int layer = int.Parse(layerStr);
            if (layer <= 13)
                return layer;
            else if (layer <= 20)
                return 14;
            else if (layer <= 27)
                return 21;
            else if (layer <= 34)
                return 28;

            return 0;
        }

        private static string getLayerFromUri(string link)
        {
            // global - search term should be name of the server
            string search = "/MapServer/";
            link = link.Substring(link.LastIndexOf(search) + search.Length);
            int end = link.IndexOf("/");

            // if / == null or the
            if (end != -1)
                link = link.Substring(0, end);

            return link;
        }



        /* ADDITIONAL FUNCTIONS FOR MAP SIZE CALCS */

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
            // size in pixels:
            int paperWidthPixels = (int)Math.Floor(DPI * PAPER_WIDTH);
            int paperHeightPixels = (int)Math.Floor(DPI * PAPER_HEIGHT);
            // Trace.WriteLine("paper WxH in pixels (max): " + paperWidthPixels + " x " + paperHeightPixels);

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
