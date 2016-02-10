using RussellGroup.Pims.DataAccess.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Web;

namespace RussellGroup.Pims.Website.Helpers
{
    public class MailHelper
    {
        public static void Send(Receipt receipt)
        {
            var environment = System.Configuration.ConfigurationManager.AppSettings["Environment"];
            var recipients = receipt.Recipients.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(f => f.Trim()).Where(f => !string.IsNullOrWhiteSpace(f));

            foreach (var recipient in recipients)
            {
                using (var stream = new MemoryStream(receipt.Content.Data))
                {
                    var message = new MailMessage();

                    message.To.Add(new MailAddress(recipient));
                    message.Subject = $"PIMS receipt for docket {receipt.Docket} of job {receipt.Job.XJobId}";

                    string text =
                        "Hello" + Environment.NewLine +
                        Environment.NewLine +
                        $"Attached is your PIMS {receipt.TransactionType.Name.ToLower()} receipt for docket {receipt.Docket} of the \"{receipt.Job.Description}\" job ({receipt.Job.XJobId}).";

                    string html =
                        "<p>Hello</p>" +
                        $"<p>Attached is your PIMS {receipt.TransactionType.Name.ToLower()} receipt for docket {receipt.Docket} of the \"{receipt.Job.Description}\" job ({receipt.Job.XJobId}).</p>";

                    if (!string.IsNullOrEmpty(environment))
                    {
                        text +=
                            Environment.NewLine +
                            Environment.NewLine +
                            $"This email message pertains to the {environment} environment.";

                        html +=
                            $"<small>This email message pertains to the {environment} environment.</small>";
                    }

                    message.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(text, null, MediaTypeNames.Text.Plain));
                    message.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(html, null, MediaTypeNames.Text.Html));

                    message.Attachments.Add(new Attachment(stream, receipt.Docket, receipt.Content.ContentType));

                    using (var smtpClient = new SmtpClient())
                    {
                        smtpClient.Send(message);
                    }
                }
            }
        }
    }
}