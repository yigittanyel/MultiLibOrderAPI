using BenchmarkDotNet.Configs;
using Mapster;
using MapsterMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MultiLLibray.API;
using MultiLLibray.API.Context;
using MultiLLibray.API.Extensions;
using MultiLLibray.API.Logger;
using MultiLLibray.API.MapperProfiles;
using MultiLLibray.API.Middlewares;
using MultiLLibray.API.Repositories;
using NLog;
using NLog.Extensions.Logging;
using System.Text;

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

#region redis
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSingleton<RedisCacheHelper>();
#endregion


#region jwt bearer
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Issuer"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });
#endregion

builder.Services.AddScoped<OrderRepository>(provider =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    return new OrderRepository(connectionString);
});

builder.Services.AddSingleton<OrderMapperProfile>();
builder.Services.AddSingleton<UserMapperProfile>();

builder.Services.AddDbContext<ApplicationDbContext>(c => c.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


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

app.UseAuthentication();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();