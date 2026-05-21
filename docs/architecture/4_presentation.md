# 4. Presentation Layer & API Design

This document covers Web API presentation designs, comparing Traditional API Controllers to modern .NET Minimal APIs using auto-discovery.

## The Role of the Presentation Layer

The Presentation layer (WebUI or API) is the entry point of the application. Its core principles are:
1. **Thin Responsibility:** Its sole responsibility is mapping incoming HTTP requests to MediatR commands/queries, dispatching them, and mapping the returned `Result<T>` envelope to the correct HTTP status codes.
2. **Zero Business Logic:** No business validation, computation, or database queries should occur in this layer.
3. **Global Exception Handling:** Middleware should catch all unhandled exceptions and format them into standardized responses (e.g., ProblemDetails) without exposing database stack traces.

---

## Traditional API Controllers

For projects preferring classical API Controllers, let controllers inherit from a base `ApiControllerBase` that automatically injects MediatR's `ISender` and handles custom `Result` mappings:

```csharp
// WebUI/Controllers/ApiControllerBase.cs
using Application.Common.Models;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace WebUI.Controllers;

[ApiController]
public abstract class ApiControllerBase : ControllerBase
{
    private ISender? _mediator;
    protected ISender Mediator => _mediator ??= HttpContext.RequestServices.GetRequiredService<ISender>();

    protected ActionResult HandleResult<T>(Result<T> result)
    {
        if (result.IsSuccess)
        {
            return result.Value is null ? Ok() : Ok(result.Value);
        }

        return result.Code switch
        {
            400 => BadRequest(new { Error = result.Error }),
            401 => Unauthorized(),
            403 => Forbid(),
            404 => NotFound(new { Error = result.Error }),
            410 => StatusCode(StatusCodes.Status410Gone, new { Error = result.Error }),
            423 => StatusCode(StatusCodes.Status423Locked, new { Error = result.Error }),
            _ => StatusCode(StatusCodes.Status500InternalServerError, new { Error = "Lỗi hệ thống xảy ra" })
        };
    }
}
```

### Example Product Controller
```csharp
// WebUI/Controllers/ProductsController.cs
using Application.Features.Products.Commands.CreateProduct;
using Application.Features.Products.Queries.GetProductList;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace WebUI.Controllers;

[Route("api/v1/products")]
public class ProductsController : ApiControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult> GetProducts()
    {
        return HandleResult(await Mediator.Send(new GetProductListQuery()));
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult> CreateProduct(CreateProductDto dto)
    {
        return HandleResult(await Mediator.Send(new CreateProductCommand { CreateProductDto = dto }));
    }
}
```

---

## Modern Minimal APIs with Auto-Discovery

Minimal APIs are highly recommended in .NET 8+ because they bypass the heavy MVC controller lifecycle, leading to faster startup times and lower memory footprints. 

To prevent `Program.cs` from becoming bloated with hundreds of route mappings, we implement an **Auto-Discovery** pattern.

### 1. The `IEndpointGroup` Interface
Define a simple interface in the WebUI layer (or an shared assembly):
```csharp
// WebUI/Endpoints/IEndpointGroup.cs
using Microsoft.AspNetCore.Routing;

namespace WebUI.Endpoints;

public interface IEndpointGroup
{
    void Map(IEndpointRouteBuilder app);
}
```

### 2. Auto-Discovery Extension Method
Write an extension method that scans the WebUI assembly for all classes implementing `IEndpointGroup` and executes their `Map` methods:
```csharp
// WebUI/Endpoints/EndpointExtensions.cs
using System;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace WebUI.Endpoints;

public static class EndpointExtensions
{
    public static IEndpointRouteBuilder MapEndpoints(this IEndpointRouteBuilder app)
    {
        var endpointGroupType = typeof(IEndpointGroup);
        var assembly = Assembly.GetExecutingAssembly();

        var endpointGroupTypes = assembly.GetTypes()
            .Where(t => endpointGroupType.IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

        foreach (var type in endpointGroupTypes)
        {
            var instance = (IEndpointGroup)Activator.CreateInstance(type)!;
            instance.Map(app);
        }

        return app;
    }
}
```

### 3. Implementing the Product Endpoints Group
Now, simply create a sealed class implementing `IEndpointGroup`. No edits are needed in `Program.cs` when adding new endpoints!
```csharp
// WebUI/Endpoints/ProductEndpoints.cs
using Application.Features.Products.Commands.CreateProduct;
using Application.Features.Products.Queries.GetProductList;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Threading.Tasks;

namespace WebUI.Endpoints;

public sealed class ProductEndpoints : IEndpointGroup
{
    public void Map(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/products").WithTags("Products");

        group.MapGet("/", GetProducts)
            .WithName("GetProducts")
            .AllowAnonymous();

        group.MapPost("/", CreateProduct)
            .WithName("CreateProduct")
            .RequireAuthorization();
    }

    private static async Task<IResult> GetProducts(ISender sender)
    {
        var result = await sender.Send(new GetProductListQuery());
        return result.IsSuccess ? TypedResults.Ok(result.Value) : TypedResults.BadRequest(result.Error);
    }

    private static async Task<IResult> CreateProduct(CreateProductDto dto, ISender sender)
    {
        var result = await sender.Send(new CreateProductCommand { CreateProductDto = dto });
        return result.IsSuccess 
            ? TypedResults.Created($"/api/v1/products/{result.Value}", result.Value) 
            : TypedResults.BadRequest(result.Error);
    }
}
```

### 4. Wire Up in `Program.cs`
Add these two lines in `Program.cs` to enable the auto-discovered endpoints:
```csharp
// WebUI/Program.cs
var app = builder.Build();

// ... normal middleware configuration ...

app.MapEndpoints(); // Automatically maps all IEndpointGroup implementations!

app.Run();
```

---

## Razor Pages (Hybrid API + Server-Side UI)

For projects that require a traditional Server-Side Rendered (SSR) web interface alongside an API (for external Frontends like React/Next.js or Mobile Apps), Razor Pages can be integrated directly into the `WebUI` project.

### The CQRS Advantage in Views
A common anti-pattern is for Server-Side rendered views (Razor/Blazor) to use `HttpClient` to call their own API endpoints. This introduces unnecessary network serialization overhead.

In Clean Architecture with MediatR, **Razor Pages should inject `ISender` and call the Application Layer directly**, exactly like API Controllers do.

```csharp
// WebUI/Pages/Products/Index.cshtml.cs
using Application.Features.Products.Queries.GetProductList;
using MediatR;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WebUI.Pages.Products;

public class IndexModel : PageModel
{
    private readonly ISender _mediator;

    public IndexModel(ISender mediator)
    {
        _mediator = mediator;
    }

    public List<GetProductDto> Products { get; set; } = new();

    public async Task OnGetAsync()
    {
        // Direct call to Application Layer (No HTTP/API overhead)
        var response = await _mediator.Send(new GetProductListQuery());
        
        if (response.IsSuccess && response.Value != null)
        {
            Products = response.Value;
        }
    }
}
```

### Migration Path to External Frontend
By keeping both `Controllers/` (for JSON API) and `Pages/` (for HTML UI) in the same project:
1. You can develop features rapidly using Razor Pages today.
2. The REST APIs are simultaneously available (and testable via Swagger) for future external consumers.
3. When transitioning to a standalone frontend (React/Angular), the frontend simply consumes the existing `Controllers/`, and the `Pages/` can be safely removed or kept as an internal Admin dashboard.
