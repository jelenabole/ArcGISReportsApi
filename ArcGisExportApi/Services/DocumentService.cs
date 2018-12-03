using System;
using System.IO;
using System.Threading.Tasks;
using PGZ.UI.PrintService.Models;
using PGZ.UI.PrintService.Inputs;
using PGZ.UI.PrintService.Responses;
using PGZ.UI.PrintService.Mappers;
using Microsoft.Extensions.Caching.Memory;
using Novacode;
using Spire.Doc;
using PdfSharp.Pdf;
using PdfSharp.Drawing;
using PdfSharp.Pdf.IO;

namespace PGZ.UI.PrintService.Services
{
    class DocumentService
    {
        async public static Task createDocument(DataRequest request, MemoryStream ms, string webRootPath)
        {
            // load document template (docx):
            DocX doc = AddTemplate(request.DocumentTemplateId, webRootPath);

            // get all map images and add them to docx:
            DataResponse dataResponse = DataRequestMapper.MapToResponse(doc, request);
            await MapImageService.AddExportedData(doc, dataResponse);
            AddInfo(doc, dataResponse);
            doc.SaveAs(ms);

            // convert to other formats, if needed:
            if (request.FileFormat == "pdf")
            {
                convertDocxToPdf(ms);
            }
        }

        private static DocX AddTemplate(string templateId, string webRootPath)
        {
            // TODO - find template by id:
            string templateName = "pgzTemplate.docx";

            string path = Path.Combine(webRootPath, "Templates", templateName);
            return DocX.Load(path).Copy();
        }


        public static void AddInfo(DocX document, DataResponse response)
        {
            // template:
            String klasa = "klasa";
            String urBroj = "urudžbeni broj";
            String datum = DateTime.Now.ToLongDateString();

            document.ReplaceText("[KLASA]", klasa);
            document.ReplaceText("[UBROJ]", urBroj);
            document.ReplaceText("[DATUM]", datum);

            Paragraph title = document.InsertParagraph("Urbanistička identifikacija".ToUpper());
            title.Alignment = Alignment.center;
            title.SpacingBefore(30d);
            title.SpacingAfter(25d);

            float[] tableWidthKatCestice = { 100F, 100F, 100F };

            // spatial conditions:
            if (response.Polygons != null && response.Polygons.Count != 0)
            {
                Novacode.Table katCesticeTable = document.AddTable(
                    response.Polygons.Count + 1, 3);
                katCesticeTable.Design = TableDesign.TableGrid;
                katCesticeTable.Alignment = Alignment.center;
                katCesticeTable.SetWidthsPercentage(tableWidthKatCestice, null);
                katCesticeTable.Rows[0].Cells[0].Paragraphs[0].Append("IZVOR");
                katCesticeTable.Rows[0].Cells[0].FillColor = System.Drawing.Color.LightGray;
                katCesticeTable.Rows[0].Cells[0].Paragraphs[0].Alignment = Alignment.center;
                katCesticeTable.Rows[0].Cells[1].Paragraphs[0].Append("VRSTA");
                katCesticeTable.Rows[0].Cells[1].FillColor = System.Drawing.Color.LightGray;
                katCesticeTable.Rows[0].Cells[1].Paragraphs[0].Alignment = Alignment.center;
                katCesticeTable.Rows[0].Cells[2].Paragraphs[0].Append("OPIS");
                katCesticeTable.Rows[0].Cells[2].FillColor = System.Drawing.Color.LightGray;
                katCesticeTable.Rows[0].Cells[2].Paragraphs[0].Alignment = Alignment.center;
                int i = 1;
                foreach (MapPolygon spatial in response.Polygons)
                {
                    katCesticeTable.Rows[i].Cells[0].Paragraphs[0].Append(spatial.Source);
                    katCesticeTable.Rows[i].Cells[0].Paragraphs[0].Alignment = Alignment.center;
                    katCesticeTable.Rows[i].Cells[1].Paragraphs[0].Append(spatial.Type);
                    katCesticeTable.Rows[i].Cells[1].Paragraphs[0].Alignment = Alignment.center;
                    katCesticeTable.Rows[i].Cells[2].Paragraphs[0].Append(spatial.Description);
                    katCesticeTable.Rows[i].Cells[2].Paragraphs[0].Alignment = Alignment.center;
                    i++;
                }

                Paragraph katCesticeTitle = document.InsertParagraph("Katastarske čestice".ToUpper());
                katCesticeTitle.Alignment = Alignment.left;
                katCesticeTitle.InsertTableAfterSelf(katCesticeTable);
            }

            // urbanistic plans results:
            Paragraph resPlanUrbPar = document.InsertParagraph(
                "Rezultat urbanističke identifikacije".ToUpper());
            resPlanUrbPar.SpacingBefore(25d);

            foreach (UrbanPlan mapImageList in response.UrbanPlans)
            {
                Paragraph urbanPlanPar = document.InsertParagraph();
                Novacode.Table table = document.AddTable(1, 4);
                table.Design = TableDesign.TableGrid;

                table.Rows[0].Cells[0].Paragraphs[0].Append(mapImageList.Status);
                table.Rows[0].Cells[0].Paragraphs[0].Alignment = Alignment.center;
                table.Rows[0].Cells[1].Paragraphs[0].Append(mapImageList.Type);
                table.Rows[0].Cells[1].Paragraphs[0].Alignment = Alignment.center;
                table.Rows[0].Cells[2].Paragraphs[0].Append(mapImageList.Name);
                table.Rows[0].Cells[2].Width = 400;
                table.Rows[0].Cells[2].Paragraphs[0].Alignment = Alignment.center;
                table.Rows[0].Cells[3].Paragraphs[0].Append(mapImageList.GisCode);
                table.Rows[0].Cells[3].Paragraphs[0].Alignment = Alignment.center;

                urbanPlanPar.InsertTableBeforeSelf(table);

                // all maps (with leg and comp) in this urban plan:
                foreach (Map planMap in mapImageList.Maps)
                {
                    Paragraph imagesParagraph = document.InsertParagraph((planMap.Name
                        + " " + "MJERILO KARTE 1:" + Math.Round(planMap.Raster.Scale)
                        + " " + "IZVORNO MJERILO KARTE 1:" + planMap.OriginalScale));
                    imagesParagraph.InsertPageBreakBeforeSelf();

                    imagesParagraph.AppendPicture(StreamService.convertToImage(document,
                        planMap.RasterImage).CreatePicture());
                    imagesParagraph.AppendPicture(StreamService.convertToImage(document,
                        planMap.LegendImage).CreatePicture());

                    Picture pic = StreamService.convertToImage(document,
                        planMap.ComponentImage).CreatePicture();
                    imagesParagraph.AppendPicture(pic);
                    if (pic.Height < 900)
                    {
                        imagesParagraph.InsertPageBreakAfterSelf();
                    }
                }
            }

            if (response.OtherPlans != null && response.OtherPlans.Count != 0)
            {
                foreach (OtherPlan other in response.OtherPlans)
                {
                    createTableForOtherPlans(document, other);
                }
            }
        }

        public static void createTableForOtherPlans(DocX document, OtherPlan other)
        {
            Novacode.Table ostaloTable = document.AddTable(other.ResultFeatures.Count + 1, 4);
            ostaloTable.Design = TableDesign.TableGrid;
            ostaloTable.Alignment = Alignment.center;
            float[] tableRowsWidth = { 100F, 100F, 100F, 100F };
            ostaloTable.SetWidthsPercentage(tableRowsWidth, null);
            ostaloTable.Rows[0].Cells[0].Paragraphs[0].Append("STATUS");
            ostaloTable.Rows[0].Cells[0].FillColor = System.Drawing.Color.LightGray;
            ostaloTable.Rows[0].Cells[0].Paragraphs[0].Alignment = Alignment.center;
            ostaloTable.Rows[0].Cells[1].Paragraphs[0].Append("VRSTA");
            ostaloTable.Rows[0].Cells[1].FillColor = System.Drawing.Color.LightGray;
            ostaloTable.Rows[0].Cells[1].Paragraphs[0].Alignment = Alignment.center;
            ostaloTable.Rows[0].Cells[2].Paragraphs[0].Append("NAZIV");
            ostaloTable.Rows[0].Cells[2].FillColor = System.Drawing.Color.LightGray;
            ostaloTable.Rows[0].Cells[2].Paragraphs[0].Alignment = Alignment.center;
            ostaloTable.Rows[0].Cells[3].Paragraphs[0].Append("BROJ");
            ostaloTable.Rows[0].Cells[3].FillColor = System.Drawing.Color.LightGray;
            ostaloTable.Rows[0].Cells[3].Paragraphs[0].Alignment = Alignment.center;

            int i = 1;
            foreach (OtherPlan.ResultFeature plan in other.ResultFeatures)
            {
                ostaloTable.Rows[i].Cells[0].Paragraphs[0].Append(plan.Status);
                ostaloTable.Rows[i].Cells[0].Paragraphs[0].Alignment = Alignment.center;
                ostaloTable.Rows[i].Cells[1].Paragraphs[0].Append(plan.Type);
                ostaloTable.Rows[i].Cells[1].Paragraphs[0].Alignment = Alignment.center;
                ostaloTable.Rows[i].Cells[2].Paragraphs[0].Append(plan.Name);
                ostaloTable.Rows[i].Cells[2].Paragraphs[0].Alignment = Alignment.center;
                ostaloTable.Rows[i].Cells[3].Paragraphs[0].Append(plan.Sn);
                ostaloTable.Rows[i].Cells[3].Paragraphs[0].Alignment = Alignment.center;
                i++;
            }

            Paragraph otherPlansPar = document.InsertParagraph(other.Id.ToUpper());
            otherPlansPar.InsertPageBreakBeforeSelf();
            otherPlansPar.InsertTableAfterSelf(ostaloTable);
        }

        public static void convertDocxToPdf(MemoryStream ms)
        {
            Document document = new Document();
            document.LoadFromStream(ms, FileFormat.Docx);
            ms.Position = 0;
            document.SaveToStream(ms, FileFormat.PDF);

            PdfDocument pdfDoc = PdfReader.Open(ms);
            PdfPages pages = pdfDoc.Pages;
            PdfPage page = pages[0];
            XGraphics gfx = XGraphics.FromPdfPage(page);

            XPen pen = new XPen(XColors.White, 10);
            gfx.DrawRectangle(pen, 60, 70, 500, 10);

            pdfDoc.Save(ms);
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
                    await createDocument(request, ms, webRootPath);
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