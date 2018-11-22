using System;
using System.Collections.Generic;

namespace PGZ.UI.PrintService.Models
{
    public class ExportResultList
    {
        // list of server results (exports):
        public List<ExportResult> MapPlans { get; set; }

        public ExportResult GetById(string kartaSifra)
        {
            foreach (ExportResult plan in MapPlans)
            {
                if (plan.Karta_Sifra.Equals(kartaSifra))
                {
                    return plan;
                }
            }
            return null;
        }
    }

    public class ExportResult
    {
        public string Karta_Sifra { get; set; }

        public String Href { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public double Scale { get; set; }
        // real extent of the (final exported) image:
        public Extent Extent { get; set; }
    }
}