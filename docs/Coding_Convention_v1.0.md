## 1\. Quy định Chung (General)

### Git & Branching

- **Main branch**: `main` (Production), `develop` (Staging/Development), .  
- **Branch naming**:  
  - `feat/feature-name`: Tính năng mới.  
  - `fix/bug-name`: Sửa lỗi.  
  - `refactor/name`: Tối ưu code.  
- **Conventional Commits**:  
  - `feat: ...`, `fix: ...`, `docs: ...`, `style: ...`, `refactor: ...`, `chore: ...`.

### General Naming

- Tên biến/hàm phải có ý nghĩa (Self-documenting). Tránh đặt `a`, `b`, `c`.

---

## 2\. Tiêu chuẩn Backend (C\# / .NET)

### Kiến trúc (Clean Architecture)

- **Domain**: Entities, Interfaces repository (No dependencies).  
- **Application**: Logic nghiệp vụ, Services, DTOs, Validators.  
- **Infrastructure**: Implement repositories, DbContext, Third-party services.  
- **Presentation (API)**: Controllers, Middlewares.

### Result & Exception Pattern

- **Result Pattern**: Sử dụng `Result<T>` để trả về từ Service. Không dùng exception cho lỗi logic (ví dụ: `UserNotFound`).  
- **Exception**: Chỉ dùng cho lỗi bất khả kháng (DB sập, lỗi code). Bắt tập trung tại Middleware.

### Giao dịch & Dữ liệu (Transaction & EF Core)

- **Transaction**: Mọi hoạt động ghi (Insert/Update/Delete) liên quan nhiều hơn 1 bảng phải nằm trong Transaction.  
- **Unit of Work**: Chỉ gọi `SaveChangesAsync()` một lần duy nhất tại cuối Service/Action.  
- **EF Core**:  
  - Ưu tiên `.AsNoTracking()` cho truy vấn Read-only.  
  - Tách Configuration ra file riêng (`IEntityTypeConfiguration`).

---

## 3\. Tiêu chuẩn Frontend (Next.js / TypeScript)

### Naming Conventions

- **Components**: PascalCase (ví dụ: `ButtonSubmit.tsx`).  
- **Hooks**: Bắt đầu bằng `use` (ví dụ: `useAuth.ts`).  
- **Folders/Files**: kebab-case (ví dụ: `user-profile/`).  
- **Variables/Methods**: camelCase.  
- **Type**: PascalCase (ví dụ: User \= {} )

### Cấu trúc Folder (App Router)

- `src/app`: Định nghĩa routes.  
- `src/components`: Các shared components (UI, Common).  
- `src/hooks`: Custom hooks dùng chung.  
- `src/lib`: Cấu hình thư viện (Axios, Auth).  
- `src/services`: Gọi API (mapping với BE).  
- `src/types`: Định nghĩa interfaces/types cho TypeScript.  
- `src/store`: Định nghĩa Redux store.

### Nguyên tắc Component

- **Atomic Design**: Cố gắng tách nhỏ component để tái sử dụng.  
- **Props**: Luôn định nghĩa Type cho Props. Tránh dùng `any`.  
- **Client vs Server Component**:  
  - Tận dụng Server Component cho SEO và Load dữ liệu.  
  - Chỉ dùng Client Component khi cần tương tác (onClick, useState, useEffect).

---

## 4\. Giao tiếp & Bảo mật (API & Security)

### RESTful API

- URL: Số nhiều, chữ thường (`/api/products`).  
- Method: `GET` (Read), `POST` (Create), `PUT` (Update All), `PATCH` (Partial Update), `DELETE`.

### Security

- **Authentication**: JWT Bearer lưu tại Redux State. Refresh token lưu trong cookie  
- **Validation**:  
  - BE: Kiểm tra bằng FluentValidation.  
  - FE: Kiểm tra bằng Zod/Yup trước khi gửi request.  
- **Secrets**: Không push `.env` hoặc `appsettings.json` chứa Key lên Git.

---

## 5\. Quy trình Review & Testing

- **Pull Request (PR)**: Mỗi PR phải được ít nhất 1 thành viên khác review trước khi merge.  
  không được push thẳng lên nhánh main và dev, cần tạo các nhánh riêng theo quy ước và tạo PR  
- **Unit Test**: Viết test cho các Logic nghiệp vụ phức tạp ở Service (BE) và Hooks quan trọng (FE).

