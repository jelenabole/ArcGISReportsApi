using System.Collections.Generic;
using System.Drawing;

namespace ArcGisExportApi.Models
{
    public sealed class DataResponse
    {
        public List<MapObject> Maps { get; set; }

        public DataResponse()
        {
            Maps = new List<MapObject>();
        }
    }

    public class MapObject
    {
        public int Id { get; set; }
        public MapImage Polygon { get; set; }
        public MapImage Raster { get; set; }
        public MapImage Legend { get; set; }
        public MapImage Component { get; set; }

        public MapObject(int id)
        {
            Id = id;
        }
    }

    public class MapImage
    {
        public string Href { get; set; }
        public double Scale { get; set; }
        public Image Image { get; set; }
    }
}