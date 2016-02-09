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
        private class CheckinReceiptPdfGenerator
        {
            public static byte[] Create(BatchCheckin batch)
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
                document.AddKeywords("checkin receipt");
                document.AddSubject("Checkin Receipt");
                document.AddTitle("Checkin Receipt");
            }

            private static void AddHeading(Document document, BatchCheckin batch)
            {
                var logo = GetLogo();

                var table = new PdfPTable(new[] { 60f, 20f, 30f })
                {
                    SpacingBefore = 0,
                    SpacingAfter = 10,
                    WidthPercentage = 100
                };

                table.DefaultCell.Border = PdfPCell.NO_BORDER;

                table.AddCell(new Phrase("Checkin Receipt", TitleFont));
                table.AddCell(new PdfPCell(new Phrase(System.Configuration.ConfigurationManager.AppSettings["Environment"], EnvironmentFont)) { Border = PdfPCell.NO_BORDER, HorizontalAlignment = PdfPCell.ALIGN_CENTER });
                table.AddCell(new PdfPCell(logo, true) { Border = PdfPCell.NO_BORDER, HorizontalAlignment = PdfPCell.ALIGN_RIGHT });

                table.AddCell(new PdfPCell(new Phrase($"Checked in: {batch.WhenEnded.ToShortDateString()}", SubtitleFont)) { Colspan = 3, Border = PdfPCell.NO_BORDER });
                table.AddCell(new PdfPCell(new Phrase($"Checked in by: {HttpContext.Current.User.Identity.Name}", SubtitleFont)) { Colspan = 3, Border = PdfPCell.NO_BORDER });

                document.Add(table);
                document.Add(new Chunk(line));
            }

            private static void AddTable(Document document, BatchCheckin batch)
            {
                var table = new PdfPTable(5)
                {
                    HorizontalAlignment = PdfPCell.ALIGN_LEFT,
                    SpacingBefore = 10,
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
                table.AddHeaderCell("return docket");
                table.AddHeaderCell("plant id/new id");
                table.AddHeaderCell("description");
                table.AddHeaderCell("status");

                foreach (var transaction in batch.CheckinTransactions)
                {
                    table.AddJobCell(transaction.Job.XJobId);
                    table.AddJobCell(transaction.ReturnDocket);
                    table.AddJobCell(string.Empty);
                    table.AddJobCell(transaction.Job.Description, 2);

                    foreach (var hire in transaction.PlantHires)
                    {
                        table.AddCell(string.Empty);
                        table.AddCell(string.Empty);
                        table.AddCell(new Phrase(hire.Plant.XPlantIdAndXPlantNewId, BodyFont));
                        table.AddCell(new Phrase(hire.Plant.Description, BodyFont));
                        table.AddCell(new Phrase(hire.Plant.Status.Name, BodyFont));
                    }
                }

                document.Add(table);
            }
        }
    }
}