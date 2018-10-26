using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using ArcGisExportApi.Models;
using ArcGisExportApi.Services;
using ArcGisExportApi.TestUtils;
using Microsoft.AspNetCore.Mvc;
using Novacode;

namespace ArcGisExportApi.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ReportController : ControllerBase
    {
        // async
        [HttpGet]
        async public Task<FileStreamResult> Get()
        {
            // test data:
            DataRequest request = DownloadUtils.getData();

            // create pdf:
            using (MemoryStream ms = new MemoryStream())
            {
                DocX doc = await PdfService.createPdf(request, ms);
                doc.SaveAs(ms);
                ms.Position = 0;

                var response = new HttpResponseMessage(HttpStatusCode.OK);
                var file = new FileStreamResult(ms, "application/vnd.openxmlformats-officedocument.wordprocessingml.document")
                {
                    FileDownloadName = string.Format("PGZ_test.docx")
                };

                return file;
            }
        }

        // POST [Fromform]
        [HttpPost]
        public ActionResult<IEnumerable<string>> Post([FromBody] DataRequest request)
        {
            // do something
            return new string[] { "value", "value" };
        }
    }
}
