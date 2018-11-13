using System.Collections.Generic;

namespace PGZ.UI.PrintService.Models
{
    public sealed class DataResponse
    {
        public List<MapImageList> UrbanPlansImages { get; set; }
        public string TemplatePath { get; set; }

        public DataResponse()
        {
            UrbanPlansImages = new List<MapImageList>();
        }
    }

    public sealed class MapImageList
    {
        public List<MapPolygon> MapPolygons { get; set; }
        public Size PaperSize { get; set; }

        public List<MapPlans> Maps { get; set; }
        public string ServerPath { get; set; }

        public string Status;
        public string Type;
        public string GisCode;
        public string Name;

        public MapImageList()
        {
            Maps = new List<MapPlans>();
            MapPolygons = new List<MapPolygon>();
        }

        public MapPlans GetById(string id)
        {
            foreach (MapPlans map in Maps)
            {
                if (map.Id == id)
                    return map;
            }
            return null;
        }
    }

    public class MapPlans
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string MapScale { get; set; }
        public string OriginalScale { get; set; }

        public MapImage Polygon { get; set; }
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