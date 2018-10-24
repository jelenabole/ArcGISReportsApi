using ArcGisExportApi.Models;
using ArcGisExportApi.Services;
using Newtonsoft.Json;
using Novacode;
using System;
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
        public static void downloadPdf(StreamService stream)
        {
            //System.Console.WriteLine("docx");
            String dest = "C:/Sample.docx";

            DocX document = DocX.Create("C:/Input.docx");
            System.Console.WriteLine("done");


            Image image = document.AddImage(stream, "image/png");
            // Set Picture Height and Width.
            var picture = image.CreatePicture();


            var t = document.AddTable(7, 4);
            t.Design = TableDesign.LightGrid;
            t.Alignment = Alignment.center;
            t.Rows[0].Cells[0].Paragraphs[0].Append("KO MBR");
            t.Rows[0].Cells[1].Paragraphs[0].Append("KO NAZIV");
            t.Rows[0].Cells[2].Paragraphs[0].Append("KČ BROJ");
            t.Rows[0].Cells[3].Paragraphs[0].Append("NOVA IZMJERA");
            t.Rows[1].Cells[0].Paragraphs[0].Append("324639");
            t.Rows[1].Cells[1].Paragraphs[0].Append("Kraljevica");
            t.Rows[1].Cells[2].Paragraphs[0].Append("345");
            t.Rows[1].Cells[3].Paragraphs[0].Append("DA");
            t.Rows[2].Cells[0].Paragraphs[0].Append("324639");
            t.Rows[2].Cells[1].Paragraphs[0].Append("Kraljevica");
            t.Rows[2].Cells[2].Paragraphs[0].Append("346");
            t.Rows[2].Cells[3].Paragraphs[0].Append("DA");
            t.Rows[3].Cells[0].Paragraphs[0].Append("324639");
            t.Rows[3].Cells[1].Paragraphs[0].Append("Kraljevica");
            t.Rows[3].Cells[2].Paragraphs[0].Append("347");
            t.Rows[3].Cells[3].Paragraphs[0].Append("DA");
            t.Rows[4].Cells[0].Paragraphs[0].Append("324639");
            t.Rows[4].Cells[1].Paragraphs[0].Append("Kraljevica");
            t.Rows[4].Cells[2].Paragraphs[0].Append("348");
            t.Rows[4].Cells[3].Paragraphs[0].Append("DA");
            t.Rows[5].Cells[0].Paragraphs[0].Append("324639");
            t.Rows[5].Cells[1].Paragraphs[0].Append("Kraljevica");
            t.Rows[5].Cells[2].Paragraphs[0].Append("349/1");
            t.Rows[5].Cells[3].Paragraphs[0].Append("DA");
            t.Rows[6].Cells[0].Paragraphs[0].Append("324639");
            t.Rows[6].Cells[1].Paragraphs[0].Append("Kraljevica");
            t.Rows[6].Cells[2].Paragraphs[0].Append("349/3");
            t.Rows[6].Cells[3].Paragraphs[0].Append("DA");

            // Add a row at the end of the table and sets its values.
            //var r = t.InsertRow();
            //r.Cells[0].Paragraphs[0].Append("Mario");
            //r.Cells[1].Paragraphs[0].Append("54");
            // Insert a new Paragraph into the document.
            var par = document.InsertParagraph("Urbanistička identifikacija za područje:");
            par.Alignment = Alignment.center;
            par.SpacingAfter(40d);
            // Insert the Table after the Paragraph.
            par.InsertTableAfterSelf(t);

            var p = document.InsertParagraph("Test ČĆŠĐŽ čćšđž");
            p.AppendPicture(picture);
            document.AddImage("C:/map_image.png");

            document.SaveAs("D:/Test.docx");

        }


    }
}
