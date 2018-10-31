using System.IO;
using System.Threading.Tasks;
using PGZ.UI.PrintService.Inputs;
using PGZ.UI.PrintService.Services;
using PGZ.UI.PrintService.Utilities;
using Microsoft.AspNetCore.Mvc;

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
        async public Task<FileStreamResult> Post([FromBody] DataRequest request)
        {
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
    }
}
