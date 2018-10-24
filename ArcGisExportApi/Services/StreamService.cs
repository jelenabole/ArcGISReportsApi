using System;
using System.Drawing;
using System.IO;
using System.Net;
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

        public static async Task<Image> getImageFromUrl(string uri)
        {
            Uri requestUri = new Uri(uri);
            using (WebClient webClient = new WebClient())
            {
                byte[] data = webClient.DownloadData(requestUri);
                using (MemoryStream mem = new MemoryStream(data))
                {
                    using (Image yourImage = Image.FromStream(mem))
                    {
                        return yourImage;
                    }
                }
            }
        }
    }
}
