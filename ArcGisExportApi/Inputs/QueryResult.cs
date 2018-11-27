using System.Collections.Generic;

namespace PGZ.UI.PrintService.Models
{
    public sealed class QueryResult
    {
        public List<Feature> Features;

        public Geometry GetGeometryByMapId(string kartaSifra)
        {
            foreach (Feature feat in Features)
            {
                if (feat.Attributes.Karta_sifra.Equals(kartaSifra))
                {
                    return feat.Geometry;
                }
            }
            return null;
        }
    }

    public class Feature
    {
        public Attributes Attributes;
        public Geometry Geometry;
    }

    public class Attributes
    {
        public int ObjectId;
        public string Name;
        public string Karta_sifra;
    }

    public class Geometry
    {
        public List<List<List<double>>> Rings;
    }
}