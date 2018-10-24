using ArcGisExportApi.Models;
using ArcGisExportApi.Tests;
using ArcGisExportApi.TestUtils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using static ArcGisExportApi.Models.UrbanisticPlansResults;

namespace ArcGisExportApi.Services
{
    public class ResponseMapper
    {
        public static DataResponse mapToReponse(DataRequest request)
        {
            // Skopirat plan map
            // request.UrbanisticPlansResults[0].PlanMaps;

            List<int> mapPlanIdList = getListOfIds(request.UrbanisticPlansResults[0].PlanMaps);

            DataResponse response = CreateMapPlans(mapPlanIdList);
            AddLegends(response, request.UrbanisticPlansResults[0].LegenRestURL, mapPlanIdList);

            // polygon, raster, legend, component

            return response;
        }

        public static List<int> getListOfIds(List<PlanMap> mapPlans)
        {
            List<int> ids = new List<int>();

            for (int i = 0; i < mapPlans.Count; i++)
            {
                // CHECK - parsing & Id of string value
                ids.Add(int.Parse(mapPlans[i].Id));
            }
            return ids;
        }

        public static DataResponse CreateMapPlans(List<int> ids)
        {
            DataResponse response = new DataResponse();
            for (int i = 0; i < ids.Count; i++)
            {
                response.Maps.Add(new MapObject(ids[i]));
            }
            return response;
        }

        async public static void AddLegends(DataResponse response, string restUrl, List<int> mapPlanIds)
        {
            // legends:
            QueryResult legendInfo = await QueryUtils.queryAll(restUrl, mapPlanIds);
            ExportResultList legendImages = await ExportUtils.getAll(legendInfo, restUrl);

            Trace.WriteLine("\t Query - number of returned objects: " + legendInfo.Features.Count);
            // response, add maps to that

            // TODO - put data in output object (by id) (...)
            // CHECK - saved docs:
            for (int i = 0; i < legendImages.MapPlans.Count; i++)
            {
                // map export, filename .png
                string format = ".png";
                await StreamService.DownloadImage(new Uri(legendImages.MapPlans[i].Href),
                    legendImages.MapPlans[i].Scale + "." + format);
            }
        }


        /*
           public MapImage Nesto { get; set; }
           public MapImage Raster { get; set; }
           public MapImage Legend { get; set; }
           public MapImage Component { get; set; }
           */

    }
}