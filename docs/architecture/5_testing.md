# 5. Testing Architecture & Guidelines

This document details the unit and integration testing architecture for the C# Clean Architecture solution.

## Test Infrastructure Stack

- **xUnit:** The industry-standard testing framework for .NET.
- **FluentAssertions:** A set of extension methods that allow you to naturally specify the expected outcome of a test.
- **Moq:** A mocking library used to simulate simple service behaviors (such as `ICurrentUserService` and `TimeProvider`).
- **SQLite In-Memory (`TestDbContextFactory`):** The primary tool used to test database queries and transactions.

---

## CRITICAL: Why We NEVER Mock the DbContext

In many generic .NET guides, you will see developers trying to mock the `IAppDbContext` or `DbSet<T>` using Moq:
```csharp
// DANGEROUS ANTI-PATTERN: Do NOT do this!
var dbSetMock = new Mock<DbSet<Product>>();
var contextMock = new Mock<IAppDbContext>();
contextMock.Setup(x => x.Products).Returns(dbSetMock.Object);
```
**Why this is dangerous and incorrect:**
1. **Extension Methods are not mockable:** EF Core makes heavy use of extension methods (such as `FirstOrDefaultAsync`, `CountAsync`, `ToListAsync`). Moq **cannot** mock static C# extension methods. Running a test with these methods against a mocked `DbSet` will result in a `NullReferenceException` or crash.
2. **Behavior is unrealistic:** Mocking collections does not enforce database constraints (such as unique keys, foreign keys, or database column length validations), leading to green tests that crash in production.

### The Solution: SQLite In-Memory Database
We use a real SQLite database running purely in the system's memory. SQLite is extremely fast, takes milliseconds to spin up, and behaves exactly like a real SQL database—enforcing real foreign keys, transactions, and fully executing EF Core extension methods.

---

## Implementing the `TestDbContextFactory`

Create this helper in your `Application.UnitTests` project:

```csharp
// Application.UnitTests/Helpers/TestDbContextFactory.cs
using Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System;

namespace Application.UnitTests.Helpers;

public static class TestDbContextFactory
{
    public static AppDbContext CreateInMemoryDbContext()
    {
        // 1. Establish an in-memory SQLite connection
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();

        // 2. Configure DbContext options to use SQLite
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;

        var context = new AppDbContext(options);

        // 3. Ensure the physical SQLite schema is created (executes configurations)
        context.Database.EnsureCreated();

        return context;
    }
}
```

---

## Step-by-Step Test Guide

### 1. Naming Conventions
Follow the naming standard: `MethodName_Should[ExpectedBehavior]_When[Scenario]`
Examples:
- `Handle_ShouldCreateProduct_WhenValid`
- `Handle_ShouldReturnForbidden_WhenUserIsNotOwner`
- `Handle_ShouldReturnNotFound_WhenProductDoesNotExist`

### 2. Testing Create Command Handlers
When testing handlers, seed dependent foreign keys (like the user) because SQLite enforces actual FK constraints.

```csharp
using Application.Common.Models;
using Application.Common.Interfaces;
using Application.Features.Products.Commands.CreateProduct;
using Domain.Entities;
using FluentAssertions;
using Moq;
using Infrastructure.Persistence;
using Application.UnitTests.Helpers;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Application.UnitTests.Products.Commands.CreateProduct;

public class CreateProductCommandHandlerTests
{
    private readonly AppDbContext _context;
    private readonly Mock<ICurrentUserService> _currentUserMock;
    private readonly Mock<TimeProvider> _clockMock;
    private readonly CreateProductCommandHandler _handler;
    private readonly DateTimeOffset _testTime = new(2026, 5, 20, 12, 0, 0, TimeSpan.Zero);

    public CreateProductCommandHandlerTests()
    {
        _context = TestDbContextFactory.CreateInMemoryDbContext();
        _currentUserMock = new Mock<ICurrentUserService>();
        _currentUserMock.Setup(x => x.UserId).Returns("user-1");

        _clockMock = new Mock<TimeProvider>();
        _clockMock.Setup(c => c.GetUtcNow()).Returns(_testTime);

        _handler = new CreateProductCommandHandler(_context, _currentUserMock.Object, _clockMock.Object);
    }

    private async Task SeedUserAsync()
    {
        _context.Users.Add(new ApplicationUser { Id = "user-1", UserName = "test" });
        await _context.SaveChangesAsync();
    }

    [Fact]
    public async Task Handle_ShouldCreateProduct_WhenValid()
    {
        // Arrange
        await SeedUserAsync();
        var dto = new CreateProductDto { Title = "Test Product", Description = "Test Desc", Price = 100 };
        var command = new CreateProductCommand { CreateProductDto = dto };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNullOrEmpty();

        var saved = await _context.Products.FindAsync(result.Value);
        saved.Should().NotBeNull();
        saved!.Title.Should().Be("Test Product");
        saved.CreatedDate.Should().Be(_testTime.UtcDateTime);
    }
}
```

### 3. Testing Update/Delete Handlers (Ownership & State checks)
```csharp
[Fact]
public async Task Handle_ShouldReturnForbidden_WhenUserIsNotOwner()
{
    // Arrange
    var context = await CreateContextWithProductSeededAsync(ownerId: "owner-id");
    
    // Attacker tries to update
    _currentUserMock.Setup(x => x.UserId).Returns("attacker-id");
    var handler = new UpdateProductCommandHandler(context, _currentUserMock.Object, _clockMock.Object);

    var product = await context.Products.FirstAsync();
    var command = new UpdateProductCommand 
    { 
        Id = product.Id, 
        UpdateProductDto = new UpdateProductDto { Title = "Hacked Title", Description = "Hacked Desc" } 
    };

    // Act
    var result = await handler.Handle(command, CancellationToken.None);

    // Assert
    result.IsSuccess.Should().BeFalse();
    result.Code.Should().Be(403);
    result.Error.Should().Be("Bạn không có quyền cập nhật dữ liệu này");
}
```

### 4. Testing FluentValidation Rules
Use FluentValidation's built-in `TestValidate` helper. You do not need database configurations to test validator rules.

```csharp
using Application.Features.Products.Commands.CreateProduct;
using FluentValidation.TestHelper;
using Xunit;

namespace Application.UnitTests.Products.Commands.CreateProduct;

public class CreateProductCommandValidatorTests
{
    private readonly CreateProductCommandValidator _validator = new();

    [Fact]
    public void Should_HaveValidationError_WhenTitleIsEmpty()
    {
        var command = new CreateProductCommand
        {
            CreateProductDto = new CreateProductDto { Title = "", Description = "Valid" }
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.CreateProductDto!.Title);
    }
}
```
