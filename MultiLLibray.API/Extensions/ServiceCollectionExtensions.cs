using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MultiLLibray.API.Context;
using MultiLLibray.API.Logger;
using MultiLLibray.API.Repositories;
using NLog;
using NLog.Extensions.Logging;
using System.Reflection;
using System.Text;

namespace MultiLLibray.API.Extensions;
public static class ServiceCollectionExtensions
{
    public static IServiceCollection LoadCustomServices(this IServiceCollection services, IConfiguration config)
    {
        var assembly = typeof(Program).Assembly;
        var mapperProfileTypes = assembly.GetTypes()
            .Where(t => t.IsClass && t.Name.EndsWith("MapperProfile"));

        foreach (var mapperProfileType in mapperProfileTypes)
        {
            var mapperProfileInstance = Activator.CreateInstance(mapperProfileType);
            services.AddSingleton(mapperProfileType, mapperProfileInstance);
        }


        #region dbContext
        services.AddDbContext<ApplicationDbContext>(c => c.UseSqlServer(config.GetConnectionString("DefaultConnection")));
        #endregion

        #region redis
        services.AddDistributedMemoryCache();
        services.AddSingleton<RedisCacheHelper>();
        #endregion

        #region jwt bearer
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = config["Jwt:Issuer"],
                    ValidAudience = config["Jwt:Issuer"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]))
                };
            });
        #endregion

        #region Nlog

        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();

        LogManager.LoadConfiguration("nlog.config");

        services.AddLogging(loggingBuilder =>
        {
            loggingBuilder.ClearProviders();
            loggingBuilder.AddNLog(configuration);
        });

        services.AddSingleton<ILoggerService, LoggerService>();
        #endregion

        services.AddScoped<OrderRepository>(provider =>
        {
            var connectionString = config.GetConnectionString("DefaultConnection");
            return new OrderRepository(connectionString);
        });

        return services;
    }
}