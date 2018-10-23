using ArcGisExportApi.Models;
using ArcGisExportApi.Services;
using Newtonsoft.Json;
using System;
using System.IO;

namespace ArcGisExportApi.TestUtils
{
    public class DownloadUtils
    {
        // TODO - maknut ovo:
        private static QueryService queryService = new QueryService();
        private static ExportService mapService = new ExportService();
        // int index = 0; // temp

        public static DataRequest getData()
        {
            return DeserializeJsonFile();
        }

        private static DataRequest DeserializeJsonFile()
        {
            // stream from a file:
            var serializer = new JsonSerializer();
            string str = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
                    + "\\" + "copy.json";

            // sent text instead of stream
            using (var sr = new StreamReader(str))
            using (var jsonTextReader = new JsonTextReader(sr))
            {
                return (DataRequest)serializer.Deserialize(sr, typeof(DataRequest));
            }
        }

        /*
        async public static void downloadAll(QueryResult queryResult, string uriLayer)
        {
            for (int i = 0; i < queryResult.Features.Count; i++)
            {
                // calculate geometry of the first legend:
                Extent legendExtent = CalcUtils.FindPoints(queryResult.Features[i].Geometry);

                string linkMap = "?f=json" + AddBoundingBox(legendExtent)
                    + CalculateSizeToCrop(legendExtent)
                    // Layer removed - cause its green, while the legend is on the 21th layer:
                    + AddLayerGroup(uriLayer);

                Trace.WriteLine("link for legend:");
                Trace.WriteLine(linkMap);
                MapExport exportedLegend = await mapService.GetMapExport(linkMap);

                if (exportedLegend.Href != null)
                {
                    string name = legends.Features[i].Attributes.Name ?? "map_" + index;
                    index++;
                    SaveImage(exportedLegend, name, "png");

                }
            }
        }
        */

    }
}
