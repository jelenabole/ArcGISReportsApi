using System.Collections.Generic;

namespace PGZ.UI.PrintService.Models
{
    public class MapPolygon
    {
        public List<MapPoint> Points { get; set; }

        public MapPolygon (Inputs.Geometry geometry)
        {
            // get polygon from geometry:
            Points = new List<MapPoint>();
            for (int i = 0; i < geometry.Rings[0].Count; i++)
            {
                Points.Add(new MapPoint(geometry.Rings[0][i][0], geometry.Rings[0][i][1]));
            }
        }
    }

    public class MapPoint
    {
        public double XPoint { get; set; }
        public double YPoint { get; set; }

        public MapPoint(double x, double y)
        {
            XPoint = x;
            YPoint = y;
        }
    }
}