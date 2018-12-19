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
using System.Collections.Generic;

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
            //find template by id:
            
            templateId += ".docx";

            string path = Path.Combine(webRootPath, "Templates", templateId);
            return DocX.Load(path).Copy();
        }


        public static void AddInfo(DocX document, DataResponse response)
        {
            // template:
            string klasa = "klasa";
            string urBroj = "urudžbeni broj";
            string datum = DateTime.Now.ToLongDateString();
            string font = "Arial";
            int fontSize = 12;
            List<string> urbanisticPlanResultsOrder = new List<string> { "DPU", "UPU", "PPUOG",
            "GUP", "PPPPO", "PPŽ", "DPPR"};

            List<string> urbanisticPlanResultsStatusOrder = new List<string> { "VAŽEĆI", "U IZRADI", "OBVEZA DONOŠENJA", "VAN SNAGE ODLUKA O IZRADI", "VAN SNAGE"};
            List<System.Drawing.Color> colorList = new List<System.Drawing.Color> { System.Drawing.Color.LightGreen, System.Drawing.Color.Yellow, System.Drawing.Color.White,
                System.Drawing.Color.White, System.Drawing.Color.White };

            document.ReplaceText("[KLASA]", klasa);
            document.ReplaceText("[UBROJ]", urBroj);
            document.ReplaceText("[DATUM]", datum);
           
            Paragraph title = document.InsertParagraph("Urbanistička identifikacija".ToUpper()).Font(font).FontSize(fontSize);
            title.Alignment = Alignment.center;
            title.SpacingBefore(30d);
            title.SpacingAfter(25d);

            float[] tableWidthKatCestice = { 40F, 40F, 40F, 40F, 40F, 60F};

            // spatial conditions:
            if (response.Polygons != null && response.Polygons.Count != 0)
            {
                Novacode.Table katCesticeTable = document.AddTable(
                    response.Polygons.Count + 1, 6);
                katCesticeTable.Design = TableDesign.TableGrid;
                katCesticeTable.Alignment = Alignment.center;
                katCesticeTable.SetWidthsPercentage(tableWidthKatCestice, null);
                katCesticeTable.Rows[0].Cells[0].Paragraphs[0].Append("KO MBR");
                katCesticeTable.Rows[0].Cells[0].Paragraphs[0].Alignment = Alignment.center;
                katCesticeTable.Rows[0].Cells[0].Paragraphs[0].Font(font);
                katCesticeTable.Rows[0].Cells[1].Paragraphs[0].Append("KO NAZIV");
                katCesticeTable.Rows[0].Cells[1].Paragraphs[0].Alignment = Alignment.center;
                katCesticeTable.Rows[0].Cells[1].Paragraphs[0].Font(font);
                katCesticeTable.Rows[0].Cells[2].Paragraphs[0].Append("KČ BROJ");
                katCesticeTable.Rows[0].Cells[2].Paragraphs[0].Alignment = Alignment.center;
                katCesticeTable.Rows[0].Cells[2].Paragraphs[0].Font(font);
                katCesticeTable.Rows[0].Cells[3].Paragraphs[0].Append("KO STATUS");
                katCesticeTable.Rows[0].Cells[3].Paragraphs[0].Alignment = Alignment.center;
                katCesticeTable.Rows[0].Cells[3].Paragraphs[0].Font(font);
                katCesticeTable.Rows[0].Cells[4].Paragraphs[0].Append("KO DATUM");
                katCesticeTable.Rows[0].Cells[4].Paragraphs[0].Alignment = Alignment.center;
                katCesticeTable.Rows[0].Cells[4].Paragraphs[0].Font(font);
                katCesticeTable.Rows[0].Cells[5].Paragraphs[0].Append("KO AKTIVNA");
                katCesticeTable.Rows[0].Cells[5].Paragraphs[0].Alignment = Alignment.center;
                katCesticeTable.Rows[0].Cells[5].Paragraphs[0].Font(font);
                int i = 1;
                foreach (MapPolygon spatial in response.Polygons)
                {
                    //katCesticeTable.Rows[i].Cells[0].Paragraphs[0].Append(spatial.Source);
                    katCesticeTable.Rows[i].Cells[0].Paragraphs[0].Append("KO MBR");
                    katCesticeTable.Rows[i].Cells[0].Paragraphs[0].Alignment = Alignment.center;
                    katCesticeTable.Rows[i].Cells[0].Paragraphs[0].Font(font);
                    //katCesticeTable.Rows[i].Cells[1].Paragraphs[0].Append(spatial.Type);
                    String koNaziv = spatial.Description.Substring(spatial.Description.IndexOf("KO") + 2,
                        spatial.Description.IndexOf(",") - 2);
                    katCesticeTable.Rows[i].Cells[1].Paragraphs[0].Append(koNaziv);
                    katCesticeTable.Rows[i].Cells[1].Paragraphs[0].Alignment = Alignment.center;
                    katCesticeTable.Rows[i].Cells[1].Paragraphs[0].Font(font);
                    String kcBroj = spatial.Description.Substring(spatial.Description.IndexOf(",") + 1);
                    katCesticeTable.Rows[i].Cells[2].Paragraphs[0].Append(kcBroj);
                    katCesticeTable.Rows[i].Cells[2].Paragraphs[0].Alignment = Alignment.center;
                    katCesticeTable.Rows[i].Cells[2].Paragraphs[0].Font(font);
                    katCesticeTable.Rows[i].Cells[3].Paragraphs[0].Append("SLUZBENA");
                    katCesticeTable.Rows[i].Cells[3].Paragraphs[0].Alignment = Alignment.center;
                    katCesticeTable.Rows[i].Cells[3].Paragraphs[0].Font(font);
                    katCesticeTable.Rows[i].Cells[4].Paragraphs[0].Append(DateTime.Now.ToOADate().ToString());
                    katCesticeTable.Rows[i].Cells[4].Paragraphs[0].Alignment = Alignment.center;
                    katCesticeTable.Rows[i].Cells[4].Paragraphs[0].Font(font);
                    katCesticeTable.Rows[i].Cells[5].Paragraphs[0].Append("AKTIVNA");
                    katCesticeTable.Rows[i].Cells[5].Paragraphs[0].Alignment = Alignment.center;
                    katCesticeTable.Rows[i].Cells[5].Paragraphs[0].Font(font);
                    i++;
                }

                Paragraph katCesticeTitle = document.InsertParagraph("Katastarske čestice".ToUpper()).Font(font).FontSize(fontSize);
                katCesticeTitle.Alignment = Alignment.left;
                katCesticeTitle.InsertTableAfterSelf(katCesticeTable);
            }
            if (response.OtherPlans != null && response.OtherPlans.Count != 0)
            {
                Paragraph zasticenoPodrucjePar = document.InsertParagraph("Zaštičeno područje".ToUpper()).Font(font).FontSize(fontSize);
                zasticenoPodrucjePar.Alignment = Alignment.left;
                zasticenoPodrucjePar.Font(font);
                zasticenoPodrucjePar.SpacingBefore(30d);
                foreach (OtherPlan otherPlan in response.OtherPlans)
                {
                   
                    if (!urbanisticPlanResultsOrder.Contains(otherPlan.Title))
                    {

                        for (int i = 0; i < otherPlan.ResultFeatures.Count; i++) {
                            Novacode.Table zasticenoPodrucjeTable = document.AddTable(
                                1, 3);
                            zasticenoPodrucjeTable.Design = TableDesign.TableGrid;
                            zasticenoPodrucjeTable.Alignment = Alignment.center;
                            zasticenoPodrucjeTable.Rows[0].Cells[0].Paragraphs[0].Append(otherPlan.ResultFeatures[i].Type);
                            zasticenoPodrucjeTable.Rows[0].Cells[0].Paragraphs[0].Alignment = Alignment.center;
                            zasticenoPodrucjeTable.Rows[0].Cells[0].Paragraphs[0].Font(font);
                            zasticenoPodrucjeTable.Rows[0].Cells[1].Paragraphs[0].Append(otherPlan.ResultFeatures[i].Name);
                            zasticenoPodrucjeTable.Rows[0].Cells[1].Width = 400;
                            zasticenoPodrucjeTable.Rows[0].Cells[1].Paragraphs[0].Alignment = Alignment.center;
                            zasticenoPodrucjeTable.Rows[0].Cells[1].Paragraphs[0].Font(font);
                            zasticenoPodrucjeTable.Rows[0].Cells[2].Paragraphs[0].Append(otherPlan.ResultFeatures[i].Sn);
                            zasticenoPodrucjeTable.Rows[0].Cells[2].Paragraphs[0].Alignment = Alignment.center;
                            zasticenoPodrucjeTable.Rows[0].Cells[2].Paragraphs[0].Font(font);
                            zasticenoPodrucjePar.InsertTableAfterSelf(zasticenoPodrucjeTable);
                            
                        }
                        
                    }

                }
            }

            List<UrbanPlan> urbanPlansMaps = new List<UrbanPlan> { };

            // urbanistic plans:
            if (response.UrbanPlans != null && response.UrbanPlans.Count != 0)
            {
                Paragraph resPlanUrbPar = document.InsertParagraph(
                    "Rezultat urbanističke identifikacije".ToUpper()).Font(font).FontSize(fontSize);
                resPlanUrbPar.SpacingBefore(25d);
                for (int i = 0; i < urbanisticPlanResultsOrder.Count; i++)
                {
                    for (int j = 0; j < urbanisticPlanResultsStatusOrder.Count; j++)
                    {
                        foreach (UrbanPlan mapImageList in response.UrbanPlans)
                        {
                            if (mapImageList.Type == urbanisticPlanResultsOrder[i] && mapImageList.Status == urbanisticPlanResultsStatusOrder[j])
                            {

                                Paragraph urbanPlanPar = document.InsertParagraph();
                                Novacode.Table table = document.AddTable(1, 4);
                                table.Design = TableDesign.TableGrid;
                                table.Alignment = Alignment.center;


                                table.Rows[0].Cells[0].Paragraphs[0].Append(mapImageList.Status);
                                table.Rows[0].Cells[0].Paragraphs[0].Alignment = Alignment.center;
                                table.Rows[0].Cells[0].FillColor = colorList[j];
                                table.Rows[0].Cells[0].Paragraphs[0].Font(font);
                                table.Rows[0].Cells[1].Paragraphs[0].Append(mapImageList.Type);
                                table.Rows[0].Cells[1].Paragraphs[0].Alignment = Alignment.center;
                                table.Rows[0].Cells[1].FillColor = colorList[j];
                                table.Rows[0].Cells[1].Paragraphs[0].Font(font);
                                table.Rows[0].Cells[2].Paragraphs[0].Append(mapImageList.Name);
                                table.Rows[0].Cells[2].Width = 400;
                                table.Rows[0].Cells[2].Paragraphs[0].Alignment = Alignment.center;
                                table.Rows[0].Cells[2].FillColor = colorList[j];
                                table.Rows[0].Cells[2].Paragraphs[0].Font(font);
                                table.Rows[0].Cells[3].Paragraphs[0].Append(mapImageList.GisCode);
                                table.Rows[0].Cells[3].Paragraphs[0].Alignment = Alignment.center;
                                table.Rows[0].Cells[3].FillColor = colorList[j];
                                table.Rows[0].Cells[3].Paragraphs[0].Font(font);

                                urbanPlanPar.InsertTableBeforeSelf(table);

                                urbanPlansMaps.Add(mapImageList);

                            }
                        }
                    }

                    bool firstImageOccurrence = true;

                    if (urbanPlansMaps != null && urbanPlansMaps.Count != 0)
                    {
                        foreach (UrbanPlan urbanPlan in urbanPlansMaps)
                        {
                            if (urbanPlan.Maps != null && urbanPlan.Maps.Count != 0)
                            {
                                foreach (Map planMap in urbanPlan.Maps)
                                {
                                    Paragraph imagesParagraph = document.InsertParagraph((planMap.Name
                                        + " " + "MJERILO KARTE 1:" + Math.Round(planMap.Raster.Scale)
                                        + " " + "IZVORNO MJERILO KARTE 1:" + planMap.OriginalScale)).Font(font).FontSize(fontSize);
                                    if (firstImageOccurrence)
                                    {
                                        imagesParagraph.InsertPageBreakBeforeSelf();
                                        firstImageOccurrence = false;
                                    }


                                    imagesParagraph.AppendPicture(StreamService.convertToImage(document,
                                        planMap.RasterImage).CreatePicture());
                                    imagesParagraph.AppendPicture(StreamService.convertToImage(document,
                                        planMap.LegendImage).CreatePicture());
                                    imagesParagraph.AppendPicture(StreamService.convertToImage(document,
                                        planMap.ComponentImage).CreatePicture());
                                    if (planMap == urbanPlan.Maps[urbanPlan.Maps.Count - 1])
                                    {
                                        imagesParagraph.InsertPageBreakAfterSelf();
                                    }
                                }
                            }
                        }
                    }
                    urbanPlansMaps.Clear();
                }
            }
            bool firstOtherPlanOccurrence = true;

            // other plans:
            if (response.OtherPlans != null && response.OtherPlans.Count != 0)
            {
                foreach (OtherPlan other in response.OtherPlans)
                {
                    if (urbanisticPlanResultsOrder.Contains(other.Title))
                    {
                        createTableForOtherPlans(document, other, firstOtherPlanOccurrence, font, fontSize);
                        firstOtherPlanOccurrence = false;
                    }
                }
            }

            //add basemap
            
            if(response.BaseMaps.Count != 0 && response.BaseMaps != null)
            {
                foreach(BaseMap baseMap in response.BaseMaps)
                {
                    Paragraph imagesParagraph = document.InsertParagraph((baseMap.ResultFeatures[0].Name
                                        + " " + "MJERILO KARTE 1:" + baseMap.ResultFeatures[0].MapScale)).Font(font).FontSize(fontSize);
                    
                    imagesParagraph.AppendPicture(StreamService.convertToImage(document,
                        baseMap.ResultFeatures[0].BaseMapImage).CreatePicture());
                    if (baseMap != response.BaseMaps[response.BaseMaps.Count - 1])
                    {
                        imagesParagraph.InsertPageBreakAfterSelf();
                    }
                }
            }
            
           
        }

        // additional function:
        public static void createTableForOtherPlans(DocX document, OtherPlan other, bool occurrence, string font, int fontSize)
        {
            float[] tableColsWidth = { 20, 10, 100, 20 };
            Novacode.Table ostaloTable = document.AddTable(other.ResultFeatures.Count + 1, 4);
            ostaloTable.Design = TableDesign.TableGrid;
            ostaloTable.Alignment = Alignment.center;
            //ostaloTable.SetWidthsPercentage(tableColsWidth, null);
            ostaloTable.Rows[0].Cells[0].Paragraphs[0].Append("STATUS");
            ostaloTable.Rows[0].Cells[0].FillColor = System.Drawing.Color.LightGray;
            ostaloTable.Rows[0].Cells[0].Paragraphs[0].Alignment = Alignment.center;
            ostaloTable.Rows[0].Cells[0].Paragraphs[0].Font(font);
            ostaloTable.Rows[0].Cells[1].Paragraphs[0].Append("VRSTA");
            ostaloTable.Rows[0].Cells[1].FillColor = System.Drawing.Color.LightGray;
            ostaloTable.Rows[0].Cells[1].Paragraphs[0].Alignment = Alignment.center;
            ostaloTable.Rows[0].Cells[0].Paragraphs[0].Font(font);
            ostaloTable.Rows[0].Cells[2].Paragraphs[0].Append("NAZIV");
            ostaloTable.Rows[0].Cells[2].FillColor = System.Drawing.Color.LightGray;
            ostaloTable.Rows[0].Cells[2].Paragraphs[0].Alignment = Alignment.center;
            ostaloTable.Rows[0].Cells[2].Paragraphs[0].Font(font);
            ostaloTable.Rows[0].Cells[3].Paragraphs[0].Append("BROJ");
            ostaloTable.Rows[0].Cells[3].FillColor = System.Drawing.Color.LightGray;
            ostaloTable.Rows[0].Cells[3].Paragraphs[0].Alignment = Alignment.center;
            ostaloTable.Rows[0].Cells[3].Paragraphs[0].Font(font);

            int i = 1;
            foreach (OtherPlan.ResultFeature plan in other.ResultFeatures)
            {
                ostaloTable.Rows[i].Cells[0].Paragraphs[0].Append(plan.Status);
                ostaloTable.Rows[i].Cells[0].Paragraphs[0].Alignment = Alignment.center;
                ostaloTable.Rows[i].Cells[0].Paragraphs[0].Font(font);
                ostaloTable.Rows[i].Cells[1].Paragraphs[0].Append(plan.Type);
                ostaloTable.Rows[i].Cells[1].Paragraphs[0].Alignment = Alignment.center;
                ostaloTable.Rows[i].Cells[1].Paragraphs[0].Font(font);
                ostaloTable.Rows[i].Cells[2].Paragraphs[0].Append(plan.Name);
                ostaloTable.Rows[i].Cells[2].Paragraphs[0].Alignment = Alignment.center;
                ostaloTable.Rows[i].Cells[2].Paragraphs[0].Font(font);
                ostaloTable.Rows[i].Cells[3].Paragraphs[0].Append(plan.Sn);
                ostaloTable.Rows[i].Cells[3].Paragraphs[0].Alignment = Alignment.center;
                ostaloTable.Rows[i].Cells[3].Paragraphs[0].Font(font);
                i++;
            }

            Paragraph otherPlansPar = document.InsertParagraph("Ostali planovi na području JLS (nepoznat obuhvat)").Font(font).FontSize(fontSize);
            if (!occurrence)
            {
                otherPlansPar.InsertPageBreakBeforeSelf();
            }
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