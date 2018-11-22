using System;

namespace PGZ.UI.PrintService.Models
{
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