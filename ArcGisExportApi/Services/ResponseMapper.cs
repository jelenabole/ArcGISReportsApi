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

            // create a list of maps:
            DataResponse response = new DataResponse();
            List<string> mapPlanIdList = new List<string>();
            foreach (PlanMap planMap in request.UrbanisticPlansResults[0].PlanMaps)
            {
                // create map plan, with id and scales:
                MapObject map = new MapObject
                {
                    Id = planMap.Id,
                    MapScale = planMap.MapScale,
                    OriginalScale = planMap.OriginalScale
                };
                response.Maps.Add(map);

                mapPlanIdList.Add(planMap.Id);
            }

            // TODO - +polygon
            bool done = await AddRaster(response, request.UrbanisticPlansResults[0].RasterRestURL, mapPlanIdList);
            done = await AddLegends(response, request.UrbanisticPlansResults[0].LegendRestURL, mapPlanIdList);
            done = await AddComponents(response, request.UrbanisticPlansResults[0].ComponentRestURL, mapPlanIdList);

            return response;
        }

        async public static Task<bool> AddRaster(DataResponse response, string restUrl, List<string> mapPlanIds)
        {
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
                response.Maps[i].LegendUrl = exportServer 
                    + ExportUtils.getImageUrl(legendsInfo.Features[i].Geometry, restUrl);
            }

            return true;
        }

        async public static Task<bool> AddComponents(DataResponse response, string restUrl, List<string> mapPlanIds)
        {
            QueryResult componentsInfo = await QueryUtils.queryAll(restUrl, mapPlanIds);

            for (int i = 0; i < response.Maps.Count; i++)
            {
                response.Maps[i].ComponentUrl = exportServer 
                    + ExportUtils.getImageUrl(componentsInfo.Features[i].Geometry, restUrl);
            }

            return true;
        }
    }
}