using Novacode;
using PGZ.UI.PrintService.Inputs;
using PGZ.UI.PrintService.Models;
using PGZ.UI.PrintService.Utilities;
using System.Collections.Generic;

namespace PGZ.UI.PrintService.Mappers
{
    class DataRequestMapper
    {
        public static DataResponse MapToResponse(DocX doc, DataRequest request)
        {
            // create a list of maps:
            DataResponse response = new DataResponse
            {
                Polygons = new List<MapPolygon>(),
                UrbanPlans = new List<UrbanPlan>(),
                FileFormat = request.FileFormat,
                HighlightColor = request.HighlightColor
            };

            // copy polygons (to draw from later) - spatial condition list:
            // and calculate extent from them (for the map extent):
            foreach (SpatialCondition spatialCondition in request.SpatialConditionList)
            {
                response.Polygons.Add(new MapPolygon
                {
                    Source = spatialCondition.Source,
                    Type = spatialCondition.Type,
                    Description = spatialCondition.Description,
                    Points = MapPolygon.AddPoints(spatialCondition.Geometry)
                });
            }
            response.PolygonsExtent = ExportUtils.FindPoints(response.Polygons);

            // go through all urbanistic plans
            foreach (UrbanisticPlansResults urbanisticPlan in request.UrbanisticPlansResults)
            {
                UrbanPlan planResults = new UrbanPlan()
                {
                    Maps = new List<Map>(),

                    Status = urbanisticPlan.Status,
                    Type = urbanisticPlan.Type,
                    GisCode = urbanisticPlan.GisCode,
                    Name = urbanisticPlan.Name,
                    ServerPath = GetServerName(urbanisticPlan.RasterRestURL),
                    PaperSize = new Size()
                    {
                        Width = (int)doc.PageWidth,
                        Height = (int)doc.PageHeight
                    },

                    Id = urbanisticPlan.Id,
                    RasterIdAttribute = urbanisticPlan.RasterIdAttribute,
                    PolygonRestURL = urbanisticPlan.PolygonRestURL,
                    RasterRestURL = urbanisticPlan.RasterRestURL,
                    LegendRestURL = urbanisticPlan.LegendRestURL,
                    ComponentRestURL = urbanisticPlan.ComponentRestURL
                };

                string rasterIdAttribute = urbanisticPlan.RasterIdAttribute;

                foreach (UrbanisticPlansResults.PlanMap planMap in urbanisticPlan.PlanMaps)
                {
                    // create map plan, with id and scales:
                    planResults.Maps.Add(new Map
                    {
                        Id = planMap.Id,
                        Name = planMap.Name,
                        MapScale = planMap.MapScale,
                        OriginalScale = planMap.OriginalScale,
                        // RasterIdAttribute = rasterIdAttribute
                    });
                }
                response.UrbanPlans.Add(planResults);
            }

            return response;
        }

        public static string GetServerName(string layerURL)
        {
            return layerURL.Substring(0, layerURL.LastIndexOf("/"));
        }

    }
}