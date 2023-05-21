using DocumentsUploadingDownloadingApi.Models;
using Emailing.Models;
using Microsoft.EntityFrameworkCore;
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
        private readonly IServiceProvider _serviceProvider;

        public RabbitMqListener(IServiceProvider serviceProvider)
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.ExchangeDeclare(exchange: "notifier", type: ExchangeType.Fanout);
            _queueName = _channel.QueueDeclare().QueueName;
            _channel.QueueBind(queue: _queueName,
                exchange: "notifier",
                routingKey: string.Empty);
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (sender, e) =>
            {
                var content = Encoding.UTF8.GetString(e.Body.ToArray());
                MqDocument document = JsonSerializer.Deserialize<MqDocument>(content);

                using (IServiceScope scope = _serviceProvider.CreateScope())
                {
                    IScopedProcessingService scopedProcessingService =
                        scope.ServiceProvider.GetRequiredService<IScopedProcessingService>();

                    Task.Run(async() => await scopedProcessingService.DoWorkAsync(document)).Wait();
                }

                _channel.BasicAck(e.DeliveryTag, false);
            };

            _channel.BasicConsume(_queueName, true, consumer);

        }

        public override void Dispose()
        {
            _channel.Close();
            _connection.Close();
            base.Dispose();
        }
    }
}
