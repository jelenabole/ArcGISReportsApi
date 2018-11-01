using PGZ.UI.PrintService.Inputs;
using PGZ.UI.PrintService.Services;
using PGZ.UI.PrintService.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System;
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
        [Route("[controller]/get")]
        public string StartCreatingDocument([FromBody] DataRequest request)
        {
            // generate key, and start file creation:
            string key = Guid.NewGuid().ToString();
            DocumentService.CreateCacheFile(request, _cache, key);

            return key;
        }

        [HttpGet]
        [Route("[controller]/check/{key}")]
        public bool CheckDocumentStatus(string key)
        {
            if (_cache.Get<FileStreamResult>(key) != null)
            {
                return true;
            }
            return false;
        }

        [HttpGet]
        [Route("[controller]/get/{key}")]
        public FileStreamResult GetDocument(string key)
        {
            return _cache.Get<FileStreamResult>(key);
        }

    }
}
