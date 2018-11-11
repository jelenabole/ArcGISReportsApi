using System;
using System.IO;
using System.Threading.Tasks;
using PGZ.UI.PrintService.Models;
using PGZ.UI.PrintService.Inputs;
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
            DataResponse dataResponse = await MapImageService.mapToReponse(doc, request);
            await AddInfo(doc, request, dataResponse);
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


        async public static Task<DocX> AddInfo(DocX document, DataRequest request, DataResponse response)
        {
            // template:
            String klasa = "klasa";
            String urBroj = "urudžbeni broj";
            String datum = DateTime.Now.ToLongDateString();

            document.ReplaceText("[KLASA]", klasa);
            document.ReplaceText("[UBROJ]", urBroj);
            document.ReplaceText("[DATUM]", datum);

            // spatial condition list:
            if (request.SpatialConditionList != null && request.SpatialConditionList.Count != 0)
            {
                Novacode.Table katCesticeTable = document.AddTable(
                    request.SpatialConditionList.Count + 1, 3);
                katCesticeTable.Design = TableDesign.LightGrid;
                katCesticeTable.Alignment = Alignment.left;
                katCesticeTable.Rows[0].Cells[0].Paragraphs[0].Append("IZVOR");
                katCesticeTable.Rows[0].Cells[1].Paragraphs[0].Append("VRSTA");
                katCesticeTable.Rows[0].Cells[2].Paragraphs[0].Append("OPIS");

                int i = 1;
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
            }

            // urbanistic plans results:
            bool firstResUrbIdent = true;
            foreach (MapImageList mapImageList in response.UrbanPlansImages)
            {
                Paragraph rezUrbIdentTitle = document.InsertParagraph(
                    "Rezultat urbanističke identifikacije".ToUpper());
                rezUrbIdentTitle.Alignment = Alignment.left;
                rezUrbIdentTitle.SpacingBefore(15d);

                Novacode.Table table = document.AddTable(1, 4);
                table.Design = TableDesign.LightGrid;
                table.Alignment = Alignment.center;
                table.Rows[0].Cells[0].Paragraphs[0].Append(mapImageList.Status);
                table.Rows[0].Cells[1].Paragraphs[0].Append(mapImageList.Type);
                table.Rows[0].Cells[2].Paragraphs[0].Append(mapImageList.Name);
                table.Rows[0].Cells[3].Paragraphs[0].Append(mapImageList.GisCode);

                rezUrbIdentTitle.InsertTableAfterSelf(table);
                if (!firstResUrbIdent)
                {
                    rezUrbIdentTitle.InsertPageBreakBeforeSelf();
                }
                firstResUrbIdent = false;


                // all maps (with leg and comp) in this urban plan:
                foreach (MapPlans planMap in mapImageList.Maps)
                {
                    Paragraph imagesParagraph = document.InsertParagraph((planMap.Name
                        + " " + "MJERILO KARTE 1:" + planMap.MapScale
                        + " " + "IZVORNO MJERILO KARTE 1:" + planMap.OriginalScale));
                    imagesParagraph.InsertPageBreakBeforeSelf();

                    imagesParagraph.AppendPicture(StreamService.convertToImage(document,
                        planMap.RasterImage).CreatePicture());
                    imagesParagraph.AppendPicture(StreamService.convertToImage(document, 
                        planMap.LegendImage).CreatePicture());
                    imagesParagraph.AppendPicture(StreamService.convertToImage(document,
                        planMap.ComponentImage).CreatePicture());
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