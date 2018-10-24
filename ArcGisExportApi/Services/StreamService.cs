using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace ArcGisExportApi.Services
{
    class StreamService
    {
        public static async Task DownloadImage(Uri requestUri, string filename)
        {
            using (var client = new HttpClient())
            using (var request = new HttpRequestMessage(HttpMethod.Get, requestUri))
            using (
                Stream contentStream = await (await client.SendAsync(request)).Content.ReadAsStreamAsync(),
                stream = new FileStream(Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
                    + "\\" + filename, FileMode.Create, FileAccess.Write, FileShare.None, 3145728, true))
            {
                await contentStream.CopyToAsync(stream);
            }
        }
    }
}
