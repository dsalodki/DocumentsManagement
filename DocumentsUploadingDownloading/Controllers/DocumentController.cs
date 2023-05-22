using DocumentsUploadingDownloading.Models;
using DocumentsUploadingDownloadingApi.Models;
using DocumentsUploadingDownloadingApi.RabbitMQ;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DocumentsUploadingDownloading.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DocumentController : ControllerBase
    {
        private readonly DocumentsApiContext _db;
        private const string _mimeType = "text/plain";
        private const long _maxFileSize = 1048576;
        private static readonly string[] _acceptableFileTypes = { ".txt" };
        private readonly IRabbitMqService _mqService;

        public DocumentController(DocumentsApiContext db, IRabbitMqService mqService)
        {
            _db = db;
            _mqService = mqService;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> DownloadFile(int id)
        {
            var doc = _db.Documents.FirstOrDefault(x => x.Id == id);

            if (doc == null)
            {
                return NotFound();
            }


            return new FileContentResult(doc.Content, _mimeType)
            {
                FileDownloadName = doc.FileName
            };
        }

        [HttpPost]
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            long size = file.Length;
            if (size > _maxFileSize || size == 0)
            {
                return BadRequest("File size exide than 1Mb or empty");
            }

            string fileType = Path.GetExtension(file.FileName);
            if (!_acceptableFileTypes.Contains(fileType))
            {
                return BadRequest("File extension should be '.txt'");
            }


            using (var memoryStream = new MemoryStream())
            {
                await file.CopyToAsync(memoryStream);

                var doc = new Document()
                {
                    FileName = file.FileName,
                    Content = memoryStream.ToArray(),
                    Create = DateTime.UtcNow,
                };

                await _db.Documents.AddAsync(doc);
                await _db.SaveChangesAsync();

                var message = new MqDocument
                {
                    Content = doc.Content,
                    FileName = doc.FileName,
                    Id = doc.Id
                };

                _mqService.SendMessage(message);

                return Ok(doc.Id);
            }
        }
    }
}
