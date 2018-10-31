using PGZ.UI.PrintService.Inputs;
using PGZ.UI.PrintService.Models;
using PGZ.UI.PrintService.Utilities;
using System.Collections.Generic;
using System.Threading.Tasks;
using static PGZ.UI.PrintService.Inputs.UrbanisticPlansResults;

namespace PGZ.UI.PrintService.Services
{
    public class MapImageService
    {
        public static string exportServer = "https://gdiportal.gdi.net/server/rest/services/PGZ/PGZ_UI_QUERY_DATA/MapServer/export";


        async public static Task<MapImageList> mapToReponse(DataRequest request)
        {
            // CHECK - if there are no urban.plan. results checked - return null:
            if (request.UrbanisticPlansResults == null || request.UrbanisticPlansResults.Count == 0)
            {
                return null;
            }

            // create a list of maps:
            MapImageList response = new MapImageList();
            List<string> mapPlanIdList = new List<string>();
            foreach (PlanMap planMap in request.UrbanisticPlansResults[0].PlanMaps)
            {
                // create map plan, with id and scales:
                MapPlans map = new MapPlans
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

        async public static Task<bool> AddRaster(MapImageList response, string restUrl, List<string> mapPlanIds)
        {
            QueryResult rasterInfo = await QueryUtils.queryAll(restUrl, mapPlanIds);

            // response (for scales):
            ExportResultList rasterImages = await ExportUtils.getInfo(response,
                rasterInfo, restUrl);

            // response, add maps to that
            // TODO - put data in output object (by id) (...)
            for (int i = 0; i < response.Maps.Count; i++)
            {
                for (int j = 0; j < rasterImages.MapPlans.Count; j++)
                {
                    if (response.Maps[i].Id == rasterImages.MapPlans[j].Karta_Sifra)
                    {
                        response.Maps[i].Raster = new MapImage
                        {
                            Href = rasterImages.MapPlans[j].Href,
                            Scale = rasterImages.MapPlans[j].Scale
                        };

                        break;
                    }
                }
            }

            return true;
        }





        async public static Task<bool> AddLegends(MapImageList response, string restUrl, List<string> mapPlanIds)
        {
            QueryResult legendsInfo = await QueryUtils.queryAll(restUrl, mapPlanIds);

            for (int i = 0; i < response.Maps.Count; i++)
            {
                response.Maps[i].LegendUrl = exportServer 
                    + ExportUtils.getImageUrl(legendsInfo.Features[i].Geometry, restUrl);
            }

            return true;
        }

        async public static Task<bool> AddComponents(MapImageList response, string restUrl, List<string> mapPlanIds)
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