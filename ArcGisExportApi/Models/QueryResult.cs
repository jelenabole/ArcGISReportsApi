using System.Collections.Generic;

namespace ArcGisExportApi.Tests
{
    // TODO - Geometry already exists in models (request)

    public sealed class QueryResult
    {
        // list of objects by id:
        public List<Feature> Features { get; set; }
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
    }

    public class Geometry
    {
        public List<List<List<double>>> Rings { get; set; }
    }
}