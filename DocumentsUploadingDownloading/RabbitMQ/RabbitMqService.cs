using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DocumentsUploadingDownloadingApi.RabbitMQ
{
    public class RabbitMqService : IRabbitMqService
    {
        private readonly static IModel _channel;

        static RabbitMqService()
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            var connection = factory.CreateConnection();
            _channel = connection.CreateModel();

            _channel.ExchangeDeclare(exchange: "notifier", type: ExchangeType.Fanout);
        }

        public void SendMessage(object obj)
        {
            var message = JsonSerializer.Serialize(obj);
            SendMessage(message);
        }

        public void SendMessage(string message)
        {

            var body = Encoding.UTF8.GetBytes(message);

            _channel.BasicPublish(exchange: "notifier",
                routingKey: string.Empty,
                basicProperties: null,
                body: body);
        }
    }
}
