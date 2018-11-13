using System;
using Novacode;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace PGZ.UI.PrintService.Services
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

        // used for get image from url (in doc):
        public static async Task<Image> getImageFromUrl(DocX doc, string uri)
        {
            Uri requestUri = new Uri(uri);
            using (WebClient webClient = new WebClient())
            {
                byte[] data = webClient.DownloadData(requestUri);
                using (MemoryStream mem = new MemoryStream(data))
                {
                    Image image = doc.AddImage(mem);
                    return image;
                }
            }
        }

        // used for get image from url (in doc):
        public static async Task<byte[]> getImageFromUrlAsync(string uri)
        {
            Uri requestUri = new Uri(uri);
            using (WebClient webClient = new WebClient())
            {
                byte[] data = webClient.DownloadData(requestUri);
                return data;
            }
        }

        // used for get image from url (in doc):
        public static Image convertToImage(DocX doc, byte[] bytes)
        {
            using (MemoryStream mem = new MemoryStream(bytes))
            {
                Image image = doc.AddImage(mem);
                return image;
            }
        }

    }
}
