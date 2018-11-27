using System;
using System.Collections.Generic;

namespace PGZ.UI.PrintService.Models
{
    public class ScaledPolygon
    {
        public List<ScaledPoint> Points;

        public ScaledPolygon()
        {
            Points = new List<ScaledPoint>();
        }

        public void AddPoint(double x, double y)
        {
            Points.Add(new ScaledPoint
            {
                XPoint = Convert.ToInt32(x),
                YPoint = Convert.ToInt32(y)
            });
        }
    }

    public class ScaledPoint
    {
        public int XPoint;
        public int YPoint;
    }
}