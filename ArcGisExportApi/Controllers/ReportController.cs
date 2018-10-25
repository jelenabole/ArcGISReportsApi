using System.Collections.Generic;
using System.Threading.Tasks;
using ArcGisExportApi.Models;
using ArcGisExportApi.Services;
using ArcGisExportApi.TestUtils;
using Microsoft.AspNetCore.Mvc;

namespace ArcGisExportApi.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ReportController : ControllerBase
    {
        // async
        [HttpGet]
        async public Task<ActionResult<IEnumerable<string>>> Get()
        {
            // test data:
            DataRequest request = DownloadUtils.getData();

            // get all data and export images:
            DataResponse mapResponse = await ResponseMapper.mapToReponse(request);

            // TODO - put data in output object (by id) (... Antun)
            await PdfService.createPdf(request, mapResponse);

            return new string[] { "...", "pdf" };
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
