using PGZ.UI.PrintService.Models;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace PGZ.UI.PrintService.Services
{
    class QueryRepo
    {

        public async Task<QueryResult> getQuery(String uri)
        {
            using (var client = new HttpClient())
            {
                var response = await client.GetAsync(uri);
                var Item = new QueryResult();
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    Item = JsonConvert.DeserializeObject<QueryResult>(content, 
                        new JsonSerializerSettings {
                            NullValueHandling = NullValueHandling.Ignore
                    });
                }
                return Item;
            }
        }
    }
}
