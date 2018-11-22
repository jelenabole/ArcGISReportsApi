using PGZ.UI.PrintService.Models;
using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;

namespace PGZ.UI.PrintService.Services
{
    class ExportRepo
    {
        public static async Task<ExportResult> getImageInfo(string uri)
        {
            using (var client = new HttpClient())
            {
                // create map uri:
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
                    Item = JsonConvert.DeserializeObject<ExportResult>(content, jsonSettings);
                }
                return Item;
            }
        }
    }
}
