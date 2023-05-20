using DocumentsUploadingDownloading.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Headers;
using System.Text;

namespace DocumentsUploadingDownloadingApi.IntegrationTests
{
    public class Tests
    {
        //[SetUp]
        //public void Setup()
        //{
        //}

        [Test]
        public async Task UploadDocument_ShouldReturnId()
        {
            // Arrange

            using WebApplicationFactory<Program> webHost = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    var dbContextDescriptor = services.SingleOrDefault(d =>
                    d.ServiceType == typeof(DbContextOptions<DocumentsApiContext>));

                    services.Remove(dbContextDescriptor);

                    services.AddDbContext<DocumentsApiContext>(options =>
                    {
                        options.UseInMemoryDatabase("upload_db");
                    });
                });
            });

            DocumentsApiContext db = webHost.Services.CreateScope().ServiceProvider.GetService<DocumentsApiContext>();

            List<Document> documents = new List<Document> {
                new Document {
                    Content = Encoding.UTF8.GetBytes("привет"),
                    Create = new DateTime(1984, 1, 27),
                    FileName = "greeting.txt"
                },
                new Document
                {
                    Content = Encoding.UTF8.GetBytes("test"),
                    Create = new DateTime(2023, 5, 20),
                    FileName = "test.txt"
                },
                new Document
                {
                    Content = Encoding.UTF8.GetBytes("какой-то текст"),
                    Create = DateTime.UtcNow,
                    FileName = "какой-то файл.txt"
                }
            };

            await db.Documents.AddRangeAsync(documents);
            await db.SaveChangesAsync();

            // Act

            using var httpClient = webHost.CreateClient();

            using var multipartFormContent = new MultipartFormDataContent();

            string fileContent = "тестовый контент файла";
            string fileName = "тестовое имя файла.txt";
            string mimeType = "text/plain";
            //Load the file and set the file's Content-Type header
            using var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(fileContent));

            var fileStreamContent = new StreamContent(memoryStream);
            fileStreamContent.Headers.ContentType = new MediaTypeHeaderValue(mimeType);

            //Add the file
            multipartFormContent.Add(fileStreamContent, name: "file", fileName: fileName);

            HttpResponseMessage response = await httpClient.PostAsync("api/Document", multipartFormContent);

            response.EnsureSuccessStatusCode();

            // Assert
            Assert.True(int.TryParse(await response.Content.ReadAsStringAsync(), out int id));

            Assert.AreEqual((await db.Documents.LastAsync()).Id, id);
        }
    }
}