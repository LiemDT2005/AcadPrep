# 2. Domain Layer & Rich Domain Model

This document explains how to design the Domain layer using a **Rich Domain Model** and avoid the **Anemic Domain Model** anti-pattern.

## Rich Domain Model vs. Anemic Domain Model

### Anemic Domain Model (Anti-Pattern)
An anemic entity is a simple "data bag" containing only properties with public getters and setters, and zero logic:
```csharp
// BAD: Anemic Entity
public class Product : BaseEntity
{
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public decimal Price { get; set; }
}

// In the Handler: Logic is scattered outside the domain
var product = new Product();
product.Title = dto.Title;
product.Description = dto.Description;
product.Price = dto.Price; // State changes occur without validation or business rule enforcement
```
*Why this is bad:*
- The entity cannot defend its invariants (business rules). For example, a product's price could be set to a negative number anywhere in the Application layer, bypassing rules.
- State mutation rules are duplicated across various commands, leading to bugs.

---

### Rich Domain Model (Best Practice)
A rich domain model enforces **encapsulation**. The entity manages its own state and defends its business invariants. Properties have `private set` or `protected set`, and state changes occur only via explicit domain methods.

```csharp
// GOOD: Rich Domain Entity
using Domain.Common;

namespace Domain.Entities;

public class Product : BaseEntity
{
    private Product() { } // Required by EF Core for hydration

    public string Title { get; private set; } = null!;
    public string Description { get; private set; } = null!;
    public decimal Price { get; private set; }
    public string UserId { get; private set; } = null!;

    // 1. Static Factory Method (Handles creation logic and guarantees valid initial state)
    public static Product Create(string title, string description, decimal price, string userId, DateTimeOffset now)
    {
        if (price < 0)
        {
            throw new ArgumentException("Giá sản phẩm không được nhỏ hơn 0");
        }

        return new Product
        {
            Id = Guid.CreateVersion7().ToString(),
            Title = title,
            Description = description,
            Price = price,
            UserId = userId,
            CreatedDate = now.UtcDateTime,
            CreatedBy = userId,
            IsActive = true,
            IsDeleted = false
        };
    }

    // 2. Encapsulated State Modification Behaviors
    public void UpdateDetails(string title, string description, decimal price, string userId, DateTimeOffset now)
    {
        if (price < 0)
        {
            throw new ArgumentException("Giá sản phẩm không được nhỏ hơn 0");
        }

        Title = title;
        Description = description;
        Price = price;
        UpdatedDate = now.UtcDateTime;
        UpdatedBy = userId;
    }

    public void SoftDelete(string userId, DateTimeOffset now)
    {
        IsDeleted = true;
        UpdatedDate = now.UtcDateTime;
        UpdatedBy = userId;
    }
}
```

---

## Designing a Solid Domain Entity

When writing a Domain Entity in a new C# Clean Architecture project, adhere to these rules:

1. **Use Private Constructors for EF Core**
   - EF Core requires an empty parameterless constructor to map database columns back to C# objects. Define this as `private` or `protected` so that external application code cannot instantiate an empty entity.
   - Example: `private Product() { }`

2. **Make Setters Private**
   - Define property setters as `private set` or `protected set`. The only way to modify state is through clear, descriptive domain methods (e.g., `UpdateDetails`, `Approve`, `Cancel`).

3. **Avoid Parameterized Constructors for Instantiation**
   - Prefer **Static Factory Methods** (`public static Product Create(...)`). This makes instantiation clear and enables you to perform pre-checks before returning a fully initialized object.

4. **Base Entity Inheritance**
   - Let entities inherit from a standard `BaseEntity` which provides common tracking fields:
     ```csharp
     public abstract class BaseEntity
     {
         public string Id { get; set; } = null!; // Or Guid
         public DateTime CreatedDate { get; set; }
         public string? CreatedBy { get; set; }
         public DateTime? UpdatedDate { get; set; }
         public string? UpdatedBy { get; set; }
         public bool IsActive { get; set; }
         public bool IsDeleted { get; set; }
     }
     ```

5. **No Framework Dependencies**
   - The Domain layer must depend strictly on standard .NET types. Do **not** reference Entity Framework, MediatR, or any external NuGet library here.
