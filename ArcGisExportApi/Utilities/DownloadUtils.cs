using ArcGisExportApi.Models;
using ArcGisExportApi.Services;
using Newtonsoft.Json;
using System;
using System.IO;

namespace ArcGisExportApi.Utilities
{
    public class DownloadUtils
    {
        // TODO - maknut ovo:
        private static QueryRepo queryService = new QueryRepo();
        private static ExportRepo mapService = new ExportRepo();
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
