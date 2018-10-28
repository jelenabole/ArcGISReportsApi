using ArcGisExportApi.Models;
using ArcGisExportApi.Tests;
using ArcGisExportApi.Utilities;
using System.Collections.Generic;
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

            bool done = await AddRaster(response, request.UrbanisticPlansResults[0].RasterRestURL, mapPlanIdList);
            done = await AddLegends(response, request.UrbanisticPlansResults[0].LegenRestURL, mapPlanIdList);
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



        async public static Task<bool> AddRaster(DataResponse response, string restUrl, List<int> mapPlanIds)
        {
            // legends:
            QueryResult rasterInfo = await QueryUtils.queryAll(restUrl, mapPlanIds);
            ExportResultList rasterImages = await ExportUtils.getAll(rasterInfo, restUrl);

            // response, add maps to that
            // TODO - put data in output object (by id) (...)
            for (int i = 0; i < response.Maps.Count; i++)
            {
                for (int j = 0; j < rasterImages.MapPlans.Count; j++)
                {
                    if (response.Maps[i].Id == rasterImages.MapPlans[j].Id)
                    {
                        response.Maps[i].Raster = mapExportedDataToResponse(rasterImages.MapPlans[j]);
                        break;
                    }
                }
            }

            return true;
        }


        async public static Task<bool> AddLegends(DataResponse response, string restUrl, List<int> mapPlanIds)
        {
            // legends:
            QueryResult legendsInfo = await QueryUtils.queryAll(restUrl, mapPlanIds);
            ExportResultList legendImages = await ExportUtils.getAll(legendsInfo, restUrl);

            for (int i = 0; i < response.Maps.Count; i++)
            {
                for (int j = 0; j < legendImages.MapPlans.Count; j++)
                {
                    if (response.Maps[i].Id == legendImages.MapPlans[j].Id)
                    {
                        response.Maps[i].Legend = mapExportedDataToResponse(legendImages.MapPlans[j]);
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
            // ExportResultList componentImages = await ExportUtils.getAll(componentsInfo, restUrl);
            string exportServer = "https://gdiportal.gdi.net/server/rest/services/PGZ/PGZ_UI_QUERY_DATA/MapServer/export";

            for (int i = 0; i < response.Maps.Count; i++)
            {
                string url = exportServer + ExportUtils.getImageUrl(componentsInfo.Features[i].Geometry, restUrl);
                response.Maps[i].Component = new MapImage
                {
                    Href = url
                };

            }

            return true;
        }


        public static MapImage mapExportedDataToResponse(ExportResult mapPlan)
        {
            MapImage mapImage = new MapImage
            {
                Href = mapPlan.Href,
                Scale = mapPlan.Scale
            };
            return mapImage;
        }
    }
}