namespace PGZ.UI.PrintService.Models
{
    public class Extent
    {
        public double Xmin;
        public double Ymin;
        public double Xmax;
        public double Ymax;
        public SpatialReference SpatialReference;
    }

    public class SpatialReference
    {
        public int Wkid;
        public int LatestWkid;
    }

    public sealed class Size
    {
        public int Width;
        public int Height;
    }
}