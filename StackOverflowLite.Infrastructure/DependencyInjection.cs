using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using StackOverflowLite.Application.Common.Interfaces;
using StackOverflowLite.Application.Features.Auth.Commands;
using StackOverflowLite.Infrastructure.Services;

namespace StackOverflowLite.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        // JWT
        services.AddScoped<JwtService>();
        services.AddScoped<IJwtService>(sp => sp.GetRequiredService<JwtService>());
        services.AddScoped<IJwtServiceRef>(sp => sp.GetRequiredService<JwtService>());

        // Redis
        var redisConn = config.GetConnectionString("Redis") ?? "localhost:6379";
        services.AddSingleton<IConnectionMultiplexer>(
            ConnectionMultiplexer.Connect(redisConn));

        services.AddStackExchangeRedisCache(opt => opt.Configuration = redisConn);
        services.AddScoped<ICacheService, RedisCacheService>();

        return services;
    }
}
