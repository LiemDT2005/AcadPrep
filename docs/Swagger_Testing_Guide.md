# Hướng Dẫn Sử Dụng Swagger Để Test API

Tài liệu này hướng dẫn cách sử dụng Swagger UI để kiểm thử các API endpoint của hệ thống AcadPrep trong quá trình phát triển.

---

## 1. Swagger Là Gì?

**Swagger (OpenAPI)** là công cụ tự động tạo giao diện trực quan cho các API, cho phép developer:

- Xem tất cả các endpoint đã expose.
- Xem chi tiết request body, parameters, và response schema.
- **Gửi request trực tiếp** từ trình duyệt mà không cần Postman hay cURL.
- Xem mã HTTP response và dữ liệu trả về.

---

## 2. Cách Truy Cập Swagger

### Bước 1: Khởi động Infrastructure (Docker)

```bash
cd /home/thanh-liem/Private/AcadPrep/infra/docker/dev
docker compose up -d db redis
```

Chờ đến khi SQL Server healthy (khoảng 15-20 giây):

```bash
docker compose ps
```

### Bước 2: Chạy Backend

```bash
cd /home/thanh-liem/Private/AcadPrep/backend
dotnet run --project src/AcadPrep.WebUI/AcadPrep.WebUI.csproj
```

### Bước 3: Mở Swagger UI

Mở trình duyệt và truy cập:

```
http://localhost:5000/swagger
```

> **Lưu ý:** Swagger UI được cấu hình tại đường dẫn `/swagger`. Giao diện web Razor Pages nằm tại các đường dẫn khác (ví dụ: `/Courses`).

Nếu cần truy cập Swagger spec (JSON):

```
http://localhost:5000/swagger/v1/swagger.json
```

---

## 3. Cách Test Từng API

### 3.1. Test GET — Lấy danh sách khóa học

| Thông tin | Giá trị |
|-----------|---------|
| **Method** | `GET` |
| **URL** | `/api/Courses` |
| **Auth** | Không yêu cầu (hiện tại) |

**Các bước:**

1. Tìm section **Courses** trên Swagger UI.
2. Click vào dòng **GET /api/Courses**.
3. Click nút **"Try it out"**.
4. Click nút **"Execute"**.
5. Xem kết quả ở phần **Response body**.

**Response mẫu (200 OK):**

```json
{
  "isSuccess": true,
  "message": "Lấy danh sách khóa học thành công",
  "value": [
    {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "title": "Tiếng Anh Giao Tiếp Cơ Bản",
      "description": "Khóa học cho người mới bắt đầu",
      "level": "Beginner",
      "price": 500000,
      "createdDate": "2026-05-21T00:00:00"
    }
  ],
  "errors": null
}
```

**Response khi DB trống (200 OK):**

```json
{
  "isSuccess": true,
  "message": "Lấy danh sách khóa học thành công",
  "value": [],
  "errors": null
}
```

---

### 3.2. Test POST — Tạo khóa học mới

| Thông tin | Giá trị |
|-----------|---------|
| **Method** | `POST` |
| **URL** | `/api/Courses` |
| **Content-Type** | `application/json` |
| **Auth** | Không yêu cầu (hiện tại) |

**Các bước:**

1. Tìm section **Courses** trên Swagger UI.
2. Click vào dòng **POST /api/Courses**.
3. Click nút **"Try it out"**.
4. Nhập Request Body (xem mẫu bên dưới).
5. Click nút **"Execute"**.
6. Xem kết quả ở phần **Response body** và **Response code** (201 Created).

**Request Body mẫu:**

```json
{
  "title": "Tiếng Anh Giao Tiếp Cơ Bản",
  "description": "Khóa học giúp bạn tự tin giao tiếp tiếng Anh hàng ngày",
  "level": "Beginner",
  "price": 500000
}
```

**Response mẫu (201 Created):**

```json
{
  "isSuccess": true,
  "message": "Tạo khóa học thành công",
  "value": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "errors": null
}
```

**Response khi Validation lỗi (400 Bad Request):**

```json
{
  "isSuccess": false,
  "message": "Dữ liệu không hợp lệ",
  "value": null,
  "errors": [
    {
      "propertyName": "Title",
      "errorMessage": "Tiêu đề không được để trống."
    }
  ]
}
```

---

## 4. Kiểm Tra Redis Cache Hoạt Động

Để xác nhận Redis cache đang hoạt động đúng:

### Bước 1: Gọi GET lần đầu

1. Gọi **GET /api/Courses** → Swagger trả về danh sách.
2. Quan sát **Console log** của backend → sẽ thấy EF Core query SQL.

### Bước 2: Gọi GET lần thứ hai

1. Gọi lại **GET /api/Courses** ngay sau đó.
2. Quan sát **Console log** → **Không có SQL query** nào được thực thi.
3. Response trả về nhanh hơn rõ rệt → dữ liệu đến từ Redis cache.

### Bước 3: Kiểm tra trực tiếp trong Redis (tùy chọn)

```bash
# Kết nối vào Redis container
docker exec -it acadprep_redis redis-cli

# Kiểm tra key đã được cache
KEYS *

# Xem giá trị của key CourseList
GET CourseList

# Xem TTL còn lại
TTL CourseList
```

---

## 5. Xử Lý Lỗi Thường Gặp

### 5.1. Không truy cập được Swagger

| Nguyên nhân | Cách xử lý |
|-------------|------------|
| Backend chưa chạy | Chạy `dotnet run --project src/AcadPrep.WebUI/AcadPrep.WebUI.csproj` |
| Sai port | Kiểm tra console log để xem port thực tế |
| Không ở Development | Swagger chỉ hiện khi `ASPNETCORE_ENVIRONMENT=Development` |

### 5.2. Lỗi 500 Internal Server Error

| Nguyên nhân | Cách xử lý |
|-------------|------------|
| DB chưa chạy | `docker compose up -d db` và chờ healthy |
| Redis chưa chạy | `docker compose up -d redis` |
| Chưa chạy migration | `dotnet ef database update -p src/AcadPrep.Infrastructure -s src/AcadPrep.WebUI` |
| Connection string sai | Kiểm tra `appsettings.json` → `ConnectionStrings` |

### 5.3. Response trả về nhưng data rỗng

- Database chưa có dữ liệu → Dùng **POST** để tạo trước.
- Kiểm tra điều kiện filter `IsDeleted = false` và `IsActive = true`.

---

## 6. Cấu Hình Swagger Trong Code

Swagger được đăng ký trong `Program.cs`:

```csharp
// Đăng ký Swagger service
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Kích hoạt Swagger UI (chỉ trong Development)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "AcadPrep API v1");
        // Swagger UI tại /swagger (mặc định). Root URL "/" dành cho Razor Pages.
    });
}
```

### Tùy chỉnh nâng cao (khi cần)

Khi project phát triển thêm (ví dụ JWT Auth), có thể bổ sung cấu hình Swagger để hỗ trợ gửi token:

```csharp
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "AcadPrep API",
        Version = "v1",
        Description = "API cho hệ thống học tiếng Anh AcadPrep"
    });

    // Thêm nút Authorize cho JWT Bearer token
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Nhập JWT token: Bearer {token}"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});
```

---

## 7. Swagger vs Razor Pages — Phạm Vi Test

### Swagger test cái gì?

Swagger **chỉ** hiển thị và test các **API Controllers** (các endpoint `/api/...`). Razor Pages (`/Courses`, `/Students`,...) **không xuất hiện** trên Swagger vì chúng trả về HTML thay vì JSON.

### Tại sao test Swagger vẫn đảm bảo Razor Pages hoạt động?

Vì cả hai đều gọi **cùng một logic nghiệp vụ** trong Application Layer:

```
Swagger (API Controller)  ──→  ISender.Send(GetCourseListQuery)  ──→  Application Layer
                                        ↑ Cùng logic                        │
Razor Pages (Code-behind) ──→  ISender.Send(GetCourseListQuery)  ──→  Application Layer
```

Nếu Swagger trả về dữ liệu đúng → Razor Pages cũng sẽ nhận được dữ liệu đúng.

### Quy trình test đầy đủ

| Bước | Công cụ | URL | Mục đích |
|------|---------|-----|----------|
| 1 | **Swagger** | `http://localhost:5000/swagger` | Test API trả đúng JSON, validation, cache |
| 2 | **Trình duyệt** | `http://localhost:5000/Courses` | Test giao diện hiển thị đúng, layout, dữ liệu |

### Ví dụ cụ thể cho feature Courses

**Test 1 — Tạo dữ liệu qua Swagger:**

1. Mở `http://localhost:5000/swagger`
2. Tìm **POST /api/Courses** → Click **Try it out** → Nhập body → Click **Execute**
3. Xác nhận response trả về thành công (có `isSuccess: true` và `value` chứa ID)

**Test 2 — Kiểm tra API đọc dữ liệu qua Swagger:**

1. Tìm **GET /api/Courses** → Click **Try it out** → Click **Execute**
2. Xác nhận response trả về danh sách có chứa course vừa tạo

**Test 3 — Kiểm tra giao diện Razor Pages:**

1. Mở trình duyệt tại `http://localhost:5000/Courses`
2. Xác nhận trang web hiển thị danh sách khóa học giống dữ liệu trong Swagger
3. Kiểm tra layout, bảng, badge hiển thị đúng

> **Mẹo:** Nếu Swagger trả đúng dữ liệu nhưng Razor Pages hiển thị sai hoặc trống → lỗi nằm ở file `.cshtml` (View), không phải logic nghiệp vụ.

---

## 8. Quy Trình Test Chuẩn (Workflow)

```
1. docker compose up -d db redis    → Bật DB & Redis
        ↓
2. dotnet ef database update ...    → Tạo/cập nhật schema DB
        ↓
3. dotnet run                       → Chạy Backend
        ↓
4. Mở http://localhost:5000/swagger → Test API (tạo dữ liệu mẫu)
        ↓
5. POST /api/Courses                → Tạo khóa học mẫu
        ↓
6. GET /api/Courses                 → Kiểm tra response JSON
        ↓
7. GET lần 2                        → Xác nhận cache (không có SQL log)
        ↓
8. Mở http://localhost:5000/Courses → Kiểm tra giao diện Razor Pages
```

---

## 9. Script Test Nhanh — Create Exam API

Dùng script sau để test `POST /api/Exams` qua curl (tương đương Swagger Try it out):

```bash
# Chạy backend trước
dotnet run --project backend/src/AcadPrep.WebUI/AcadPrep.WebUI.csproj

# Terminal khác
./backend/scripts/test-swagger-create-exam.sh

# Tùy chọn: đổi port hoặc exam series ID
BASE_URL=http://localhost:5001 EXAM_SERIES_ID=1 ./backend/scripts/test-swagger-create-exam.sh
```

> **Lưu ý:** UI tạo exam trên `/Admin/Exams` dùng **Razor Pages code-behind** (`OnPostCreateAsync`), không gọi API. Controller `/api/Exams` giữ lại để test Swagger.

---
