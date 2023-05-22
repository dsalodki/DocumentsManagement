using Emailing.Models;
using Microsoft.EntityFrameworkCore;
using System.Net.Mail;
using System.Net;
using DocumentsUploadingDownloadingApi.Models;

namespace Emailing.RabbitMq
{
    public class DefaultScopedProcessingService : IScopedProcessingService
    {
        private readonly EmailingDbContext _db;

        public DefaultScopedProcessingService(EmailingDbContext db)
        {
            _db = db;
        }

        public async Task DoWorkAsync(MqDocument document)
        {
            // Inbox pattern
            if (await _db.Documents.AnyAsync(d => d.DocumentId == document.Id))
            {
                return;
            }

            int uploadedDocumentStatusId = (await _db.Statuses.FirstAsync(s => s.Name == "UploadedDocument")).Id;
            var doc = new Document
            {
                DocumentId = document.Id,
                StatusId = uploadedDocumentStatusId
            };
            _db.Documents.Add(doc);
            await _db.SaveChangesAsync();

            await SendEmail(document);

            // change status to sent
            int sentEmailStatusId = (await _db.Statuses.FirstAsync(s => s.Name == "SentEmail")).Id;
            doc = await _db.Documents.FirstAsync(d => d.DocumentId == document.Id);
            doc.StatusId = sentEmailStatusId;
            await _db.SaveChangesAsync();
        }

        private async Task SendEmail(MqDocument document)
        {
            MailMessage mail = new MailMessage();
            SmtpClient smtpServer = new SmtpClient("smtp.mail.ru");
            mail.From = new MailAddress("dzmitry.piatrovich.salodki@mail.ru");
            mail.To.Add("dsalodki@gmail.com");
            mail.Subject = "Загружен документ - " + document.FileName;
            mail.Body = "файл прикреплён";


            using var memoryStream = new MemoryStream(document.Content);
            memoryStream.Position = 0;

            System.Net.Mime.ContentType ct = new System.Net.Mime.ContentType(System.Net.Mime.MediaTypeNames.Text.Plain);

            System.Net.Mail.Attachment attachment;
            attachment = new System.Net.Mail.Attachment(memoryStream, ct);
            attachment.ContentDisposition.FileName = document.FileName;
            mail.Attachments.Add(attachment);

            smtpServer.UseDefaultCredentials = false;
            smtpServer.Port = 587;
            smtpServer.Credentials = new System.Net.NetworkCredential("dzmitry.piatrovich.salodki@mail.ru", "cVTjaidyFS4GrkWN7idB");
            smtpServer.EnableSsl = true;

            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            System.Net.ServicePointManager.Expect100Continue = false;

            smtpServer.Send(mail);
            memoryStream.Close();
        }

    }
}
