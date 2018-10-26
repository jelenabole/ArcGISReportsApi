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

    }
}
