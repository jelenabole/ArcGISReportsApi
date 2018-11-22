using System.Collections.Generic;

namespace PGZ.UI.PrintService.Models
{
    public sealed class DataResponse
    {
        public string FileFormat { get; set; }
        // add file format MIME type by the format type

        public string HighlightColor { get; set; }

        // spatial condition list
        public List<MapPolygon> Polygons { get; set; }
        public Extent PolygonsExtent { get; set; }

        // first copy data:
        public List<UrbanPlan> UrbanPlans { get; set; }
    }


    // info
    public sealed class UrbanPlan
    {
        // public List<MapPolygon> MapPolygons { get; set; }
        public List<Map> Maps { get; set; }
        public string ServerPath { get; set; }
        public Size PaperSize { get; set; }

        // prepisani podaci - za mape:
        public int Id;
        public string RasterIdAttribute;
        public string PolygonRestURL;
        public string RasterRestURL;
        public string LegendRestURL;
        public string ComponentRestURL;

        // za dokument:
        public string Status;
        public string Type;
        public string GisCode;
        public string Name;

        public Map GetById(string id)
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
        public string Id { get; set; }
        public string Name { get; set; }
        public string MapScale { get; set; }
        public string OriginalScale { get; set; }

        // public string RasterIdAttribute { get; set; }

        public Extent FullMapExtent { get; set; }
        public MapImage Raster { get; set; }

        public string LegendUrl { get; set; }
        public string ComponentUrl { get; set; }

        public byte[] RasterImage { get; set; }
        public byte[] LegendImage { get; set; }
        public byte[] ComponentImage { get; set; }
    }

    public class MapImage
    {
        public string Href { get; set; }
        public double Scale { get; set; }

        public int Width { get; set; }
        public int Height { get; set; }
        public Extent Extent { get; set; }
    }
}