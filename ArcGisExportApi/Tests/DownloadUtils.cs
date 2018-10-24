using ArcGisExportApi.Models;
using ArcGisExportApi.Services;
using Newtonsoft.Json;
using Novacode;
using System;
using System.Collections.Generic;
using System.IO;

namespace ArcGisExportApi.TestUtils
{
    public class DownloadUtils
    {
        // TODO - maknut ovo:
        private static QueryService queryService = new QueryService();
        private static ExportService mapService = new ExportService();
        // int index = 0; // temp

        public static DataRequest getData()
        {
            return DeserializeJsonFile();
        }

        private static DataRequest DeserializeJsonFile()
        {
            // stream from a file:
            var serializer = new JsonSerializer();
            string str = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
                    + "\\" + "copy.json";

            // sent text instead of stream
            using (var sr = new StreamReader(str))
            using (var jsonTextReader = new JsonTextReader(sr))
            {
                return (DataRequest)serializer.Deserialize(sr, typeof(DataRequest));
            }
        }

        /*
        async public static void downloadAll(QueryResult queryResult, string uriLayer)
        {
            for (int i = 0; i < queryResult.Features.Count; i++)
            {
                // calculate geometry of the first legend:
                Extent legendExtent = CalcUtils.FindPoints(queryResult.Features[i].Geometry);

                string linkMap = "?f=json" + AddBoundingBox(legendExtent)
                    + CalculateSizeToCrop(legendExtent)
                    // Layer removed - cause its green, while the legend is on the 21th layer:
                    + AddLayerGroup(uriLayer);

                Trace.WriteLine("link for legend:");
                Trace.WriteLine(linkMap);
                MapExport exportedLegend = await mapService.GetMapExport(linkMap);

                if (exportedLegend.Href != null)
                {
                    string name = legends.Features[i].Attributes.Name ?? "map_" + index;
                    index++;
                    SaveImage(exportedLegend, name, "png");

                }
            }
        }
        */
        public static void downloadPdf()
        {
            //System.Console.WriteLine("docx");
            String dest = "C:/Sample.docx";

            DocX document = DocX.Create("C:/Input.docx");
            System.Console.WriteLine("done");


            Image image = document.AddImage("C:/map_image.png");
            // Set Picture Height and Width.
            var picture = image.CreatePicture();


            var katCesticeTable = document.AddTable(1, 4);
            katCesticeTable.Design = TableDesign.LightGrid;
            katCesticeTable.Alignment = Alignment.center;
            katCesticeTable.Rows[0].Cells[0].Paragraphs[0].Append("KO MBR");
            katCesticeTable.Rows[0].Cells[1].Paragraphs[0].Append("KO NAZIV");
            katCesticeTable.Rows[0].Cells[2].Paragraphs[0].Append("KČ BROJ");
            katCesticeTable.Rows[0].Cells[3].Paragraphs[0].Append("NOVA IZMJERA");
            // Add a row at the end of the table and sets its values.
            //var r = t.InsertRow();
            //r.Cells[0].Paragraphs[0].Append("Mario");
            //r.Cells[1].Paragraphs[0].Append("54");
            // Insert a new Paragraph into the document.
            var title = document.InsertParagraph("Urbanistička identifikacija".ToUpper());
            title.Alignment = Alignment.center;
            title.SpacingAfter(40d);

            Paragraph katCesticeTitle = document.InsertParagraph("Katastarske čestice".ToUpper());
            katCesticeTitle.Alignment = Alignment.left;
            katCesticeTitle.InsertTableAfterSelf(katCesticeTable);

            Paragraph par = document.

            document.SaveAs("D:/Test.docx");

        }

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
            
            foreach(SpatialCondition spatial in dataRequest.SpatialConditionList)
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
