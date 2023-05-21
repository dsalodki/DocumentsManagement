using DocumentsUploadingDownloadingApi.Models;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Text.Json;

namespace Emailing.RabbitMq
{
    public class RabbitMqListener : BackgroundService
    {
        private IConnection _connection;
        private IModel _channel;
        private string _queueName;

        public RabbitMqListener()
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.ExchangeDeclare(exchange: "notifier", type: ExchangeType.Fanout);
            _queueName = _channel.QueueDeclare().QueueName;
            _channel.QueueBind(queue: _queueName,
                exchange: "notifier",
                routingKey: string.Empty);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (sender, e) =>
            {
                var content = Encoding.UTF8.GetString(e.Body.ToArray());
                MqDocument document = JsonSerializer.Deserialize<MqDocument>(content);

                SendEmail(document);

                _channel.BasicAck(e.DeliveryTag, false);
            };

            _channel.BasicConsume(_queueName, true, consumer);
        }

        private void SendEmail(MqDocument document)
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

        public override void Dispose()
        {
            _channel.Close();
            _connection.Close();
            base.Dispose();
        }
    }
}
