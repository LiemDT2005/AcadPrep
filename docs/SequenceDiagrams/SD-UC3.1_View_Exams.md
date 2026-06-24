# Sequence Diagram – UC-3.1: View Exams

> **Diagram Source:** [`SD-UC3.1_View_Exams.puml`](./SD-UC3.1_View_Exams.puml)  
> **Tool:** PlantUML (tương thích Visual Paradigm, IntelliJ PlantUML Plugin, VS Code)  
> **Author:** Senior Developer  
> **Date:** 2026-06-06

---

## 1. Participants (Các thành phần tham gia)

| Participant | Class / Type | Layer | Vai trò |
|---|---|---|---|
| **Guest** | Actor | – | Người dùng truy cập trang danh sách đề thi |
| **:Index.cshtml** | View | WebUI | Giao diện hiển thị HTML danh sách đề thi |
| **:IndexModel** | PageModel (Razor Page) | WebUI | Entry point server-side; inject `ISender` gọi trực tiếp Application |
| **:ValidationBehavior** | Pipeline Behavior | Application | Pipeline kiểm tra tính hợp lệ của query trước khi xử lý |
| **:GetExamListQueryHandler** | Query Handler | Application | Xử lý logic nghiệp vụ: kiểm tra cache, query DB, lưu cache |
| **:ICacheService** | Cache Service | Infrastructure | Interface tương tác với Redis Cache (Cache-Aside pattern) |
| **Redis** | Database (NoSQL) | External | Hệ thống cache lưu dữ liệu in-memory |
| **:IAppDbContext** | DbContext | Infrastructure | Interface truy vấn cơ sở dữ liệu qua EF Core |
| **SQL Server** | Database (SQL) | External | Cơ sở dữ liệu quan hệ lưu trữ thông tin đề thi |

---

## 2. Vòng đời dữ liệu & Các nhánh xử lý (alt / else)

### A. Nhánh Cache HIT (Dữ liệu có sẵn ở Redis)
- Trình duyệt gửi request HTTP GET `/Exams`.
- `IndexModel` gửi Query qua MediatR.
- `GetExamListQueryHandler` kiểm tra Redis và thấy dữ liệu có sẵn.
- Trả về dữ liệu trực tiếp, bỏ qua việc query Database.
- Render trang `Index.cshtml` và hiển thị danh sách đề thi.

### B. Nhánh Cache MISS (Không có sẵn ở Redis)
- Kiểm tra Redis thất bại (trả về `null`/`nil`).
- Handler query database thông qua `IAppDbContext` lấy các exam chưa bị xóa (`!IsDeleted`) và đã xuất bản (`Status == "Published"`).
- **Trường hợp thành công (Success):**
  - Lưu dữ liệu vừa lấy từ Database vào Redis Cache với TTL (30 phút).
  - Trả kết quả về cho `IndexModel`.
  - Nếu danh sách rỗng (A1): Hiển thị thông báo không có đề thi.
  - Nếu danh sách có dữ liệu: Render và hiển thị danh sách dạng card.
- **Trường hợp lỗi kết nối Database (E1):**
  - Trả exception ngược lại.
  - Razor Page chuyển hướng sang Error Page hiển thị lỗi hệ thống.

---

## 3. Quy tắc nghiệp vụ (Business Rules) trong diagram

- **BR-13 (TTL 30m):** Được thể hiện qua bước `SetAsync("ExamList", items, TTL 30m)`.
- **BR-15 (Soft Delete & Published):** Thể hiện ở bước query DB với điều kiện lọc hoạt động và đã xuất bản.
- **BR-22 / BR-23:** Kiểm soát trạng thái filter và pagination nằm trực tiếp tại `IndexModel` và `Index.cshtml` trước/sau khi gọi query.

---

## 4. Hướng dẫn import vào Visual Paradigm

1. Tạo một **Sequence Diagram** mới trong Visual Paradigm.
2. Chọn chức năng **Import from PlantUML** (hoặc dùng tổ hợp phím tắt hỗ trợ).
3. Copy toàn bộ code trong file [`SD-UC3.1_View_Exams.puml`](./SD-UC3.1_View_Exams.puml) và paste vào cửa sổ import.
4. Nhấn **OK** để công cụ tự động dựng sơ đồ theo chuẩn style tối giản đã khai báo.
