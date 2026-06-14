using Application;
using Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WebUI.Middlewares;

var builder = WebApplication.CreateBuilder(args);

// 1. Register Clean Architecture Layer Services
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

// 2. Register Controllers & Razor Pages
builder.Services.AddControllers();
builder.Services.AddRazorPages();

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

// 4. Configure HTTP request pipeline & Custom Exception Middleware
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

// Enable CORS for frontend applications (Must be before Auth)
app.UseCors("AllowExternalFrontend");

app.UseAuthorization();

// 5. Map Controllers & Razor Pages
app.MapControllers();
app.MapRazorPages();

// Redirect root to /Exams
app.MapGet("/", () => Results.Redirect("/Exams"));

app.Run();
