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
                OtherPlans = new List<OtherPlan>(),
                BaseMaps = new List<BaseMap>(),
                FileFormat = request.FileFormat,
                HighlightColor = request.ParcelHighlightColor
            };

            // spatial conditions:
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

            // urbanistic plans:
            foreach (UrbanisticPlansResult urbanisticPlan in request.UrbanisticPlansResults)
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
                    PlanIdAttribute = urbanisticPlan.PlanIdAttribute,
                    RasterIdAttribute = urbanisticPlan.RasterIdAttribute,
                    PolygonRestURL = urbanisticPlan.PolygonRestURL,
                    RasterRestURL = urbanisticPlan.RasterRestURL,
                    LegendRestURL = urbanisticPlan.LegendRestURL,
                    ComponentRestURL = urbanisticPlan.ComponentRestURL
                };

                if (urbanisticPlan.PlanMaps != null)
                {
                    foreach (UrbanisticPlansResult.PlanMap planMap in urbanisticPlan.PlanMaps)
                    {
                        planResults.Maps.Add(new Map
                        {
                            Id = planMap.Id,
                            Name = planMap.Name,
                            MapScale = planMap.MapScale,
                            OriginalScale = planMap.OriginalScale,
                        });
                    }
                }
                response.UrbanPlans.Add(planResults);
            }

            // spatial queries:
            if (request.SpatialQueryResults != null)
            {
                foreach (SpatialQueryResult spatialQuery in request.SpatialQueryResults)
                {
                    OtherPlan otherPlan = new OtherPlan()
                    {
                        Id = spatialQuery.Id,
                        Title = spatialQuery.Title,
                        RestUrl = spatialQuery.RestUrl,
                        IdAttribute = spatialQuery.IdAttribute,
                        ResultFeatures = new List<OtherPlan.ResultFeature>()
                    };

                    if (spatialQuery.ResultFeatures != null)
                    {
                        foreach (SpatialQueryResult.ResultFeature spatResult in spatialQuery.ResultFeatures)
                        {
                            otherPlan.ResultFeatures.Add(new OtherPlan.ResultFeature()
                            {
                                Id = spatResult.Id,
                                Status = spatResult.Status,
                                Type = spatResult.Type,
                                Name = spatResult.Name,
                                Sn = spatResult.Sn,
                                MapScale = spatResult.MapScale,
                            });
                        }
                    }

                    response.OtherPlans.Add(otherPlan);
                }
            }

            // base map results:
            if (request.BaseMapResults != null)
            {
                foreach (BaseMapResult baseMapResult in request.BaseMapResults)
                {
                    BaseMap baseMap = new BaseMap()
                    {
                        Id = baseMapResult.Id,
                        Title = baseMapResult.Title,
                        RestUrl = baseMapResult.RestUrl,
                        BaseMapTitle = baseMapResult.BaseMapTitle,
                        ResultFeatures = new List<BaseMap.ResultFeature>(),

                        ServerPath = GetServerName(baseMapResult.RestUrl),

                        PaperSize = new Size()
                        {
                            Width = (int)doc.PageWidth,
                            Height = (int)doc.PageHeight
                        }
                    };

                    if (baseMapResult.ResultFeatures != null)
                    {
                        foreach (BaseMapResult.ResultFeature baseResult in baseMapResult.ResultFeatures)
                        {
                            baseMap.ResultFeatures.Add(new BaseMap.ResultFeature()
                            {
                                Id = baseResult.Id,
                                Type = baseResult.Type,
                                Name = baseResult.Name,
                                MapScale = baseResult.MapScale,
                            });
                        }
                    }

                    response.BaseMaps.Add(baseMap);
                }
            }

            return response;
        }

        public static string GetServerName(string layerURL)
        {
            return layerURL.Substring(0, layerURL.LastIndexOf("/"));
        }
    }
}