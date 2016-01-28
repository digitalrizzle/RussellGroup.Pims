using iTextSharp.text;
using iTextSharp.text.pdf;
using RussellGroup.Pims.Website.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace RussellGroup.Pims.Website.Generators
{
    public partial class ReceiptPdfGenerator
    {
        private class CheckoutReceiptPdfGenerator
        {
            public static byte[] Create(BatchCheckout batch)
            {
                using (var stream = new MemoryStream())
                {
                    using (var document = new Document(PageSize.A4, Margin, Margin, Margin, Margin + 12))
                    {
                        var writer = PdfWriter.GetInstance(document, stream);
                        writer.PageEvent = new ReceiptPdfPageEventHandler();

                        document.Open();

                        AddMeta(document);
                        AddHeading(document, batch);
                        AddTable(document, batch);
                    }

                    return stream.ToArray();
                }
            }

            private static void AddMeta(Document document)
            {
                document.AddAuthor("digitalrizzle");
                document.AddCreator("PIMS for Dominion Constructors Limited");
                document.AddKeywords("checkout receipt");
                document.AddSubject("Checkout Receipt");
                document.AddTitle("Checkout Receipt");
            }

            private static void AddHeading(Document document, BatchCheckout batch)
            {
                var logoPath = HttpContext.Current.Server.MapPath("~/Content/dominion_constructors.png");
                var logo = Image.GetInstance(logoPath);

                var table = new PdfPTable(new[] { 60f, 20f, 30f })
                {
                    SpacingBefore = 0,
                    SpacingAfter = 10,
                    WidthPercentage = 100
                };

                table.DefaultCell.Border = PdfPCell.NO_BORDER;

                table.AddCell(new Phrase("Checkout Receipt", TitleFont));
                table.AddCell(new PdfPCell(new Phrase(System.Configuration.ConfigurationManager.AppSettings["Environment"], EnvironmentFont)) { Border = PdfPCell.NO_BORDER, HorizontalAlignment = PdfPCell.ALIGN_CENTER });
                table.AddCell(new PdfPCell(logo) { Border = PdfPCell.NO_BORDER, HorizontalAlignment = PdfPCell.ALIGN_RIGHT });

                table.AddCell(new PdfPCell(new Phrase($"Checked out: {batch.WhenStarted.ToShortDateString()}", SubtitleFont)) { Colspan = 3, Border = PdfPCell.NO_BORDER });

                document.Add(table);
                document.Add(new Chunk(line));
            }

            private static void AddTable(Document document, BatchCheckout batch)
            {
                var table = new PdfPTable(5)
                {
                    HorizontalAlignment = PdfPCell.ALIGN_LEFT,
                    SpacingBefore = 20,
                    SpacingAfter = 10,
                    HeaderRows = 1,
                    WidthPercentage = 100
                };

                table.DefaultCell.Border = PdfPCell.NO_BORDER;
                table.DefaultCell.BorderColor = BaseColor.GRAY;
                table.DefaultCell.BorderWidthBottom = 0.25f;
                table.DefaultCell.Padding = CellPadding;

                table.SetWidths(new[] { 12, 15, 18, 40, 15 });

                table.AddHeaderCell("job");
                table.AddHeaderCell("docket");
                table.AddHeaderCell("plant id/new id");
                table.AddHeaderCell("description");
                table.AddHeaderCell("condition");

                foreach (var transaction in batch.CheckoutTransactions)
                {
                    table.AddJobCell(transaction.Job.XJobId);
                    table.AddJobCell(transaction.Docket);
                    table.AddJobCell(string.Empty);
                    table.AddJobCell(transaction.Job.Description, 2);

                    foreach (var plant in transaction.Plants)
                    {
                        table.AddCell(string.Empty);
                        table.AddCell(string.Empty);
                        table.AddCell(new Phrase(plant.XPlantIdAndXPlantNewId, BodyFont));
                        table.AddCell(new Phrase(plant.Description, BodyFont));
                        table.AddCell(new Phrase(plant.Condition.Name, BodyFont));
                    }
                }

                document.Add(table);
            }
        }
    }
}