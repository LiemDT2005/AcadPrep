using Application.Common.Interfaces;
using Infrastructure.Persistence;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Connection string 'DefaultConnection' không được tìm thấy trong cấu hình.");
        }

        // Register AppDbContext with Microsoft SQL Server
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(connectionString, builder =>
                builder.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)));

        // Bind IAppDbContext to DbContext implementation
        services.AddScoped<IAppDbContext>(provider => provider.GetRequiredService<AppDbContext>());

        // Register Database Initializer
        services.AddScoped<AppDbContextInitializer>();

        // Register HttpContextAccessor
        services.AddHttpContextAccessor();

        // Register CurrentUserService
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        // Register PasswordHasher
        services.AddScoped<IPasswordHasher, PasswordHasher>();

        // Register standard TimeProvider
        services.AddSingleton(TimeProvider.System);

        // Redis cache configuration: Fallback to In-Memory cache if Redis is not configured
        var redisConnectionString = configuration.GetConnectionString("Redis");
        if (!string.IsNullOrEmpty(redisConnectionString))
        {
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisConnectionString;
            });
        }
        else
        {
            services.AddDistributedMemoryCache();
        }
        services.AddSingleton<ICacheService, RedisCacheService>();

        // Register AI generation service
        services.AddScoped<IAiGenerationService, MockAiGenerationService>();
        
        // Register Cloudinary storage service
        services.AddScoped<IFileStorageService, CloudinaryStorageService>();
        // Register Email service (mock — swap for real SMTP/SendGrid in production)
        services.AddScoped<IEmailService, MockEmailService>();

        // Register OTP issuer
        services.AddScoped<IOtpIssuer, OtpIssuer>();

        return services;
    }
}
