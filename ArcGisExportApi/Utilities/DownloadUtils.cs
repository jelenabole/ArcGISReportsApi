using ArcGisExportApi.Inputs;
using Newtonsoft.Json;
using System;
using System.IO;

namespace ArcGisExportApi.Utilities
{
    public class DownloadUtils
    {
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
