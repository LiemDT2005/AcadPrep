# 3. Application Layer & CQRS Use Cases

This document describes how to structure the Application layer, implement CQRS use cases with MediatR, write validators, and manage database operations using `IAppDbContext`.

## CQRS & MediatR Use Case Structure

Each use case in the Application layer is treated as a single, isolated class. Do not build huge "service" classes. Instead, group related commands and queries into feature directories:

```
Application/
  └── Features/
       └── Products/
            ├── Commands/
            │    ├── CreateProduct/
            │    │    ├── CreateProductCommand.cs
            │    │    ├── CreateProductCommandHandler.cs
            │    │    ├── CreateProductCommandValidator.cs
            │    │    └── CreateProductDto.cs
            │    └── UpdateProduct/
            └── Queries/
                 ├── GetProductList/
                 │    ├── GetProductListQuery.cs
                 │    ├── GetProductListQueryHandler.cs
                 │    └── GetProductListQueryValidator.cs
                 └── Common/DTOs/GetProductDto.cs
```

---

## Code Templates & Best Practices

To write highly performant, testable, and modern use cases (C# 12+ / .NET 8+), adopt the following standards:

### 1. Primary Constructors
Use primary constructors on handlers to inject dependencies directly into the class header. This removes boilerplate private read-only fields and constructors.

### 2. ValueTask for MediatR
Use `ValueTask<T>` instead of `Task<T>` in handler signatures to optimize memory allocations when results are returned synchronously or from memory.

### 3. TimeProvider Mocking
Inject standard `TimeProvider` instead of calling `DateTime.UtcNow` directly. This makes time-based logic fully determinable and easily mockable in unit tests.

### 4. Direct `.Select()` Query Projection (Bypass AutoMapper)
For read queries (Queries), use LINQ's `.Select(...)` method directly to map database records to DTOs. This avoids loading entire entities into memory and bypasses AutoMapper, maximizing database query performance.

---

## Complete Command Template Example

### 1. Command DTO
```csharp
// Application/Features/Products/Commands/CreateProduct/CreateProductDto.cs
namespace Application.Features.Products.Commands.CreateProduct;

public class CreateProductDto
{
    public required string Title { get; set; }
    public required string Description { get; set; }
    public decimal Price { get; set; }
}
```

### 2. Command Request
```csharp
// Application/Features/Products/Commands/CreateProduct/CreateProductCommand.cs
using Application.Common.Models;
using MediatR;

namespace Application.Features.Products.Commands.CreateProduct;

public class CreateProductCommand : IRequest<Result<string>>
{
    public required CreateProductDto CreateProductDto { get; set; }
}
```

### 3. Validator (FluentValidation)
Always validate that the DTO is not null first, then scope validations inside `When(...)` to avoid `NullReferenceException`. Ensure error messages are written in Vietnamese.
```csharp
// Application/Features/Products/Commands/CreateProduct/CreateProductCommandValidator.cs
using FluentValidation;

namespace Application.Features.Products.Commands.CreateProduct;

public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.CreateProductDto)
            .NotNull().WithMessage("Thông tin sản phẩm là bắt buộc");

        When(x => x.CreateProductDto != null, () =>
        {
            RuleFor(x => x.CreateProductDto!.Title)
                .NotEmpty().WithMessage("Tiêu đề không được để trống")
                .MaximumLength(200).WithMessage("Tiêu đề không được vượt quá 200 ký tự");

            RuleFor(x => x.CreateProductDto!.Description)
                .NotEmpty().WithMessage("Mô tả không được để trống")
                .MaximumLength(2000).WithMessage("Mô tả không được vượt quá 2000 ký tự");

            RuleFor(x => x.CreateProductDto!.Price)
                .GreaterThanOrEqualTo(0).WithMessage("Giá sản phẩm không được nhỏ hơn 0");
        });
    }
}
```

### 4. Use Case Handler (ValueTask, Primary Constructor, TimeProvider & Rich Entity)
```csharp
// Application/Features/Products/Commands/CreateProduct/CreateProductCommandHandler.cs
using Application.Common.Models;
using Application.Common.Interfaces;
using Domain.Entities;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Products.Commands.CreateProduct;

internal sealed class CreateProductCommandHandler(
    IAppDbContext context,
    ICurrentUserService currentUserService,
    TimeProvider clock) : IRequestHandler<CreateProductCommand, Result<string>>
{
    public async ValueTask<Result<string>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        // Encapsulated state initialization using Rich Domain factory
        var product = Product.Create(
            request.CreateProductDto.Title,
            request.CreateProductDto.Description,
            request.CreateProductDto.Price,
            currentUserService.UserId!,
            clock.GetUtcNow());

        context.Products.Add(product);
        var result = await context.SaveChangesAsync(cancellationToken) > 0;

        if (!result)
        {
            return Result<string>.Failure("Không thể tạo mới dữ liệu", 400);
        }

        return Result<string>.Success("Tạo mới thành công", product.Id);
    }
}
```

---

## Direct Projection Query Template Example

```csharp
// Application/Features/Products/Queries/GetProductList/GetProductListQueryHandler.cs
using Application.Common.Models;
using Application.Common.Interfaces;
using Application.Features.Products.Queries.Common.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Products.Queries.GetProductList;

public record GetProductListQuery : IRequest<Result<List<GetProductDto>>>;

internal sealed class GetProductListQueryHandler(IAppDbContext context)
    : IRequestHandler<GetProductListQuery, Result<List<GetProductDto>>>
{
    public async ValueTask<Result<List<GetProductDto>>> Handle(
        GetProductListQuery request, CancellationToken cancellationToken)
    {
        // 1. Always filter out soft-deleted and inactive records
        var query = context.Products
            .Where(x => !x.IsDeleted && x.IsActive);

        // 2. Perform direct projection to prevent loading full entities in EF Core tracker
        var items = await query
            .Select(x => new GetProductDto
            {
                Id = x.Id,
                Title = x.Title,
                Price = x.Price,
                CreatedDate = x.CreatedDate
            })
            .ToListAsync(cancellationToken);

        return Result<List<GetProductDto>>.Success("Lấy danh sách thành công", items);
    }
}
```

---

## Why IAppDbContext is Preferred Over Repositories

Many older .NET patterns enforce writing a Generic Repository (e.g., `IRepository<T>`) and a `UnitOfWork` interface. Clean Architecture avoids this abstraction layer:

1. **EF Core is already a repository:** `DbSet<T>` is a Repository, and `DbContext` is a Unit of Work. Wrapping them adds indirection and reduces EF Core's capabilities (like direct projections, optimized joins, and raw SQL queries) without adding real architectural value.
2. **Abstracting the DbContext in Application:** By defining an interface `IAppDbContext` containing only the relevant `DbSet<T>` sets, the Application layer gets full SQL power while remaining decoupled from the physical SQL database configuration:
   ```csharp
   public interface IAppDbContext
   {
       DbSet<Product> Products { get; }
       Task<int> SaveChangesAsync(CancellationToken cancellationToken);
   }
   ```
