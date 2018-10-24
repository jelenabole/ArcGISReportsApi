using System;
using ArcGisExportApi.Models;
using Novacode;
using System.Collections.Generic;

namespace ArcGisExportApi.Services
{
    class PdfService
    {
        public static void createPdf(DataRequest dataRequest, DataResponse dataResponse)
        {
            DocX document = DocX.Create("C:/Primjer.docx");
            int numSpatialCond = dataRequest.SpatialConditionList.Count + 1;
            int numUrbanisticPlanResult = dataRequest.UrbanisticPlansResults.Count;
            double koMbr = 324639;
            int i = 1;
            String novaIzmjera = "DA";
            List<Table> urbPlanResTables = new List<Table> { };


            Table katCesticeTable = document.AddTable(numSpatialCond, 4);
            katCesticeTable.Design = TableDesign.LightGrid;
            katCesticeTable.Alignment = Alignment.center;
            katCesticeTable.Rows[0].Cells[0].Paragraphs[0].Append("KO MBR");
            katCesticeTable.Rows[0].Cells[1].Paragraphs[0].Append("KO NAZIV");
            katCesticeTable.Rows[0].Cells[2].Paragraphs[0].Append("KČ BROJ");
            katCesticeTable.Rows[0].Cells[3].Paragraphs[0].Append("NOVA IZMJERA");

            foreach (SpatialCondition spatial in dataRequest.SpatialConditionList)
            {
                katCesticeTable.Rows[i].Cells[0].Paragraphs[0].Append(koMbr.ToString());
                katCesticeTable.Rows[i].Cells[1].Paragraphs[0].Append(spatial.Description.Substring(2, spatial.Description.IndexOf(",")));
                katCesticeTable.Rows[i].Cells[2].Paragraphs[0].Append(spatial.Description.Substring(spatial.Description.IndexOf(",")));
                katCesticeTable.Rows[i].Cells[1].Paragraphs[0].Append(novaIzmjera);
            }

            Paragraph title = document.InsertParagraph("Urbanistička identifikacija".ToUpper());
            title.Alignment = Alignment.center;
            title.SpacingAfter(40d);

            Paragraph katCesticeTitle = document.InsertParagraph("Katastarske čestice".ToUpper());
            katCesticeTitle.Alignment = Alignment.left;
            katCesticeTitle.InsertTableAfterSelf(katCesticeTable);

            Table rezUrbIdentTable = document.AddTable(numUrbanisticPlanResult, 4);
            rezUrbIdentTable.Design = TableDesign.LightGrid;
            rezUrbIdentTable.Alignment = Alignment.center;
            foreach (UrbanisticPlansResults rezUrbIdent in dataRequest.UrbanisticPlansResults)
            {
                Table table = document.AddTable(0, 4);
                table.Design = TableDesign.LightGrid;
                table.Alignment = Alignment.center;
                table.Rows[0].Cells[0].Paragraphs[0].Append(rezUrbIdent.Status);
                table.Rows[0].Cells[1].Paragraphs[0].Append(rezUrbIdent.Type);
                table.Rows[0].Cells[2].Paragraphs[0].Append(rezUrbIdent.Name);
                table.Rows[0].Cells[3].Paragraphs[0].Append(rezUrbIdent.GisCode);
                urbPlanResTables.Add(table);
            }
            Paragraph rezUrbIdentTitle = document.InsertParagraph("Rezultat urbanističke identifikacije".ToUpper());
            rezUrbIdentTitle.Alignment = Alignment.left;

            document.Save();
        }

    }
}