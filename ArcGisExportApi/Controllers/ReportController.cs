using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using ArcGisExportApi.Inputs;
using ArcGisExportApi.Services;
using ArcGisExportApi.Utilities;
using Microsoft.AspNetCore.Mvc;
using Novacode;
using Spire.Doc;

namespace ArcGisExportApi.Controllers
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
            DocX doc = await DocumentService.createDocx(request, ms);
            doc.SaveAs(ms);
            ms.Position = 0;

            //convert docx to pdf
            /*
            Document document = new Document();
            document.LoadFromStream(ms, FileFormat.Docx);
            MemoryStream msPdf = new MemoryStream();
            document.SaveToStream(msPdf, FileFormat.PDF);
            msPdf.Position = 0;
            */
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            var file = new FileStreamResult(ms, "application/vnd.openxmlformats-officedocument.wordprocessingml.document")
            {
                FileDownloadName = string.Format("PGZ_test.docx")
            };

            return file;
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
