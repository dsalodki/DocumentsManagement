namespace DocumentsUploadingDownloadingApi.RabbitMQ
{
    public interface IRabbitMqService
    {
        void SendMessage(string message);
        void SendMessage(object obj);
    }
}
