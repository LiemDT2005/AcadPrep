# Hướng Dẫn Phát Triển Backend (Backend Developer Guide)

Tài liệu này hướng dẫn cách sử dụng, cấu trúc code và cách phát triển các tính năng mới cho hệ thống Backend của AcadPrep.

## 1. Kiến Trúc Hệ Thống (Architecture)

Hệ thống được xây dựng theo kiến trúc **Clean Architecture** (hoặc Onion Architecture) kết hợp với các pattern hiện đại như **CQRS** (Command Query Responsibility Segregation).

### Các Lớp Trong Hệ Thống:

- **Domain**: Chứa các thực thể (Entities), Enums, và các quy tắc nghiệp vụ cốt lõi. Không phụ thuộc vào bất kỳ thư viện ngoài nào ngoại trừ các thư viện hệ thống.
- **Application**: Chứa logic nghiệp vụ (Services, MediatR Handlers), DTOs, Mappings, và Interfaces cho các service bên ngoài. Đây là lớp điều phối chính.
- **Infrastructure**: Chứa các triển khai chi tiết cho việc lưu trữ (Persistence - EF Core), Security (JWT), Email, v.v.
- **Presentation (WebUI)**: Chứa API Controllers, Razor Pages (giao diện web), và Middlewares.

---

## 2. Công Nghệ Sử Dụng (Tech Stack)

- **Language**: C# 13 / .NET 9
- **Database**: Microsoft SQL Server (Entity Framework Core 9 - `Microsoft.EntityFrameworkCore.SqlServer`)
- **Mapping**: AutoMapper
- **Messaging**: MediatR (CQRS Pattern)
- **Validation**: FluentValidation
- **Logging**: Serilog (Nếu có)
- **Documentation**: OpenAPI (Swagger)

---

## 3. Cấu Trúc Folder & Quy Tắc Đặt Tên

### Folder Structure

- `Domain/Entities/`: Tên file PascalCase, số ít (ví dụ: `Employee.cs`).
- `Application/Features/[FeatureName]/Commands/`: Chứa các yêu cầu thay đổi dữ liệu.
- `Application/Features/[FeatureName]/Queries/`: Chứa các yêu cầu đọc dữ liệu.
- `Infrastructure/Persistence/Configurations/`: Cấu hình Fluent API cho EF Core.

### Coding Rules

- Sử dụng **File-scoped namespaces** để giảm indentation.
- Tuân thủ **Coding_Convention_v1.0.md** đã đề ra.
- Luôn sử dụng `async/await` cho các thao tác IO (DB, Network).

---

## 4. Cách Thêm Tính Năng Mới (Step-by-Step)

Giả sử bạn muốn thêm tính năng "Lấy danh sách nhân viên":

### Bước 1: Tạo DTO

Tạo file `EmployeeDto.cs` trong `Application/DTOs/Employees/` (nếu chưa có).
Sử dụng `IMapFrom<Employee>` để tự động mapping.

### Bước 2: Tạo Query & Handler

Tạo file `GetEmployeesQuery.cs` trong `Application/Features/Employees/Queries/`:

```csharp
public record GetEmployeesQuery : IRequest<List<EmployeeDto>>;

public class GetEmployeesQueryHandler : IRequestHandler<GetEmployeesQuery, List<EmployeeDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetEmployeesQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<List<EmployeeDto>> Handle(GetEmployeesQuery request, CancellationToken cancellationToken)
    {
        var employees = await _unitOfWork.Repository<Employee>()
            .Query()
            .ToListAsync(cancellationToken);

        return _mapper.Map<List<EmployeeDto>>(employees);
    }
}
```

### Bước 3: Tạo Controller

Nên tạo một `ApiControllerBase` để dùng chung `ISender` (Mediator):

```csharp
[ApiController]
[Route("api/[controller]")]
public abstract class ApiControllerBase : ControllerBase
{
    private ISender _mediator = null!;
    protected ISender Mediator => _mediator ??= HttpContext.RequestServices.GetRequiredService<ISender>();
}
```

Sau đó tạo `EmployeesController.cs`:

```csharp
public class EmployeesController : ApiControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetEmployees()
    {
        var result = await Mediator.Send(new GetEmployeesQuery());
        return Ok(result);
    }
}
```

---

## 5. Các Patterns Quan Trọng

### Result Pattern (Khuyến nghị)

Thay vì throw exception cho các lỗi logic, hãy dùng `Result<T>`:

- `Result.Success(data)`
- `Result.Failure(errors)`

### Validation Behavior

Hệ thống đã cấu hình `ValidationBehavior` trong MediatR Pipeline. Khi bạn gửi một Request (Command/Query):

1. Pipeline sẽ tìm tất cả các class kế thừa `AbstractValidator<TRequest>`.
2. Nếu có lỗi validation, nó sẽ throw `ValidationException`.
3. `ExceptionMiddleware` sẽ bắt được và trả về lỗi 400 kèm chi tiết các field bị lỗi.

**Cách dùng:** Chỉ cần tạo file validator cùng thư mục với Command.

```csharp
public class CreateEmployeeCommandValidator : AbstractValidator<CreateEmployeeCommand>
{
    public CreateEmployeeCommandValidator()
    {
        RuleFor(v => v.Email).NotEmpty().EmailAddress();
        RuleFor(v => v.Username).MinimumLength(5);
    }
}
```

### AutoMapper (IMapFrom Pattern)

Để tránh việc phải cấu hình Mapping thủ công cho từng DTO, hệ thống sử dụng interface `IMapFrom<T>`.

**Cách dùng:**

- Cho mapping đơn giản (tên field giống nhau):

```csharp
public class EmployeeDto : IMapFrom<Employee> { }
```

- Cho mapping phức tạp (cần custom logic):

```csharp
public class EmployeeDto : IMapFrom<Employee>
{
    public string FullName { get; set; }
    public string RoleName { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<Employee, EmployeeDto>()
            .ForMember(d => d.RoleName, opt => opt.MapFrom(s => s.Role.ToString()));
    }
}
```

### Unit of Work & Generic Repository

Hệ thống sử dụng Unit of Work để quản lý transaction và đảm bảo tính nhất quán của dữ liệu.

**Cách dùng trong Handler:**

```csharp
// Thêm mới
await _unitOfWork.Repository<Employee>().AddAsync(employee);
// Lưu thay đổi (Đây là lúc DbContext.SaveChangesAsync được gọi)
await _unitOfWork.SaveChangeAsync(cancellationToken);
```

### Xử lý lỗi (Exception Handling)

Sử dụng các Exception tùy chỉnh để `ExceptionMiddleware` có thể trả về đúng mã lỗi HTTP:

- `NotFoundException("Message")` -> Trả về 404.
- `BusinessException("Message")` -> Trả về 400.
- Các lỗi khác -> Trả về 500.

---

## 6. EF Core Migrations

Vì dự án chia theo nhiều lớp (Domain, Infrastructure, WebUI), việc chạy lệnh Migration cần chỉ định rõ project và startup project.

### Các lệnh cơ bản (Chạy tại thư mục gốc backend/):

1. **Thêm Migration mới:**

   ```bash
   dotnet ef migrations add [MigrationName] -p src/AcadPrep.Infrastructure -s src/AcadPrep.WebUI
   ```

   _(-p là viết tắt của --project chỉ định lớp Infrastructure chứa DbContext, -s là viết tắt của --startup-project chỉ định lớp WebUI làm startup project chứa Program.cs và appsettings.json)_

2. **Cập nhật Database:**

   ```bash
   dotnet ef database update -p src/AcadPrep.Infrastructure -s src/AcadPrep.WebUI
   ```

3. **Xóa Migration cuối cùng (chưa update DB):**

   ```bash
   dotnet ef migrations remove -p src/AcadPrep.Infrastructure -s src/AcadPrep.WebUI
   ```

4. **Tạo file Script SQL (để deploy tay):**
   ```bash
   dotnet ef migrations script -p src/AcadPrep.Infrastructure -s src/AcadPrep.WebUI
   ```

### Lưu ý quan trọng:

- **PascalCase (Standard Naming):** Mặc định sử dụng chuẩn đặt tên PascalCase của Microsoft SQL Server / EF Core cho Tên Bảng và Tên Cột (ví dụ: `Employees`, `Id`, `FullName`), giúp đồng bộ với chuẩn thiết kế C#.
- **Data Seeding:** Có thể thực hiện seeding trong file `AppDbContext.cs` hoặc thông qua các class `Configuration`.
- **Migration History:** Bảng `__EFMigrationsHistory` sẽ lưu lại các migration đã chạy, đừng xóa bảng này.

---

## 7. Hướng Dẫn Chạy Project (Development Mode)

Dự án sử dụng Docker cho các dịch vụ nền (SQL Server, Redis) và chạy Backend trực tiếp trên máy host bằng `dotnet run`. Cách này giúp bạn code và debug nhanh nhất.

### Yêu cầu trước khi bắt đầu

- **.NET SDK 9** đã cài đặt (`dotnet --version` để kiểm tra)
- **Docker** và **Docker Compose** đã cài đặt
- **EF Core CLI** đã cài đặt (`dotnet tool install --global dotnet-ef`)

### Bước 1: Cấu hình file `.env`

Sao chép file mẫu và chỉnh sửa thông tin phù hợp:

```bash
cd infra/docker/dev
cp .env.example .env
# Mở file .env và sửa lại các giá trị (đặc biệt là DB_PASSWORD)
```

> **Lưu ý:** `DB_PASSWORD` phải đủ mạnh cho SQL Server (ít nhất 8 ký tự, bao gồm chữ hoa + chữ thường + số + ký tự đặc biệt). Ví dụ: `AcadPrep@12345`

### Bước 2: Khởi động Database & Redis qua Docker

```bash
cd infra/docker/dev
docker compose up -d db redis
```

Chờ đến khi SQL Server healthy (khoảng 15-20 giây):

```bash
docker compose ps
```

Kết quả mong đợi: cột STATUS hiển thị `Up ... (healthy)` cho service `db`.

### Bước 3: Đồng bộ Connection String

Mở file `backend/src/AcadPrep.WebUI/appsettings.json` và đảm bảo `DefaultConnection` khớp với `.env`:

```json
"ConnectionStrings": {
    "DefaultConnection": "Server=localhost,<DB_PORT>;Database=AcadPrepDb;User Id=sa;Password=<DB_PASSWORD>;TrustServerCertificate=True;",
    "Redis": "localhost:6379"
}
```

> Thay `<DB_PORT>` và `<DB_PASSWORD>` bằng giá trị thực tế trong file `.env`.

### Bước 4: Chạy Migration để tạo cấu trúc Database

```bash
cd backend
dotnet ef database update -p src/AcadPrep.Infrastructure -s src/AcadPrep.WebUI
```

### Bước 5: Chạy Backend

```bash
cd backend
dotnet run --project src/AcadPrep.WebUI/AcadPrep.WebUI.csproj
```

Hoặc sử dụng `dotnet watch` để tự động reload khi sửa code:

```bash
dotnet watch run --project src/AcadPrep.WebUI/AcadPrep.WebUI.csproj
```

### Bước 6: Truy cập ứng dụng

| Trang | URL |
|-------|-----|
| Razor Pages (Giao diện web) | `http://localhost:5000/Courses` |
| Swagger UI (Test API) | `http://localhost:5000/swagger` |
| Swagger JSON spec | `http://localhost:5000/swagger/v1/swagger.json` |

### Tóm tắt quy trình (Quick Reference)

```
1. docker compose up -d db redis     → Bật DB & Redis
        ↓
2. dotnet ef database update ...     → Tạo/cập nhật schema DB
        ↓
3. dotnet run / dotnet watch run     → Chạy Backend
        ↓
4. Mở http://localhost:5000/Courses  → Giao diện Razor Pages
   Mở http://localhost:5000/swagger  → Test API qua Swagger
```

### Dừng dịch vụ

```bash
# Dừng Backend: Ctrl+C trong terminal đang chạy dotnet run

# Dừng Docker (giữ lại dữ liệu DB):
cd infra/docker/dev
docker compose down

# Dừng Docker VÀ xóa toàn bộ dữ liệu DB (reset sạch):
docker compose down -v
```

---

## 8. Razor Pages — Giao Diện Web

Dự án sử dụng **Razor Pages** để xây dựng giao diện web, chạy song song với API Controllers.

### Cấu trúc thư mục

```
AcadPrep.WebUI/
├── Controllers/          ← API Controllers (trả JSON cho Swagger, Mobile App, FE khác)
├── Pages/                ← Razor Pages (giao diện web .cshtml)
│   ├── Shared/
│   │   └── _Layout.cshtml
│   ├── _ViewImports.cshtml
│   ├── _ViewStart.cshtml
│   └── Courses/
│       ├── Index.cshtml       ← View (HTML + Razor syntax)
│       └── Index.cshtml.cs    ← Code-behind (logic C#)
├── Middlewares/
└── Program.cs
```

### Nguyên tắc quan trọng

**Razor Pages gọi thẳng vào Application Layer qua MediatR**, KHÔNG gọi qua HTTP/API.

Điều này có nghĩa là:
- Razor Pages và API Controllers **dùng chung** logic nghiệp vụ (Application Layer).
- Không có code bị lặp lại (DRY - Don't Repeat Yourself).
- Không tốn chi phí mạng nội bộ (không gọi HTTP vào chính mình).

### Ví dụ code-behind chuẩn

```csharp
// Pages/Courses/Index.cshtml.cs
public class IndexModel : PageModel
{
    private readonly ISender _mediator;

    public IndexModel(ISender mediator)
    {
        _mediator = mediator;
    }

    public List<GetCourseDto> Courses { get; set; } = new();

    public async Task OnGetAsync()
    {
        // Gọi thẳng MediatR → Application Layer (KHÔNG gọi qua API)
        var response = await _mediator.Send(new GetCourseListQuery());

        if (response.IsSuccess && response.Value != null)
        {
            Courses = response.Value;
        }
    }
}
```

### So sánh Razor Pages vs API Controller

| | Razor Pages (`.cshtml.cs`) | API Controller |
|---|---|---|
| **Trả về** | HTML (giao diện web) | JSON (dữ liệu) |
| **Dùng cho** | Giao diện web hiện tại | Swagger, Mobile App, FE khác |
| **Gọi logic** | `ISender` (MediatR) trực tiếp | `ISender` (MediatR) trực tiếp |
| **URL mẫu** | `/Courses` | `/api/Courses` |

### Cách thêm Razor Page mới

1. Tạo thư mục `Pages/<Feature>/` (ví dụ: `Pages/Students/`).
2. Tạo file `Index.cshtml` (View) và `Index.cshtml.cs` (Code-behind).
3. Trong code-behind, inject `ISender` và gọi Query/Command tương ứng từ Application Layer.
4. Truy cập page tại `http://localhost:5000/<Feature>` (ví dụ: `/Students`).

### Tích hợp Frontend bên ngoài (Tương lai)

Khi cần chuyển sang dùng React/Vue/Next.js:
- Các API Controllers (`/api/...`) đã sẵn sàng và có CORS được cấu hình trong `Program.cs`.
- Frontend bên ngoài chỉ cần gọi vào các endpoint `/api/...` là hoạt động.
- Razor Pages có thể giữ lại làm trang Admin nội bộ hoặc xóa bỏ tùy ý.

---

^**\_**^
