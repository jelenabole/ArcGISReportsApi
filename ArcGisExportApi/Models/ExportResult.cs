using System;
using System.Collections.Generic;

namespace PGZ.UI.PrintService.Models
{
    public class ExportResultList
    {
        // list of server results (exports):
        public List<ExportResult> MapPlans { get; set; }
    }

    public class ExportResult
    {
        // karta_sifra:
        public string Id { get; set; }

        public String Href { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public double Scale { get; set; }
        // real extent of the (final exported) image:
        public Extent Extent { get; set; }
    }
}