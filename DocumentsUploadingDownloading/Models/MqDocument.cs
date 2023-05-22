namespace DocumentsUploadingDownloadingApi.Models
{
    public class MqDocument
    {
        public int Id { get; set; }
        public byte[] Content { get; set; }
        public string FileName { get; set; }
    }
}
