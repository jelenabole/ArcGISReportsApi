using System.Collections.Generic;

namespace ArcGisExportApi.Models
{
    public sealed class DataResponse
    {
        public List<MapObject> Maps { get; set; }

        public DataResponse()
        {
            Maps = new List<MapObject>();
        }

        public MapObject GetById(string id)
        {
            foreach (MapObject map in this.Maps) {
                if (map.Id == id)
                    return map;
            }
            return null;
        }
    }

    public class MapObject
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