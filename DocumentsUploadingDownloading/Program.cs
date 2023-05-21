using DocumentsUploadingDownloading.Models;
using DocumentsUploadingDownloadingApi.RabbitMQ;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "DocUploadDownloadAPI", Version = "v1" });
});

builder.Services.AddScoped<IRabbitMqService, RabbitMqService>();

builder.Services.AddControllers();

builder.Services.AddEntityFrameworkNpgsql().AddDbContext<DocumentsApiContext>(
    opt => opt.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseSwagger();

app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "DocUploadDownloadAPI");
});

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program { }