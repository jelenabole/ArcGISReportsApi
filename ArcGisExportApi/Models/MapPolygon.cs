using System.Collections.Generic;

namespace PGZ.UI.PrintService.Models
{
    public class MapPolygon
    {
        public string Source { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }

        public List<MapPoint> Points { get; set; }

        // get polygon from geometry:
        public static List<MapPoint> AddPoints(Inputs.Geometry geometry)
        {
            List < MapPoint> Points = new List<MapPoint>();
            for (int i = 0; i < geometry.Rings[0].Count; i++)
            {
                Points.Add(new MapPoint {
                    XPoint = geometry.Rings[0][i][0],
                    YPoint = geometry.Rings[0][i][1]
                });

            }
            return Points;
        }
    }

    public class MapPoint
    {
        public double XPoint { get; set; }
        public double YPoint { get; set; }
    }
}