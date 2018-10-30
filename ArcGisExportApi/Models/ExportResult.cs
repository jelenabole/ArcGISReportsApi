using System;
using System.Collections.Generic;

namespace ArcGisExportApi.Tests
{
    public class ExportResultList
    {
        // list of server results (exports):
        public List<ExportResult> MapPlans { get; set; }
    }

    public class ExportResult
    {
        // TODO - add id, name, links to each: raster, legend, component..
        public string Id { get; set; }

        public String Href { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public Envelope Envelope { get; set; }
        public double Scale { get; set; }
    }

    public class Envelope
    {
        public double Xmin { get; set; }
        public double Ymin { get; set; }
        public double Xmax { get; set; }
        public double Ymax { get; set; }
    }
}