using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DocumentsUploadingDownloadingApi.RabbitMQ
{
    public class RabbitMqService : IRabbitMqService
    {
        public void SendMessage(object obj)
        {
            var message = JsonSerializer.Serialize(obj);
            SendMessage(message);
        }

        public void SendMessage(string message)
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            channel.ExchangeDeclare(exchange: "notifier", type: ExchangeType.Fanout);

            var body = Encoding.UTF8.GetBytes(message);

            channel.BasicPublish(exchange: "notifier",
                routingKey: string.Empty,
                basicProperties: null,
                body: body);
        }
    }
}
