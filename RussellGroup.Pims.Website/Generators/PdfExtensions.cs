using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RussellGroup.Pims.Website.Generators
{
    public static class PdfExtensions
    {
        public static void AddHeaderCell(this PdfPTable table, string text)
        {
            table.AddCell(new PdfPCell(new Phrase(text, ReceiptPdfGenerator.BoldFont)) { VerticalAlignment = PdfPCell.ALIGN_BOTTOM, Padding = ReceiptPdfGenerator.CellPadding, Border = PdfPCell.NO_BORDER, BorderColor = BaseColor.GRAY, BorderWidthBottom = 1f });
        }

        public static void AddJobCell(this PdfPTable table, string text, int colspan = 1)
        {
            table.AddCell(new PdfPCell(new Phrase(text, ReceiptPdfGenerator.BodyFont)) { Colspan = colspan, Padding = ReceiptPdfGenerator.CellPadding, Border = PdfPCell.NO_BORDER, BorderColor = BaseColor.GRAY, BorderWidthBottom = 0.25f, BackgroundColor = ReceiptPdfGenerator.Shade });
        }
    }
}