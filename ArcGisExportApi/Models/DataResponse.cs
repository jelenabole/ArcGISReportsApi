using System.Collections.Generic;

namespace PGZ.UI.PrintService.Models
{
    public sealed class DataResponse
    {
        public string FileFormat;
        public string HighlightColor;
        public Extent PolygonsExtent;

        // spatial conditions:
        public List<MapPolygon> Polygons;

        // urban plans:
        public List<UrbanPlan> UrbanPlans;

        // spatial queries:
        public List<OtherPlan> OtherPlans;

        //baseMapResults:
        public List<BaseMap> BaseMaps;
    }

    public sealed class UrbanPlan
    {
        // document:
        public string Status;
        public string Type;
        public string GisCode;
        public string Name;

        // layers:
        public int Id;
        public string PlanIdAttribute;
        public string RasterIdAttribute;
        public string PolygonRestURL;
        public string RasterRestURL;
        public string LegendRestURL;
        public string ComponentRestURL;

        // generated:
        public string ServerPath;
        public List<Map> Maps;
        public Size PaperSize;

        public Map GetMapById(string id)
        {
            foreach (Map map in Maps)
            {
                if (map.Id == id)
                    return map;
            }
            return null;
        }
    }

    public class Map
    {
        // document:
        public string Id;
        public string Name;
        public string MapScale;
        public string OriginalScale;

        // images:
        public Extent FullMapExtent;
        public MapImage Raster;
        public string LegendUrl;
        public string ComponentUrl;

        public byte[] RasterImage;
        public byte[] LegendImage;
        public byte[] ComponentImage;
    }

    public class MapImage
    {
        public string Href;
        public double Scale;
        public int Width;
        public int Height;
        public Extent Extent;
    }

    public class OtherPlan
    {
        public string Id;
        public string Title;
        public string RestUrl;
        public string IdAttribute;
        public List<ResultFeature> ResultFeatures;

        public class ResultFeature
        {
            public string Id;
            public string Status;
            public string Type;
            public string Name;
            public string Sn;
            public string MapScale;
        }
    }

    public class BaseMap
    {
        public string Id;
        public string Title;
        public string RestUrl;
        public string BaseMapTitle;
        public List<ResultFeature> ResultFeatures;

        public class ResultFeature
        {
            public string Id;
            public string Type;
            public string Name;
            public string MapScale;
        }

    }
}