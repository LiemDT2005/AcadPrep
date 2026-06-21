# Sequence Diagram – UC-3.2: View Exam Detail

> **Diagram Source:** [`SD-UC3.2_View_Exam_Detail.puml`](./SD-UC3.2_View_Exam_Detail.puml)  
> **Tool:** PlantUML (tương thích Visual Paradigm, IntelliJ PlantUML Plugin, VS Code)  
> **Author:** Senior Developer  
> **Date:** 2026-06-09

---

## 1. Participants (Các thành phần tham gia)

| Participant | Class / Type | Layer | Vai trò |
|---|---|---|---|
| **Guest** | Actor | – | Người dùng nhấp vào exam card để xem chi tiết đề thi |
| **:Detail.cshtml** | View | WebUI | Giao diện hiển thị HTML trang chi tiết đề thi |
| **:DetailModel** | PageModel (Razor Page) | WebUI | Entry point server-side; inject `ISender` gọi trực tiếp Application |
| **:ValidationBehavior** | Pipeline Behavior | Application | Pipeline kiểm tra tính hợp lệ của query (examId không rỗng) |
| **:GetExamDetailQueryHandler** | Query Handler | Application | Xử lý logic nghiệp vụ: kiểm tra cache, query DB, lưu cache, map DTO |
| **:ICacheService** | Cache Service | Infrastructure | Interface tương tác với Redis Cache (Cache-Aside pattern) |
| **Redis** | Database (NoSQL) | External | Hệ thống cache lưu dữ liệu in-memory |
| **:IAppDbContext** | DbContext | Infrastructure | Interface truy vấn cơ sở dữ liệu qua EF Core |
| **SQL Server** | Database (SQL) | External | Cơ sở dữ liệu quan hệ lưu trữ thông tin đề thi |

---

## 2. Vòng đời dữ liệu & Các nhánh xử lý (alt / else)

### A. Nhánh Cache HIT (Dữ liệu có sẵn ở Redis)
- Guest nhấp vào một exam card → trình duyệt gửi request HTTP GET `/Exams/{id}`.
- `DetailModel` gửi `GetExamDetailQuery(examId)` qua MediatR.
- `GetExamDetailQueryHandler` kiểm tra Redis với key `ExamDetail:{id}` và thấy dữ liệu có sẵn.
- Trả về dữ liệu trực tiếp, bỏ qua việc query Database.
- Render trang `Detail.cshtml` và hiển thị thông tin chi tiết đề thi.

### B. Nhánh Cache MISS (Không có sẵn ở Redis)
- Kiểm tra Redis thất bại (trả về `null`/`nil`).
- Handler query database thông qua `IAppDbContext` lấy exam theo `Id`, với điều kiện chưa bị xóa (`!IsDeleted`) và đã xuất bản (`Status == "Published"`).
- **Trường hợp thành công (Exam found):**
  - Lưu dữ liệu vừa lấy từ Database vào Redis Cache với TTL 30 phút (**BR-13**).
  - Trả kết quả về cho `DetailModel` dưới dạng DTO chứa: tên, mô tả, năm, cấu trúc phần thi (**BR-16**: Part 1–7), tổng thời gian, số lượt làm bài, điểm trung bình.
  - **Hiển thị nút hành động dựa trên trạng thái đăng nhập:**
    - *Guest chưa đăng nhập (Normal Flow):* Hiển thị nút **"Log in to start"**.
    - *User đã đăng nhập (A1):*
      - Chưa từng làm bài → Nút **"Start"** → UC-8.1.
      - Đang làm dở dang → Nút **"Resume"** (**BR-04**: timer & answers đóng băng) → UC-8.5.
      - Đã hoàn thành → Nút **"Retake"** / **"View Result"** (**BR-05**: điểm chỉ ghi nhận khi Submit hoặc hết giờ) → UC-9.1.
- **Trường hợp Exam không tồn tại hoặc đã bị xóa mềm (E1, BR-15):**
  - Database trả về `null`.
  - Handler throw `NotFoundException` (HTTP 404).
  - PageModel redirect về trang `/Exams` kèm thông báo lỗi.

---

## 3. Quy tắc nghiệp vụ (Business Rules) trong diagram

| BR ID | Tên | Vị trí trong diagram |
|---|---|---|
| **BR-15** | Referential Integrity on Exam Deletion | Bước query DB với điều kiện `!IsDeleted && Status == Published`; nhánh E1 xử lý 404 khi exam bị soft-deleted |
| **BR-13** | Cache TTL 30 phút | Bước `SetAsync("ExamDetail:{id}", data, TTL 30m)` |
| **BR-16** | Exam and Question Structural Validation | Dữ liệu trả về bao gồm cấu trúc Part 1–7 chính xác |
| **BR-04** | Full Test Interruption Control | Nút "Resume" hiển thị khi có phiên thi dở dang |
| **BR-05** | Full Test Scoring Trigger Constraints | Nút "View Result" chỉ hiển thị khi bài thi đã được Submit hoặc hết giờ |

---

## 4. Hướng dẫn import vào Visual Paradigm

1. Tạo một **Sequence Diagram** mới trong Visual Paradigm.
2. Chọn chức năng **Import from PlantUML** (hoặc dùng tổ hợp phím tắt hỗ trợ).
3. Copy toàn bộ code trong file [`SD-UC3.2_View_Exam_Detail.puml`](./SD-UC3.2_View_Exam_Detail.puml) và paste vào cửa sổ import.
4. Nhấn **OK** để công cụ tự động dựng sơ đồ theo chuẩn style tối giản đã khai báo.
