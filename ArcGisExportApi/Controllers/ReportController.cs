using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using PGZ.UI.PrintService.Inputs;
using PGZ.UI.PrintService.Services;
using PGZ.UI.PrintService.Utilities;
using Microsoft.AspNetCore.Mvc;
using Novacode;


namespace PGZ.UI.PrintService.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ReportController : ControllerBase
    {
        [HttpGet]
        async public Task<FileStreamResult> Get()
        {
            // test data:
            DataRequest request = DownloadUtils.getData();

            // create docx:
            MemoryStream ms = new MemoryStream();
            MemoryStream msPdf = new MemoryStream();
            DocX doc = await DocumentService.createDocx(request, ms);
            doc.SaveAs(ms);
            ms.Position = 0;
            //convert docx to pdf
            if (request.FileFormat == "pdf")
            {
                msPdf = await DocumentService.convertDocxToPdf(ms);

                var response = new HttpResponseMessage(HttpStatusCode.OK);
                var file = new FileStreamResult(msPdf, "application/vnd.openxmlformats-officedocument.wordprocessingml.document")
                {
                    FileDownloadName = string.Format("PGZ_test.pdf")
                };

                return file;
            }
            else
            {
                var response = new HttpResponseMessage(HttpStatusCode.OK);
                var file = new FileStreamResult(ms, "application/vnd.openxmlformats-officedocument.wordprocessingml.document")
                {
                    FileDownloadName = string.Format("PGZ_test.docx")
                };

                return file;
            }
            
            
            
        }

        [HttpPost]
        async public Task<FileStreamResult> Post([FromBody] DataRequest request)
        {
            // create pdf:
            MemoryStream ms = new MemoryStream();
            DocX doc = await DocumentService.createDocx(request, ms);
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
}
