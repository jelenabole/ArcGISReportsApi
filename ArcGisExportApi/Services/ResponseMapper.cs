using ArcGisExportApi.Inputs;
using ArcGisExportApi.Models;
using ArcGisExportApi.Utilities;
using System.Collections.Generic;
using System.Threading.Tasks;
using static ArcGisExportApi.Inputs.UrbanisticPlansResults;

namespace ArcGisExportApi.Services
{
    public class ResponseMapper
    {
        public static string exportServer = "https://gdiportal.gdi.net/server/rest/services/PGZ/PGZ_UI_QUERY_DATA/MapServer/export";


        async public static Task<DataResponse> mapToReponse(DataRequest request)
        {
            // CHECK - if there are no urban.plan. results checked - return null:
            if (request.UrbanisticPlansResults == null || request.UrbanisticPlansResults.Count == 0)
            {
                return null;
            }

            List<string> mapPlanIdList = getListOfIds(request.UrbanisticPlansResults[0].PlanMaps);
            DataResponse response = CreateMapPlans(mapPlanIdList);

            // + polygon
            bool done = await AddRaster(response, request.UrbanisticPlansResults[0].RasterRestURL, mapPlanIdList);
            done = await AddLegends(response, request.UrbanisticPlansResults[0].LegendRestURL, mapPlanIdList);
            done = await AddComponents(response, request.UrbanisticPlansResults[0].ComponentRestURL, mapPlanIdList);

            return response;
        }

        public static List<string> getListOfIds(List<PlanMap> mapPlans)
        {
            List<string> ids = new List<string>();

            for (int i = 0; i < mapPlans.Count; i++)
            {
                // CHECK - parsing & Id of string value
                ids.Add(mapPlans[i].Id);
            }
            return ids;
        }

        public static DataResponse CreateMapPlans(List<string> ids)
        {
            DataResponse response = new DataResponse();
            for (int i = 0; i < ids.Count; i++)
            {
                response.Maps.Add(new MapObject(ids[i]));
            }
            return response;
        }



        async public static Task<bool> AddRaster(DataResponse response, string restUrl, List<string> mapPlanIds)
        {
            // legends:
            QueryResult rasterInfo = await QueryUtils.queryAll(restUrl, mapPlanIds);
            ExportResultList rasterImages = await ExportUtils.getInfo(rasterInfo, restUrl);

            // response, add maps to that
            // TODO - put data in output object (by id) (...)
            for (int i = 0; i < response.Maps.Count; i++)
            {
                for (int j = 0; j < rasterImages.MapPlans.Count; j++)
                {
                    if (response.Maps[i].Id == rasterImages.MapPlans[j].Karta_Sifra)
                    {
                        response.Maps[i].Raster = mapExportedDataToResponse(rasterImages.MapPlans[j]);
                        break;
                    }
                }
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








        async public static Task<bool> AddLegends(DataResponse response, string restUrl, List<string> mapPlanIds)
        {
            QueryResult legendsInfo = await QueryUtils.queryAll(restUrl, mapPlanIds);

            for (int i = 0; i < response.Maps.Count; i++)
            {
                string url = exportServer + ExportUtils.getImageUrl(legendsInfo.Features[i].Geometry, restUrl);
                response.Maps[i].Legend = new MapImage
                {
                    Href = url
                };
            }

            return true;
        }

        async public static Task<bool> AddComponents(DataResponse response, string restUrl, List<string> mapPlanIds)
        {
            QueryResult componentsInfo = await QueryUtils.queryAll(restUrl, mapPlanIds);

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
    }
}