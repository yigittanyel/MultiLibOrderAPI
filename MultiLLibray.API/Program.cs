using Microsoft.EntityFrameworkCore;
using MultiLLibray.API.Context;
using MultiLLibray.API.Logger;
using MultiLLibray.API.MapperProfiles;
using MultiLLibray.API.Middlewares;
using MultiLLibray.API.Repositories;
using NLog;
using NLog.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

#region Nlog

// Konfigürasyonu yükle
var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

// NLog yapýlandýrmasýný yükle
LogManager.LoadConfiguration("nlog.config");

builder.Services.AddLogging(loggingBuilder =>
{
    loggingBuilder.ClearProviders();
    loggingBuilder.AddNLog(configuration);
});

builder.Services.AddSingleton<ILoggerService, LoggerService>();
#endregion


builder.Services.AddDbContext<ApplicationDbContext>
    (c => c.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<OrderRepository>(provider =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    return new OrderRepository(connectionString);
});

builder.Services.AddSingleton<OrderMapperProfile>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

#region middleware
app.UseMiddleware<ExceptionMiddleware>();
#endregion


app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();