# Communication Diagram – UC-9.4: Submit Test (Xử lý nộp bài thi)

> **Diagram Source:** [`CD-UC9.4_Submit_Test.puml`](./CD-UC9.4_Submit_Test.puml)  
> **Tool:** PlantUML (tương thích Visual Paradigm, VS Code)  
> **Author:** Senior Developer  
> **Date:** 2026-07-19

---

## 1. Participants (Các thành phần tham gia)

| Participant | Class / Type | Layer | Vai trò |
|---|---|---|---|
| **Student** | Actor | – | Học viên làm bài thi và nộp bài |
| **:Take.cshtml** | View | WebUI | Giao diện hiển thị bài thi, các nút bấm nộp bài và các modal xác nhận |
| **exam-session.js** | Client Script | WebUI (Static Asset) | Script quản lý bộ đếm ngược, xử lý sự kiện click nộp bài, gửi request API nộp bài |
| **:Take.cshtml.cs** | PageModel | WebUI | Nhận request HTTP POST nộp bài qua handler `Submit` |
| **:SubmitTestCommand** | Command | Application | DTO chứa thông tin tham số nộp bài (AttemptId, UserId, RemainingSeconds) |
| **:SubmitTestCommandHandler** | Command Handler | Application | Xử lý logic nghiệp vụ chấm điểm tự động, cập nhật trạng thái bài thi |
| **:IAppDbContext** | DbContext | Infrastructure | Interface truy vấn và lưu trữ kết quả thi |
| **SQL Server** | Database (SQL) | External | Cơ sở dữ liệu lưu trữ thông tin thực tế của bài thi và lượt làm bài |

---

## 2. Vòng đời dữ liệu & Luồng giao tiếp

### A. Kích hoạt nộp bài (Trường hợp 1: Chủ động nộp bài)
1. Học viên click vào nút **"Submit"** trên Header.
2. `exam-session.js` hiển thị modal xác nhận (`modal-submit`) thống kê số câu đã làm, chưa làm.
3. Học viên click nút **"Submit"** trong modal để xác nhận nộp bài.
4. Script gọi hàm `submitTest()` để bắt đầu gửi yêu cầu nộp bài lên Server.

### B. Kích hoạt nộp bài (Trường hợp 2: Hết giờ làm bài)
1. Bộ đếm ngược thời gian trong `exam-session.js` đạt giá trị `0`.
2. Script hiển thị modal thông báo hết giờ (`modal-timeout`) kèm theo hiệu ứng spinner tải trang.
3. Script tự động gọi hàm `submitTest()` để gửi yêu cầu nộp bài lên Server mà không cần học viên xác nhận.

### C. Xử lý chấm điểm & lưu trữ tại Server
1. Script gửi request HTTP POST dạng JSON đến `/Exams/Take?handler=Submit`.
2. `TakeModel.OnPostSubmitAsync` giải mã request, kiểm tra thông tin định danh `UserId` từ `ICurrentUserService`.
3. Khởi tạo `SubmitTestCommand` và gửi qua MediatR.
4. `SubmitTestCommandHandler` nhận command:
   - Tải thực thể `ExamAttempt` tương ứng từ Database.
   - Truy vấn danh sách toàn bộ đáp án của câu hỏi thuộc đề thi đó.
   - So khớp các đáp án đã chọn (`SelectedOption`) của học viên với đáp án đúng (`CorrectOption`) của câu hỏi.
   - Tính toán số câu đúng cho hai phần: Listening (Nghe) và Reading (Đọc).
   - Áp dụng bảng quy đổi điểm TOEIC chuẩn (TOEIC Score Conversion Table) để tính ra `ListeningScore`, `ReadingScore` (thang điểm tối đa 495 mỗi phần) và `TotalScore` (tối đa 990).
   - Cập nhật trạng thái `IsSubmitted = true`, thời gian nộp bài `CompletedAt = DateTime.UtcNow`, và các điểm số vào đối tượng `ExamAttempt`.
5. Thực hiện lưu thay đổi (`SaveChangesAsync()`) vào cơ sở dữ liệu.
6. Server trả về kết quả JSON thông báo thành công và ID lượt làm bài.
7. Client nhận phản hồi thành công, tiến hành xóa trạng thái làm bài tạm thời trong `localStorage` (`clearLocalProgress()`).
8. Client chuyển hướng học viên sang trang kết quả `/Exams/Results?attemptId=...`.
9. Giao diện trang kết quả hiển thị bảng điểm chi tiết (Listening, Reading, Total Score) cùng các tùy chọn làm lại hoặc xem chi tiết từng câu.

---

## 3. Quy tắc nghiệp vụ (Business Rules) áp dụng

- **BR-Grading-Strict:** Điểm thi thử được chấm dựa trên đáp án chính thức lưu trữ trên hệ thống. Tổng điểm = Điểm phần nghe + Điểm phần đọc (quy đổi từ số câu đúng theo bảng điểm chuẩn).
- **BR-State-Lock:** Sau khi đã nộp bài (`IsSubmitted = true`), học viên không được phép thay đổi hay thêm bất kỳ đáp án nào cho lượt làm bài này nữa.
- **BR-Local-Clean:** Xóa sạch bản lưu nháp trong local storage của trình duyệt ngay sau khi nộp bài thành công để tránh lỗi xung đột dữ liệu.

---

## 4. Hướng dẫn import vào Visual Paradigm

1. Tạo một **Communication Diagram** mới trong Visual Paradigm.
2. Sao chép toàn bộ code trong file [`CD-UC9.4_Submit_Test.puml`](./CD-UC9.4_Submit_Test.puml).
3. Sử dụng chức năng **Import from PlantUML** để import sơ đồ tương tác.
