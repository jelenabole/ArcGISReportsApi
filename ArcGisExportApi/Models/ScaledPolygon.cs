using System;
using System.Collections.Generic;

namespace PGZ.UI.PrintService.Models
{
    public class ScaledPolygon
    {
        public List<ScaledPoint> Points { get; set; }

        public ScaledPolygon ()
        {
            Points = new List<ScaledPoint>();
        }
    }

    public class ScaledPoint
    {
        public int XPoint { get; set; }
        public int YPoint { get; set; }

        public ScaledPoint(double x, double y)
        {
            XPoint = Convert.ToInt32(x);
            YPoint = Convert.ToInt32(y);
        }
    }
}