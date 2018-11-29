using System.Collections.Generic;

namespace PGZ.UI.PrintService.Inputs
{
    public sealed class DataRequest
    {
        public string FileFormat;
        public string DocumentTemplateId;
        public string ParcelHighlightColor;

        public List<SpatialCondition> SpatialConditionList;
        public List<UrbanisticPlansResult> UrbanisticPlansResults;
    }

    public class SpatialCondition
    {
        public string Source;
        public string Type;
        public string Description;
        public Geometry Geometry;
    }

    public class Geometry
    {
        public List<List<List<double>>> Rings;
    }

    public class UrbanisticPlansResult
    {
        public int Id;
        public string Status;
        public string Type;
        public string GisCode;
        public string Name;
        public string Sn;
        public string RasterIdAttribute;

        // urls:
        public string PolygonRestURL;
        public string RasterRestURL;
        public string LegendRestURL;
        public string ComponentRestURL;

        public List<PlanMap> PlanMaps;

        public class PlanMap
        {
            public string Id;
            public string Name;
            public string MapScale;
            public string OriginalScale;
        }
    }
}