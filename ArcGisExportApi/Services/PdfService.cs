using System;
using ArcGisExportApi.Models;
using Novacode;
using System.Collections.Generic;
using System.Threading.Tasks;
using static ArcGisExportApi.Models.UrbanisticPlansResults;

namespace ArcGisExportApi.Services
{
    class PdfService
    {
        async public static Task<DocX> createPdf(DataRequest dataRequest, DataResponse dataResponse)
        {
            DocX document = DocX.Create("C:/Primjer.docx");
            int numSpatialCond = dataRequest.SpatialConditionList.Count + 1;
            int numUrbanisticPlanResult = dataRequest.UrbanisticPlansResults.Count;
            int i = 1;
            List<Table> urbPlanResTables = new List<Table> { };
            List<Paragraph> urbPlanResParagraphs = new List<Paragraph> { };


            Table katCesticeTable = document.AddTable(numSpatialCond, 3);
            katCesticeTable.Design = TableDesign.LightGrid;
            katCesticeTable.Alignment = Alignment.center;
            katCesticeTable.Rows[0].Cells[0].Paragraphs[0].Append("IZVOR");
            katCesticeTable.Rows[0].Cells[1].Paragraphs[0].Append("VRSTA");
            katCesticeTable.Rows[0].Cells[2].Paragraphs[0].Append("OPIS");

            foreach (SpatialCondition spatial in dataRequest.SpatialConditionList)
            {
                katCesticeTable.Rows[i].Cells[0].Paragraphs[0].Append(spatial.Source);
                katCesticeTable.Rows[i].Cells[1].Paragraphs[0].Append(spatial.Type);
                katCesticeTable.Rows[i].Cells[2].Paragraphs[0].Append(spatial.Description);
                i++;
            }

            Paragraph title = document.InsertParagraph("Urbanistička identifikacija".ToUpper());
            title.Alignment = Alignment.center;
            title.SpacingAfter(40d);

            Paragraph katCesticeTitle = document.InsertParagraph("Katastarske čestice".ToUpper());
            katCesticeTitle.Alignment = Alignment.left;
            katCesticeTitle.InsertTableAfterSelf(katCesticeTable);

            Table rezUrbIdentTable = document.AddTable(1, 4);
            rezUrbIdentTable.Design = TableDesign.LightGrid;
            rezUrbIdentTable.Alignment = Alignment.center;
            List<Paragraph> resPlanUrb = new List<Paragraph> { };


            Paragraph rezUrbIdentTitle = document.InsertParagraph("Rezultat urbanističke identifikacije".ToUpper());
            rezUrbIdentTitle.Alignment = Alignment.left;

            Console.WriteLine("Broj urb planova:" + dataRequest.UrbanisticPlansResults.Count);
            foreach (UrbanisticPlansResults resUrbIdent in dataRequest.UrbanisticPlansResults)
            {
                Paragraph resPlanUrbPar;
                Table table = document.AddTable(1, 4);
                table.Design = TableDesign.LightGrid;
                table.Alignment = Alignment.center;
                table.Rows[0].Cells[0].Paragraphs[0].Append(resUrbIdent.Status);
                table.Rows[0].Cells[1].Paragraphs[0].Append(resUrbIdent.Type);
                table.Rows[0].Cells[2].Paragraphs[0].Append(resUrbIdent.Name);
                table.Rows[0].Cells[3].Paragraphs[0].Append(resUrbIdent.GisCode);

                resPlanUrbPar = document.InsertParagraph(resUrbIdent.PlanMaps[0].Name + " " + "MJERILO KARTE 1:"
                    + resUrbIdent.PlanMaps[0].MapScale.ToString() + "" + "IZVORNO MJERILO KARTE 1:" + resUrbIdent.PlanMaps[0].OriginalScale.ToString());
                resPlanUrbPar.InsertTableBeforeSelf(table);

                Console.WriteLine("Broj karata data response: " + dataResponse.Maps.Count);
                Console.WriteLine("Broj karata: " + resUrbIdent.PlanMaps.Count);
                foreach (PlanMap planMap in resUrbIdent.PlanMaps)
                {
                    foreach (MapObject map in dataResponse.Maps)
                    {
                        if (planMap.Id == map.Id.ToString())
                        {
                            Paragraph imagesParagraph = document.InsertParagraph(("Id plana: " + map.Id + "\n").ToUpper());
                            Image legImage = await StreamService.getImageFromUrl(document, map.Legend.Href);

                            Picture pic = legImage.CreatePicture();
                            Console.WriteLine("Visina: " + pic.Height + "SIrina: " + pic.Width);
                            
                            imagesParagraph.AppendPicture(legImage.CreatePicture());
                            Image compImage = await StreamService.getImageFromUrl(document, map.Component.Href);
                            imagesParagraph.AppendPicture(compImage.CreatePicture());
                        }
                    }
                }

                //resPlanUrbPar.AppendPicture(dataResponse.Maps[0].Legend.Image.CreatePicture());

            }
            
            //rezUrbIdentTitle.InsertTableAfterSelf(table);

            


            document.Save();
            return document;
        }

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

            //Paragraph par = document.

            document.SaveAs("D:/Test.docx");

        }

    }
}