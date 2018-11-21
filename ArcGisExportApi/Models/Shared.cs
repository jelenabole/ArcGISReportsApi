namespace PGZ.UI.PrintService.Models
{
    public class Extent
    {
        public double Xmin { get; set; }
        public double Ymin { get; set; }
        public double Xmax { get; set; }
        public double Ymax { get; set; }

        public SpatialReference SpatialReference { get; set; }
    }

    public class SpatialReference
    {
        public int Wkid { get; set; }
        public int LatestWkid { get; set; }
    }



    public sealed class Size
    {
        public int Width { get; set; }
        public int Height { get; set; }
    }
}