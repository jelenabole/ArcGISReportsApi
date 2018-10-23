using ArcGisExportApi.Tests;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;

namespace ArcGisExportApi.Services
{
    class ExportService
    {
        // TODO - remove:
        string server = "https://gdiportal.gdi.net/server/rest/services/PGZ/PGZ_UI_QUERY_DATA/MapServer/";
        HttpClient client;

        public ExportService()
        {
            client = new HttpClient
            {
                MaxResponseContentBufferSize = 256000
            };
        }

        public async Task<ExportResult> getImage(string link)
        {
            // create map uri:
            String uri = server + "export" + link;
            Trace.WriteLine("get export:\n" + uri);

            // json settings = ignore null fields:
            var jsonSettings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            };

            // get map data:
            var response = await client.GetAsync(uri);
            var Item = new ExportResult();
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                Trace.Write(content);
                Item = JsonConvert.DeserializeObject<ExportResult>(content, jsonSettings);
            }
            return Item;
        }
    }
}
