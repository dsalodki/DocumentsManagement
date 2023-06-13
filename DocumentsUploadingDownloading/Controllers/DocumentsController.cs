using DocumentsUploadingDownloading.Models;
using DocumentsUploadingDownloadingApi.RabbitMQ;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Models;
using System.Net.Mime;

namespace DocumentsUploadingDownloading.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DocumentsController : ControllerBase
    {
        private readonly DocumentsApiContext _db;
        private const long _maxFileSize = 1048576;
        private static readonly string[] _acceptableFileTypes = { ".txt" };
        private readonly IRabbitMqService _mqService;

        public DocumentsController(DocumentsApiContext db, IRabbitMqService mqService)
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

            return File(doc.Content, MediaTypeNames.Text.Plain, fileDownloadName: doc.FileName);
        }

        [HttpPost]
        [Microsoft.AspNetCore.Mvc.ProducesResponseType(201)]
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

                return new ObjectResult(doc.Id) { StatusCode = StatusCodes.Status201Created };
            }
        }
    }
}
