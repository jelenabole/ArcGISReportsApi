using System;
using ArcGisExportApi.Models;
using Novacode;
using System.Threading.Tasks;
using System.IO;
using ArcGisExportApi.Inputs;
using static ArcGisExportApi.Inputs.UrbanisticPlansResults;

namespace ArcGisExportApi.Services
{
    class DocumentService
    {
        async public static Task<DocX> createDocx(DataRequest dataRequest, MemoryStream ms)
        {
            // get all data and export images:
            DataResponse dataResponse = await ResponseMapper.mapToReponse(dataRequest);

            DocX document = DocX.Create(ms);
            
            DocX docTemplate = DocX.Load(Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
                    + "/" + "template" + "/" + "pgzTemplate.docx");
            //DocX docTemplate = DocX.Load("C:/template.docx");
            document = docTemplate.Copy();
            
                
            int numSpatialCond = dataRequest.SpatialConditionList.Count + 1;
            int numUrbanisticPlanResult = dataRequest.UrbanisticPlansResults.Count;
            int i = 1;
            
            String klasa = "proba";
            String urBroj = "urudžbeni broj";
            String datum = DateTime.Now.ToLongDateString();

            document.ReplaceText("[KLASA]", klasa);
            document.ReplaceText("[UBROJ]", urBroj);
            document.ReplaceText("[DATUM]", datum);
            
            Table katCesticeTable = document.AddTable(numSpatialCond, 3);
            katCesticeTable.Design = TableDesign.LightGrid;
            katCesticeTable.Alignment = Alignment.left;
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
            Double spacing = 15;


            Paragraph rezUrbIdentTitle = document.InsertParagraph("Rezultat urbanističke identifikacije".ToUpper());
            rezUrbIdentTitle.Alignment = Alignment.left;
            rezUrbIdentTitle.SpacingBefore(spacing);

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

                resPlanUrbPar = document.InsertParagraph();
                resPlanUrbPar.InsertTableBeforeSelf(table);

                foreach (PlanMap planMap in resUrbIdent.PlanMaps)
                {
                    foreach (MapObject map in dataResponse.Maps)
                    {
                        if (planMap.Id == map.Id)
                        {
                            Paragraph imagesParagraph = document.InsertParagraph((planMap.Name 
                                + " " + "MJERILO KARTE 1:" + planMap.MapScale
                                + " " + "IZVORNO MJERILO KARTE 1:" + planMap.OriginalScale));

                            Image rasterImage = await StreamService.getImageFromUrl(document, map.Raster.Href);
                            Picture rasterPic = rasterImage.CreatePicture();
                            Console.WriteLine("Width:" + rasterPic.Width + " Height:" + rasterPic.Height);
                            imagesParagraph.AppendPicture(rasterPic);

                            Image legImage = await StreamService.getImageFromUrl(document, map.LegendUrl);
                            Picture legPic = legImage.CreatePicture();
                            imagesParagraph.AppendPicture(legPic);

                            Image compImage = await StreamService.getImageFromUrl(document, map.ComponentUrl);
                            Picture compPic = compImage.CreatePicture();
                            imagesParagraph.AppendPicture(compPic);

                            imagesParagraph.InsertPageBreakAfterSelf();
                        }
                    }
                }
                //resPlanUrbPar.AppendPicture(dataResponse.Maps[0].Legend.Image.CreatePicture());
            }
            

            //document.Save();
            return document;
        }

        async public static Task<MemoryStream> convertDocxToPdf(MemoryStream ms)
        {
            Spire.Doc.Document document = new Spire.Doc.Document();

            return ms;
        }


    }
}