using Models;

namespace Emailing.RabbitMq
{
    public interface IScopedProcessingService
    {
        Task DoWorkAsync(MqDocument document);
    }
}
