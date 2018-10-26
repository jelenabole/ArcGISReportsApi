using ArcGisExportApi.Tests;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;

namespace ArcGisExportApi.Services
{
    class QueryService
    {
        HttpClient client;

        public QueryService()
        {
            client = new HttpClient
            {
                MaxResponseContentBufferSize = 256000
            };
        }

        public async Task<QueryResult> getQuery(String link)
        {
            string uri = link;
            // Trace.WriteLine("get query:\n" + uri);

            // json settings = ignore null fields:
            var jsonSettings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            };

            // get map data:
            var response = await client.GetAsync(uri);
            var Item = new QueryResult();
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                Item = JsonConvert.DeserializeObject<QueryResult>(content, jsonSettings);
            }
            return Item;
        }
    }
}
