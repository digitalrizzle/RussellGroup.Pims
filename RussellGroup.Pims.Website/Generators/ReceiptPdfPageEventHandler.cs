using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using iTextSharp.text;

namespace RussellGroup.Pims.Website.Generators
{
    public class ReceiptPdfPageEventHandler : PdfPageEventHelper
    {
        private const float Margin = ReceiptPdfGenerator.Margin;
        private static readonly BaseFont footerFont = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, BaseFont.EMBEDDED);

        private PdfContentByte DirectContent { get; set; }
        private PdfTemplate Template { get; set; }
        private DateTime WhenDocumentPrinted { get; set; }

        public override void OnOpenDocument(PdfWriter writer, Document document)
        {
            base.OnOpenDocument(writer, document);

            DirectContent = writer.DirectContent;
            Template = DirectContent.CreateTemplate(50f, 50f);
            WhenDocumentPrinted = DateTime.Now;
        }

        public override void OnEndPage(PdfWriter writer, Document document)
        {
            base.OnEndPage(writer, document);

            var pageSize = document.PageSize;

            // horizontal rule
            DirectContent.SetColorStroke(BaseColor.LIGHT_GRAY);
            DirectContent.MoveTo(pageSize.GetLeft(Margin), pageSize.GetBottom(Margin));
            DirectContent.LineTo(pageSize.GetRight(Margin), pageSize.GetBottom(Margin));
            DirectContent.SetLineWidth(0.25f);
            DirectContent.Stroke();

            // page numbering
            var text = $"Page {writer.PageNumber} of ";
            var length = footerFont.GetWidthPoint(text, 8);

            DirectContent.SetColorFill(BaseColor.GRAY);
            DirectContent.BeginText();
            DirectContent.SetFontAndSize(footerFont, 8f);
            DirectContent.SetTextMatrix(pageSize.GetLeft(Margin), pageSize.GetBottom(Margin - 12));
            DirectContent.ShowText(text);
            DirectContent.EndText();
            DirectContent.AddTemplate(Template, pageSize.GetLeft(Margin) + length, pageSize.GetBottom(Margin - 12));

            // print date and time
            DirectContent.BeginText();
            DirectContent.SetFontAndSize(footerFont, 8f);
            DirectContent.SetTextMatrix(pageSize.GetLeft(Margin), pageSize.GetBottom(Margin - 12));
            DirectContent.ShowTextAligned(PdfContentByte.ALIGN_RIGHT, WhenDocumentPrinted.ToString("d/MM/yyyy h:mm:ss tt"), pageSize.GetRight(Margin), pageSize.GetBottom(Margin - 12), 0);
            DirectContent.EndText();
        }

        public override void OnCloseDocument(PdfWriter writer, Document document)
        {
            base.OnCloseDocument(writer, document);

            Template.SetColorFill(BaseColor.GRAY);
            Template.BeginText();
            Template.SetFontAndSize(footerFont, 8f);
            Template.SetTextMatrix(0, 0);
            Template.ShowText(writer.PageNumber.ToString());
            Template.EndText();
        }
    }
}