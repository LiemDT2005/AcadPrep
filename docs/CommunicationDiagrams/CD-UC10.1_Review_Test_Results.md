# Communication Diagram – UC-10.1: Review Test Results (Xem lại kết quả chi tiết)

> **Diagram Source:** [`CD-UC10.1_Review_Test_Results.puml`](./CD-UC10.1_Review_Test_Results.puml)  
> **Tool:** PlantUML (tương thích Visual Paradigm, VS Code)  
> **Author:** Senior Developer  
> **Date:** 2026-07-19

---

## 1. Participants (Các thành phần tham gia)

| Participant | Class / Type | Layer | Vai trò |
|---|---|---|---|
| **Student** | Actor | – | Học viên muốn xem chi tiết bài làm của mình |
| **:Review.cshtml** | View | WebUI | Giao diện hiển thị chi tiết câu hỏi, đáp án đã chọn, đáp án hệ thống và giải thích |
| **:ReviewModel** | PageModel | WebUI | Entry point server-side nhận request HTTP GET `/Exams/Review` |
| **:GetExamReviewQuery** | Query | Application | Đối tượng truy vấn chứa tham số `AttemptId` và `UserId` |
| **:GetExamReviewQueryHandler** | Query Handler | Application | Xử lý logic kết xuất dữ liệu chi tiết so sánh đáp án và tổng hợp lời giải |
| **:IAppDbContext** | DbContext | Infrastructure | Interface truy cập CSDL để đọc thông tin bài thi và kết quả làm bài |
| **SQL Server** | Database (SQL) | External | Cơ sở dữ liệu chứa bảng câu hỏi, lời giải, đáp án học viên và đáp án đúng |

---

## 2. Vòng đời dữ liệu & Luồng giao tiếp

### A. Yêu cầu xem lại bài làm (Request Phase)
1. Học viên click vào nút **"Review answers"** trên trang kết quả thi (hoặc trang lịch sử lượt thi).
2. Trình duyệt gửi yêu cầu HTTP GET đến trang `/Exams/Review?attemptId=...`.
3. PageModel `ReviewModel` nhận yêu cầu, trích xuất `attemptId` và xác định `userId` của người dùng đang đăng nhập thông qua `ICurrentUserService`.
4. PageModel tạo và gửi `GetExamReviewQuery(attemptId, userId)` đi qua MediatR.

### B. Xử lý nghiệp vụ tại Server (Processing Phase)
1. `GetExamReviewQueryHandler` nhận yêu cầu và tiến hành:
   - Truy vấn thông tin thực thể `ExamAttempt` tương ứng từ CSDL, đảm bảo lượt thi đã nộp (`IsSubmitted == true`) và thuộc về đúng `userId` đang đăng nhập để tránh rò rỉ dữ liệu chéo.
   - Lấy danh sách toàn bộ câu hỏi của đề thi (`Questions`, `QuestionOptions`, `Passages`).
   - Lấy danh sách các câu trả lời mà học viên đã chọn (`AttemptAnswers`).
2. Cơ sở dữ liệu SQL Server trả về toàn bộ dữ liệu thực thể liên quan.
3. Handler thực hiện logic so khớp:
   - Duyệt qua từng câu hỏi trong đề thi.
   - Lấy ra đáp án đã chọn (`SelectedOption`) của học viên và đáp án chính xác (`CorrectOption`) từ câu hỏi.
   - So sánh hai đáp án này để gán trạng thái `IsCorrect` (Đúng/Sai).
   - Đóng gói cùng nội dung giải thích chi tiết (`Explanation`) của câu hỏi.
4. Đóng gói danh sách này thành đối tượng `ExamReviewDto` và trả về cho PageModel.

### C. Hiển thị giao diện (Rendering Phase)
1. PageModel nhận được DTO kết quả, gán vào thuộc tính hiển thị và chuyển giao diện về trạng thái render trang `Review.cshtml`.
2. Trình duyệt render trang và hiển thị cho học viên:
   - Toàn bộ danh sách câu hỏi trong đề thi.
   - Câu trả lời học viên chọn được tô màu **Đỏ** (nếu sai) hoặc **Xanh** (nếu đúng).
   - Đáp án đúng của hệ thống được đánh dấu làm nổi bật.
   - Khung thông tin lời giải chi tiết (`Explanation`) hiển thị ngay dưới mỗi câu hỏi để học viên ôn tập kiến thức.

---

## 3. Quy tắc nghiệp vụ (Business Rules) áp dụng

- **BR-Review-Submitted-Only:** Chỉ cho phép xem lại chi tiết bài làm khi trạng thái lượt làm bài đã được nộp thành công (`IsSubmitted == true`). Không cho phép xem giải thích khi bài thi đang trong tiến trình làm bài dở dang (`IsSubmitted == false`).
- **BR-Security-Ownership:** Học viên chỉ có quyền xem chi tiết các lượt thi do chính tài khoản của mình thực hiện (`UserId` của Attempt phải khớp với người dùng đang truy cập).

---

## 4. Hướng dẫn import vào Visual Paradigm

1. Tạo một **Communication Diagram** mới trong Visual Paradigm.
2. Sao chép nội dung code trong file [`CD-UC10.1_Review_Test_Results.puml`](./CD-UC10.1_Review_Test_Results.puml).
3. Import sơ đồ thông qua tính năng **Import from PlantUML**.
