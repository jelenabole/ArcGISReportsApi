using System.Collections.Generic;

namespace PGZ.UI.PrintService.Models
{
    // TODO - Geometry already exists in models (request)

    public sealed class QueryResult
    {
        // list of objects by id:
        public List<Feature> Features { get; set; }

        public Geometry GetGeometryByMapId(string kartaSifra)
        {
            foreach (Feature feat in Features)
            {
                if (feat.Attributes.Karta_sifra.Equals(kartaSifra)) {
                    return feat.Geometry;
                }
            }
            return null;
        }
    }

    public class Feature
    {
        public Attributes Attributes { get; set; }
        public Geometry Geometry { get; set; }
    }

    public class Attributes
    {
        public int ObjectId { get; set; }
        public string Name { get; set; }
        public string Karta_sifra { get; set; }
    }

    public class Geometry
    {
        public List<List<List<double>>> Rings { get; set; }
    }
}