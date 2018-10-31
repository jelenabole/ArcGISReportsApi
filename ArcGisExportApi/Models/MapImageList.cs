using System.Collections.Generic;

namespace PGZ.UI.PrintService.Models
{
    public sealed class MapImageList
    {
        public List<MapImageList> Maps { get; set; }

        public MapImageList()
        {
            Maps = new List<MapImageList>();
        }

        public MapImageList GetById(string id)
        {
            foreach (MapImageList map in Maps) {
                if (map.Id == id)
                    return map;
            }
            return null;
        }
    }

    public class MapImageList
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