using Application.Common.Interfaces;
using Infrastructure.Persistence;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
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

        // Register Email service: MockEmailService ở Development, SmtpEmailService ở Production
        if (environment.IsDevelopment())
        {
            services.AddScoped<IEmailService, MockEmailService>();
        }
        else
        {
            services.AddScoped<IEmailService, SmtpEmailService>();
        }

        // Bind SmtpSettings từ configuration (cần thiết cho SmtpEmailService)
        services.Configure<SmtpSettings>(configuration.GetSection("Smtp"));

        // Register OTP issuer
        services.AddScoped<IOtpIssuer, OtpIssuer>();

        // Register Notification service (điểm chung tạo thông báo cho UC-15)
        services.AddScoped<INotificationService, NotificationService>();

        return services;
    }
}
