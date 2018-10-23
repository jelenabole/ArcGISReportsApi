using System.Collections.Generic;
using ArcGisExportApi.Models;
using ArcGisExportApi.TestUtils;
using Microsoft.AspNetCore.Mvc;

namespace ArcGisExportApi.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ReportController : ControllerBase
    {
        // POST [Fromform]
        [HttpPost]
        public ActionResult<IEnumerable<string>> Post([FromBody] DataRequest request)
        {

            // do something

            return new string[] { "value", "value" };
        }
    }
}
