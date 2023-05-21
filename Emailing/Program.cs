using Emailing.Models;
using Emailing.RabbitMq;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddHostedService<RabbitMqListener>();

builder.Services.AddScoped<IScopedProcessingService, DefaultScopedProcessingService>();

builder.Services.AddControllers();

builder.Services.AddEntityFrameworkNpgsql().AddDbContext<EmailingDbContext>(
    opt => opt.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
