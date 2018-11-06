using System.Collections.Generic;

namespace PGZ.UI.PrintService.Models
{
    public sealed class MapImageList
    {
        public List<MapPlans> Maps { get; set; }
        public string TemplatePath { get; set; }

        public MapImageList()
        {
            Maps = new List<MapPlans>();
        }

        public MapPlans GetById(string id)
        {
            foreach (MapPlans map in Maps) {
                if (map.Id == id)
                    return map;
            }
            return null;
        }
    }

    public class MapPlans
    {
        public string Id { get; set; }
        public string MapScale { get; set; }
        public string OriginalScale { get; set; }

        public MapImage Polygon { get; set; }
        public MapImage Raster { get; set; }
        public string LegendUrl { get; set; }
        public string ComponentUrl { get; set; }
    }

    public class MapImage
    {
        public string Href { get; set; }
        public double Scale { get; set; }
    }
}