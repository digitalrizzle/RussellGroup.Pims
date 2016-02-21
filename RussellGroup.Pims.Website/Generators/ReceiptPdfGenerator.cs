using iTextSharp.text;
using iTextSharp.text.pdf.draw;
using RussellGroup.Pims.DataAccess.Models;
using RussellGroup.Pims.Website.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

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

        public byte[] Create(BatchCheckout batch, Job job = null)
        {
            if (job != null)
            {
                batch = new BatchCheckout
                {
                    Scans = batch.Scans,
                    WhenStarted = batch.WhenStarted,
                    CheckoutTransactions = batch.CheckoutTransactions.Where(f => f.JobId == job.Id)
                };
            }

            return CheckoutReceiptPdfGenerator.Create(batch);
        }

        public byte[] Create(BatchCheckin batch, Job job = null)
        {
            if (job != null)
            {
                batch = new BatchCheckin
                {
                    Scans = batch.Scans,
                    WhenEnded = batch.WhenEnded,
                    CheckinTransactions = batch.CheckinTransactions.Where(f => f.JobId == job.Id)
                };
            }

            return CheckinReceiptPdfGenerator.Create(batch);
        }

        protected static Image GetLogo()
        {
            return Image.GetInstance(HttpContext.Current.Server.MapPath("~/Content/dcl_logo_600_dpi_320x109.png"));
        }
    }
}