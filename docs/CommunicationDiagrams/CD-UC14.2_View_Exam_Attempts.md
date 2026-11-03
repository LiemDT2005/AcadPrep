# Communication Diagram – UC-14.2: View Exam Attempts (Danh sách lượt thi thử)

> **Diagram Source:** [`CD-UC14.2_View_Exam_Attempts.puml`](./CD-UC14.2_View_Exam_Attempts.puml)  
> **Tool:** PlantUML (tương thích Visual Paradigm, VS Code)  
> **Author:** Senior Developer  
> **Date:** 2026-07-19

---

## 1. Participants (Các thành phần tham gia)

| Participant | Class / Type | Layer | Vai trò |
|---|---|---|---|
| **Student** | Actor | – | Học viên muốn theo dõi các lượt thi thử đã thực hiện |
| **:Profile.cshtml (Attempts Tab)** | View | WebUI | Giao diện hiển thị bảng danh sách các lượt thi thử kèm điểm số |
| **:ProfileModel** | PageModel | WebUI | Nhận yêu cầu HTTP GET từ client với tham số phân trang, bộ lọc |
| **:GetExamAttemptsQuery** | Query | Application | Truy vấn lấy danh sách lượt làm bài của học viên |
| **:GetExamAttemptsQueryHandler** | Query Handler | Application | Xử lý logic đọc dữ liệu lượt thi thử, tính thời gian làm bài thực tế |
| **:IAppDbContext** | DbContext | Infrastructure | Interface tương tác với CSDL qua EF Core |
| **SQL Server** | Database (SQL) | External | Cơ sở dữ liệu lưu các bảng `ExamAttempts` và `Exams` |

---

## 2. Vòng đời dữ liệu & Luồng giao tiếp

### A. Yêu cầu xem danh sách lượt thi thử (Request Phase)
1. Học viên mở trang cá nhân và chọn tab **"Lượt thi thử"** (Exam Attempts).
2. Trình duyệt gửi yêu cầu HTTP GET `/Account/Profile?tab=attempts`.
3. PageModel `ProfileModel` nhận yêu cầu, trích xuất `userId` người dùng đăng nhập hiện tại từ `ICurrentUserService`.
4. PageModel tạo và gửi `GetExamAttemptsQuery(userId)` đi qua MediatR.

### B. Truy vấn và Tính toán thời gian (Processing Phase)
1. `GetExamAttemptsQueryHandler` nhận truy vấn.
2. Handler thông qua `IAppDbContext` thực hiện truy vấn bảng `ExamAttempts`:
   - Lọc theo điều kiện `UserId == userId` và `IsSubmitted == true`.
   - Kết hợp (`Join`) với bảng `Exams` để lấy ra tên của đề thi (`ExamTitle`).
   - Sắp xếp kết quả theo thời gian nộp bài (`CompletedAt`) mới nhất lên trước.
3. Database SQL Server thực thi câu lệnh SQL SELECT và trả về danh sách các hàng dữ liệu.
4. Handler duyệt qua danh sách thực thể nhận được để:
   - Tính toán thời gian làm bài thực tế: `Duration = CompletedAt - StartedAt`.
   - Đóng gói dữ liệu sang danh sách `ExamAttemptDto` (chứa: `AttemptId`, `ExamTitle`, `ListeningScore`, `ReadingScore`, `TotalScore`, `Duration`, `CompletedAt`).
5. Handler trả danh sách DTO về cho PageModel.

### C. Hiển thị danh sách lượt thi (Rendering Phase)
1. PageModel nhận danh sách DTO, đưa vào thuộc tính liên kết và thực hiện kết xuất (render) HTML cho tab lượt làm bài.
2. Trình duyệt hiển thị bảng dữ liệu trực quan:
   - Các cột: Tên đề thi, Ngày thi, Thời gian làm bài, Điểm Nghe, Điểm Đọc, Tổng điểm.
   - Thêm nút liên kết **"Xem lại"** (Review) ở mỗi lượt thi, trỏ trực tiếp đến trang xem chi tiết câu trả lời đúng/sai (`/Exams/Review?attemptId=...`).

---

## 3. Quy tắc nghiệp vụ (Business Rules) áp dụng

- **BR-Submitted-Only:** Chỉ hiển thị những lượt thi thử đã được nộp hoàn chỉnh (`IsSubmitted == true`). Các lượt thi đang làm dở sẽ không hiển thị tại đây (chỉ hiển thị banner Resume ở trang chi tiết đề thi hoặc Dashboard).
- **BR-Owner-Authorization:** Chỉ hiển thị lượt thi của người dùng đang đăng nhập. Chặn truy cập từ các tài khoản khác để bảo mật thông tin học tập cá nhân.

---

## 4. Hướng dẫn import vào Visual Paradigm

1. Tạo một **Communication Diagram** mới trong Visual Paradigm.
2. Sao chép nội dung code trong file [`CD-UC14.2_View_Exam_Attempts.puml`](./CD-UC14.2_View_Exam_Attempts.puml).
3. Import sơ đồ bằng chức năng **Import from PlantUML**.
