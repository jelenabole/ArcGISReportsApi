using System;
using System.IO;
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
            // load document template (docx):
            DocX doc = AddTemplate(request.DocumentTemplateId, webRootPath);

            // get all map images and add them to docx:
            MapImageList mapImages = await MapImageService.mapToReponse(doc, request);
            await AddInfo(doc, request, mapImages);
            doc.SaveAs(ms);

            // convert to other formats, if needed:
            if (request.FileFormat == "pdf")
            {
                convertDocxToPdf(ms);
                return "pdf";
            }

            return "docx";
        }

        private static DocX AddTemplate(string templateId, string webRootPath)
        {
            // TODO - find template by id:
            string templateName = "pgzTemplate.docx";

            string path = Path.Combine(webRootPath, "Templates", templateName);
            return DocX.Load(path).Copy();
        }


        async public static Task<DocX> AddInfo(DocX document, DataRequest request,
            MapImageList mapImages)
        {
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

            int firstResUrbIdent = 0;

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
                resPlanUrbPar.InsertTableAfterSelf(table);

                if(firstResUrbIdent == 1)
                {
                    resPlanUrbPar.InsertPageBreakBeforeSelf();
                }

                firstResUrbIdent = 1;

                foreach (PlanMap planMap in resUrbIdent.PlanMaps)
                {
                    MapPlans map = mapImages.GetById(planMap.Id);
                    Paragraph imagesParagraph = document.InsertParagraph((planMap.Name
                        + " " + "MJERILO KARTE 1:" + planMap.MapScale
                        + " " + "IZVORNO MJERILO KARTE 1:" + planMap.OriginalScale));
                    imagesParagraph.InsertPageBreakBeforeSelf();
                    imagesParagraph.AppendPicture(StreamService.convertToImage(document, map.RasterImage).CreatePicture());
                    imagesParagraph.AppendPicture(StreamService.convertToImage(document, map.LegendImage).CreatePicture());
                    imagesParagraph.AppendPicture(StreamService.convertToImage(document, map.ComponentImage).CreatePicture());
                    //imagesParagraph.InsertPageBreakAfterSelf();
                }
            }
            
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
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(15));

            // add cache with pending status:
            DocumentResponse cached = new DocumentResponse();
            _cache.Set(key, cached, policy);

            // create document:
            using (MemoryStream ms = new MemoryStream())
            {
                try
                {
                    string format = await createDocument(request, ms, webRootPath);
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