using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using PGZ.UI.PrintService.Models;
using PGZ.UI.PrintService.Inputs;
using static PGZ.UI.PrintService.Inputs.UrbanisticPlansResults;
using Novacode;
using Spire.Doc;
using Microsoft.Extensions.Caching.Memory;
using PGZ.UI.PrintService.Responses;

namespace PGZ.UI.PrintService.Services
{
    class DocumentService
    {
        
        async public static Task<string> createDocument(DataRequest request, MemoryStream ms, string webRootPath)
        {
            // get all map images:
            MapImageList mapImages = await MapImageService.mapToReponse(request);
            AddTemplate(mapImages, request.DocumentTemplateId, webRootPath);

            // create document (docx):
            DocX doc = await createDocx(request, mapImages, ms);
            doc.SaveAs(ms);

            // convert to other formats, if needed:
            if (request.FileFormat == "pdf")
            {
                convertDocxToPdf(ms);
                return "pdf";
            }

            return "docx";
        }

        private static void AddTemplate(MapImageList mapImages, string templateId, string webRootPath)
        {
            // TODO - find template by id:
            string templateName = "pgzTemplate.docx";
            mapImages.TemplatePath = Path.Combine(webRootPath, "Templates", templateName);
        }


        async public static Task<DocX> createDocx(DataRequest request,
            MapImageList mapImages, MemoryStream ms)
        {
            DocX document = DocX.Create(ms);
            // string path2 = Path.Combine(Directory.GetCurrentDirectory(), "Templates", "pgzTemplate.docx");
            DocX docTemplate = DocX.Load(mapImages.TemplatePath);
            document = docTemplate.Copy();

                
            int numSpatialCond = request.SpatialConditionList.Count + 1;
            int numUrbanisticPlanResult = request.UrbanisticPlansResults.Count;
            int i = 1;
            
            String klasa = "klasa";
            String urBroj = "urudžbeni broj";
            String datum = DateTime.Now.ToLongDateString();

            document.ReplaceText("[KLASA]", klasa);
            document.ReplaceText("[UBROJ]", urBroj);
            document.ReplaceText("[DATUM]", datum);

            Novacode.Table katCesticeTable = document.AddTable(numSpatialCond, 3);
            katCesticeTable.Design = TableDesign.LightGrid;
            katCesticeTable.Alignment = Alignment.left;
            katCesticeTable.Rows[0].Cells[0].Paragraphs[0].Append("IZVOR");
            katCesticeTable.Rows[0].Cells[1].Paragraphs[0].Append("VRSTA");
            katCesticeTable.Rows[0].Cells[2].Paragraphs[0].Append("OPIS");

            foreach (SpatialCondition spatial in request.SpatialConditionList)
            {
                katCesticeTable.Rows[i].Cells[0].Paragraphs[0].Append(spatial.Source);
                katCesticeTable.Rows[i].Cells[1].Paragraphs[0].Append(spatial.Type);
                katCesticeTable.Rows[i].Cells[2].Paragraphs[0].Append(spatial.Description);
                i++;
            }

            Paragraph title = document.InsertParagraph("Urbanistička identifikacija".ToUpper());
            title.Alignment = Alignment.center;
            title.SpacingBefore(30d);
            title.SpacingAfter(40d);

            Paragraph katCesticeTitle = document.InsertParagraph("Katastarske čestice".ToUpper());
            katCesticeTitle.Alignment = Alignment.left;
            katCesticeTitle.InsertTableAfterSelf(katCesticeTable);

            Novacode.Table rezUrbIdentTable = document.AddTable(1, 4);
            rezUrbIdentTable.Design = TableDesign.LightGrid;
            rezUrbIdentTable.Alignment = Alignment.center;
            Double spacing = 15;


            Paragraph rezUrbIdentTitle = document.InsertParagraph("Rezultat urbanističke identifikacije".ToUpper());
            rezUrbIdentTitle.Alignment = Alignment.left;
            rezUrbIdentTitle.SpacingBefore(spacing);

            UrbanisticPlansResults lastUrbPlanResult = request.UrbanisticPlansResults[request.UrbanisticPlansResults.Count - 1];

            foreach (UrbanisticPlansResults resUrbIdent in request.UrbanisticPlansResults)
            {
                Paragraph resPlanUrbPar;
                Novacode.Table table = document.AddTable(1, 4);
                table.Design = TableDesign.LightGrid;
                table.Alignment = Alignment.center;
                table.Rows[0].Cells[0].Paragraphs[0].Append(resUrbIdent.Status);
                table.Rows[0].Cells[1].Paragraphs[0].Append(resUrbIdent.Type);
                table.Rows[0].Cells[2].Paragraphs[0].Append(resUrbIdent.Name);
                table.Rows[0].Cells[3].Paragraphs[0].Append(resUrbIdent.GisCode);

                resPlanUrbPar = document.InsertParagraph();
                resPlanUrbPar.InsertTableBeforeSelf(table);
                resPlanUrbPar.InsertPageBreakAfterSelf();


                foreach (PlanMap planMap in resUrbIdent.PlanMaps)
                {
                    foreach (MapPlans map in mapImages.Maps)
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

                            if (lastUrbPlanResult.Id != resUrbIdent.Id)
                            {
                                imagesParagraph.InsertPageBreakAfterSelf();
                            }
                            
                        }
                    }
                }
                
                //resPlanUrbPar.AppendPicture(dataResponse.Maps[0].Legend.Image.CreatePicture());
            }
            

            //document.Save();
            return document;
        }

        async public static void convertDocxToPdf(MemoryStream ms)
        {
            Document document = new Document();
            document.LoadFromStream(ms, FileFormat.Docx);
            ms.Position = 0;
            document.SaveToStream(ms, FileFormat.PDF);
        }


        async public static void CreateCacheFile(DataRequest request, 
            IMemoryCache _cache, string key, string webRootPath)
        {
            // cache options:
            var policy = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(10));

            // add cache with pending status:
            DocumentResponse cached = new DocumentResponse();
            _cache.Set(key, cached, policy);

            // create document:
            using (MemoryStream ms = new MemoryStream())
            {
                try
                {
                    string format = await createDocument(request, ms, webRootPath);
                    ms.Position = 0;
                    cached.Document = ms.ToArray();
                    cached.StatusCode = ResponseStatusCode.OK;
                    cached.Format = request.FileFormat;
                }
                catch (Exception ex)
                {
                    cached.StatusCode = ResponseStatusCode.ERROR;
                    cached.ErrorDescription = ex.Message;
                }
            }
        }
    }
}