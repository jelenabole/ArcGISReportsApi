using ArcGisExportApi.Models;
using ArcGisExportApi.Tests;
using ArcGisExportApi.TestUtils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using static ArcGisExportApi.Models.UrbanisticPlansResults;

namespace ArcGisExportApi.Services
{
    public class ResponseMapper
    {
        async public static Task<DataResponse> mapToReponse(DataRequest request)
        {
            List<int> mapPlanIdList = getListOfIds(request.UrbanisticPlansResults[0].PlanMaps);

            DataResponse response = CreateMapPlans(mapPlanIdList);
            bool done = await AddLegends(response, request.UrbanisticPlansResults[0].LegenRestURL, mapPlanIdList);
            done = await AddComponents(response, request.UrbanisticPlansResults[0].ComponentRestURL, mapPlanIdList);
            
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

        /* ADD PARTS (images) */

        async public static Task<bool> AddLegends(DataResponse response, string restUrl, List<int> mapPlanIds)
        {
            // legends:
            QueryResult legendsInfo = await QueryUtils.queryAll(restUrl, mapPlanIds);
            ExportResultList legendImages = await ExportUtils.getAll(legendsInfo, restUrl);

            // response, add maps to that
            // TODO - put data in output object (by id) (...)
            for (int i = 0; i < response.Maps.Count; i++)
            {
                for (int j = 0; j < legendImages.MapPlans.Count; j++)
                {
                    if (response.Maps[i].Id == legendImages.MapPlans[j].Id)
                    {
                        response.Maps[i].Legend = await mapExportedDataToResponse(legendImages.MapPlans[j]);
                        break;
                    }
                }
            }

            return true;
        }

        async public static Task<bool> AddComponents(DataResponse response, string restUrl, List<int> mapPlanIds)
        {
            // components:
            QueryResult componentsInfo = await QueryUtils.queryAll(restUrl, mapPlanIds);
            ExportResultList componentImages = await ExportUtils.getAll(componentsInfo, restUrl);

            for (int i = 0; i < response.Maps.Count; i++)
            {
                for (int j = 0; j < componentImages.MapPlans.Count; j++)
                {
                    if (response.Maps[i].Id == componentImages.MapPlans[j].Id)
                    {
                        response.Maps[i].Component = await mapExportedDataToResponse(componentImages.MapPlans[j]);
                        break;
                    }
                }
            }

            return true;
        }






        async public static Task<MapImage> mapExportedDataToResponse(ExportResult mapPlan)
        {
            MapImage mapImage = new MapImage
            {
                Href = mapPlan.Href,
                // Image = await StreamService.getImageFromUrl(mapPlan.Href), // get image from href
                Scale = mapPlan.Scale
            };
            return mapImage;
        }
    }
}