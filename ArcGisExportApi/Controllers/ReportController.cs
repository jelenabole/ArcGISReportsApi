using PGZ.UI.PrintService.Inputs;
using PGZ.UI.PrintService.Services;
using PGZ.UI.PrintService.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System;
using PGZ.UI.PrintService.Responses;
using Newtonsoft.Json;
using System.IO;
using System.Threading.Tasks;

namespace PGZ.UI.PrintService.Controllers
{
    [ApiController]
    public class ReportController : ControllerBase
    {
        private IMemoryCache _cache;
        public ReportController(IMemoryCache memoryCache)
        {
            _cache = memoryCache;
        }


        [HttpGet]
        [Route("[controller]")]
        async public Task<FileStreamResult> Get()
        {
            // test data:
            DataRequest request = DownloadUtils.getData();

            // create document:
            MemoryStream ms = new MemoryStream();
            string format = await DocumentService.createDocument(request, ms);
            ms.Position = 0;

            // send response:
            var file = new FileStreamResult(ms, "application/vnd.openxmlformats-officedocument.wordprocessingml.document")
            {
                FileDownloadName = string.Format("PGZ_test." + format)
            };
            return file;
        }








        [HttpPost]
        [Route("[controller]/submit")]
        public string Submit([FromBody] DataRequest request)
        {
            // TODO - check request mistakes (wrong type, null)
            if (request.FileFormat == null)
            {
                return serializeToJson(new ResponseStatus("File Format Empty"));
            } else
            {
                // generate key, and start file creation:
                string key = Guid.NewGuid().ToString();
                DocumentService.CreateCacheFile(request, _cache, key);

                return serializeToJson(new SubmitResponse(key));
            }
        }


        [HttpGet]
        [Route("[controller]/check/{key}")]
        public string CheckStatus(string key)
        {
            DocumentResponse cached = _cache.Get<DocumentResponse>(key);
            // no key:
            if (cached == null)
            {
                return serializeToJson(new ResponseStatus("No file with that key"));
            }

            if (cached.StatusCode == ResponseStatusCode.OK)
            {
                return serializeToJson(new CheckResponse(
                         "https://" + Request.Host.ToString() + "/report/download/" + key));
            } else if (cached.StatusCode == ResponseStatusCode.PENDING)
            {
                return serializeToJson(new ResponseStatus("Document not ready", cached.StatusCode));
            } else
            {
                // error:
                return serializeToJson(new ResponseStatus(cached.ErrorDescription));
            }
        }

        [HttpGet]
        [Route("[controller]/download/{key}")]
        public FileStreamResult Download(string key)
        {
            DocumentResponse cached = _cache.Get<DocumentResponse>(key);
            if (cached == null)
                return null;

            // create document:
            MemoryStream ms = new MemoryStream(cached.Document);
            ms.Position = 0;

            // send response:
            var file = new FileStreamResult(ms, cached.GetMimeTypeByFormat())
            {
                FileDownloadName = string.Format("PGZ_test." + cached.Format)
            };
            return file;
        }

        private string serializeToJson(object obj)
        {
            return JsonConvert.SerializeObject(obj, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            });
        }

    }
}
