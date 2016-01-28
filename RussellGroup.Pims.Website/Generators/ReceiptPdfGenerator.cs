using iTextSharp.text;
using iTextSharp.text.pdf.draw;
using RussellGroup.Pims.Website.Models;
using System;
using System.Collections.Generic;

namespace RussellGroup.Pims.Website.Generators
{
    public partial class ReceiptPdfGenerator
    {
        internal const float Margin = 36f;
        internal const float CellPadding = 8f;

        internal static readonly BaseColor Shade = new BaseColor(240, 240, 240);

        internal static readonly Font EnvironmentFont = FontFactory.GetFont("Arial", 24, Font.BOLD, BaseColor.RED);
        internal static readonly Font TitleFont = FontFactory.GetFont("Arial", 24, Font.BOLD);
        internal static readonly Font SubtitleFont = FontFactory.GetFont("Arial", 11, Font.NORMAL);
        internal static readonly Font BoldFont = FontFactory.GetFont("Arial", 10, Font.BOLD);
        internal static readonly Font BodyFont = FontFactory.GetFont("Arial", 10, Font.NORMAL);

        private static readonly LineSeparator line = new LineSeparator(1f, 100f, BaseColor.LIGHT_GRAY, Element.ALIGN_CENTER, -1);

        public byte[] Create(BatchCheckout batch)
        {
            return CheckoutReceiptPdfGenerator.Create(batch);
        }

        public byte[] Create(BatchCheckin batch)
        {
            return CheckinReceiptPdfGenerator.Create(batch);
        }
    }
}