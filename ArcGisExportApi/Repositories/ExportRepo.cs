﻿using PGZ.UI.PrintService.Models;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace PGZ.UI.PrintService.Services
{
    class ExportRepo
    {
        static string serverExport = "https://gdiportal.gdi.net/server/rest/services/PGZ/PGZ_UI_QUERY_DATA/MapServer/export";

        public static async Task<ExportResult> getImageInfo(string link)
        {
            using (var client = new HttpClient())
            {
                // create map uri:
                String uri = serverExport + link;
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
