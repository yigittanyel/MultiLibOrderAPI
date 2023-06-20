using Microsoft.EntityFrameworkCore;
using MultiLLibray.API.Context;
using System.Reflection;

namespace MultiLLibray.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection LoadCustomServices(this IServiceCollection services, IConfiguration config)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var types = assembly.GetTypes()
            .Where(t => t.IsClass && t.Name.EndsWith("MapperProfile"));

        foreach (var type in types)
        {
            var instance = Activator.CreateInstance(type);
            services.AddSingleton(instance);
        }

        return services;
    }

}