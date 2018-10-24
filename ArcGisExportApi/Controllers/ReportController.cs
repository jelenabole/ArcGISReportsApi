using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using ArcGisExportApi.Models;
using ArcGisExportApi.Tests;
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

            // legends:
            QueryResult legendInfo = await QueryUtils.queryAll(request.UrbanisticPlansResults[0].LegenRestURL,
                request.UrbanisticPlansResults[0].PlanMaps);
            Trace.WriteLine("\t Query - number of returned objects: " + legendInfo.Features.Count);
            ExportResultList legendImages = await ExportUtils.getAll(legendInfo,
                request.UrbanisticPlansResults[0].LegenRestURL);
            
            // streams:
            string format = ".png";
            await StreamService.DownloadImage(new Uri(legendImages.MapPlans[i].Href),
                legendImages.MapPlans[i].Scale + "." + format);
            
            
            // TODO - put data in output object (by id) (...)
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
