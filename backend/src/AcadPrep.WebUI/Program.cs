using Application;
using Infrastructure;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WebUI.Middlewares;
using Domain.Enums;

var builder = WebApplication.CreateBuilder(args);

// Reverse proxy (nginx/IIS): đúng scheme HTTPS + client IP cho VNPay
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    // Clear known networks/proxies when behind cloud LB — tighten in production if needed.
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

// Add Data Protection to persist keys to disk (prevents cookie invalidation on dev restarts)
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new System.IO.DirectoryInfo(
        Path.Combine(builder.Environment.ContentRootPath, ".dp-keys")));

// 1. Register Clean Architecture Layer Services
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration, builder.Environment);

// 2. Cookie + Google OAuth Authentication
var googleClientId     = builder.Configuration["Authentication:Google:ClientId"];
var googleClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
var isGoogleAuthConfigured = !string.IsNullOrWhiteSpace(googleClientId)
    && !string.IsNullOrWhiteSpace(googleClientSecret)
    && !googleClientId.StartsWith("${", StringComparison.Ordinal);

var authBuilder = builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultScheme          = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    })
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
    {
        options.LoginPath         = "/Account/Login";
        options.AccessDeniedPath  = "/Account/AccessDenied";
        options.ExpireTimeSpan    = TimeSpan.FromDays(14);
        options.SlidingExpiration = true;
    })
    .AddCookie("GoogleTempCookie");

if (isGoogleAuthConfigured)
{
    authBuilder.AddGoogle(options =>
    {
        options.ClientId     = googleClientId!;
        options.ClientSecret = googleClientSecret!;
        options.SaveTokens   = true;
        options.SignInScheme = "GoogleTempCookie";
    });
}

// 2. Register Controllers & Razor Pages
builder.Services.AddControllers();
builder.Services.AddRazorPages(options =>
{
    // 1. Chỉ cho phép Admin hoặc Moderator truy cập thư mục /Admin
    options.Conventions.AuthorizeFolder("/Admin", "RequireAdminOrModeratorRole");
    
    // Phân quyền cụ thể cho các khu vực quản trị/điều phối
    options.Conventions.AuthorizeFolder("/Admin/Exams", "RequireModeratorRole");
    options.Conventions.AuthorizeFolder("/Admin/Accounts", "RequireAdminRole");
    options.Conventions.AuthorizeFolder("/Admin/Achievements", "RequireAdminRole");
    options.Conventions.AuthorizePage("/Admin/Dashboard", "RequireAdminRole");
    options.Conventions.AuthorizePage("/Admin/Report", "RequireAdminRole");
    options.Conventions.AuthorizePage("/Admin/ExamStats", "RequireAdminRole");

    // 2. Yêu cầu đăng nhập đối với các thư mục chức năng cá nhân (chỉ dành cho Learner)
    options.Conventions.AuthorizeFolder("/Vocabulary", "RequireLearnerRole");
    options.Conventions.AuthorizeFolder("/Performance", "RequireLearnerRole");
    options.Conventions.AuthorizePage("/Exams/Take", "RequireLearnerRole");
    options.Conventions.AuthorizePage("/Exams/Practice", "RequireLearnerRole");
    options.Conventions.AuthorizePage("/Exams/Results", "RequireLearnerRole");

    // 3. Cho phép truy cập công khai không cần đăng nhập
    options.Conventions.AllowAnonymousToPage("/Exams/Index");
    options.Conventions.AllowAnonymousToPage("/Exams/Detail");
    options.Conventions.AllowAnonymousToPage("/Performance/Leaderboard");
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdminRole", policy => policy.RequireRole(nameof(UserRole.Admin)));
    options.AddPolicy("RequireModeratorRole", policy => policy.RequireRole(nameof(UserRole.Moderator)));
    options.AddPolicy("RequireLearnerRole", policy => policy.RequireRole(nameof(UserRole.Learner)));
    options.AddPolicy("RequireAdminOrModeratorRole", policy => 
        policy.RequireRole(nameof(UserRole.Admin), nameof(UserRole.Moderator)));
});

builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 104_857_600; // 100 MB
});

builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 104_857_600; // 100 MB
});

// Add CORS to allow external Frontend (React, Vue, Next.js, etc.)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowExternalFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://localhost:3001") // Define allowed frontend ports here
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // Optional: required if your FE uses HttpOnly cookies or Auth.
    });
});

// 3. Register Swagger/OpenAPI (Optional but recommended for APIs)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = System.IO.Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath);
});

var app = builder.Build();

// Seed Database
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        await Infrastructure.Persistence.AppDbContextSeed.SeedAsync(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred seeding the DB.");
    }
}
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var initializer = scope.ServiceProvider.GetRequiredService<Infrastructure.Persistence.AppDbContextInitializer>();
    await initializer.SeedAsync();
}

// 4. Configure HTTP request pipeline & Custom Exception Middleware
app.UseForwardedHeaders();
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "AcadPrep API v1");
        // Swagger UI at /swagger (default). Root URL "/" is reserved for Razor Pages.
    });
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// Enable CORS for frontend applications (Must be before Auth)
app.UseCors("AllowExternalFrontend");

app.UseAuthentication();
app.UseAuthorization();


// 5. Map Controllers & Razor Pages
app.MapControllers();
app.MapRazorPages();

// Redirect root to /Exams
app.MapGet("/", () => Results.Redirect("/Index"));

app.Run();
