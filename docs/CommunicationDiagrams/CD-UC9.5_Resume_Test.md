# Communication Diagram – UC-9.5: Resume Test (Khôi phục bài thi bị gián đoạn)

> **Diagram Source:** [`CD-UC9.5_Resume_Test.puml`](./CD-UC9.5_Resume_Test.puml)  
> **Tool:** PlantUML (tương thích Visual Paradigm, VS Code)  
> **Author:** Senior Developer  
> **Date:** 2026-07-19

---

## 1. Participants (Các thành phần tham gia)

| Participant | Class / Type | Layer | Vai trò |
|---|---|---|---|
| **Student** | Actor | – | Học viên tiếp tục bài thi đang dở dang |
| **:Detail.cshtml** | View | WebUI | Trang chi tiết đề thi hiển thị nút Resume khi phát hiện bài làm dở |
| **:Take.cshtml** | View | WebUI | Giao diện phòng thi khôi phục lại trạng thái |
| **exam-session.js** | Client Script | WebUI (Static Asset) | Script khôi phục bộ đếm thời gian, các câu trả lời và đồng bộ localStorage |
| **:TakeModel** | PageModel | WebUI | Controller Razor Page xử lý OnGet (tải session) và OnPostSaveProgress (lưu tạm thời) |
| **:GetTestSessionQuery** | Query | Application | Truy vấn lấy thông tin phiên làm bài cũ |
| **:GetTestSessionQueryHandler** | Query Handler | Application | Lấy thông tin lượt thi chưa nộp, danh sách đáp án đã tích và thời gian còn lại |
| **:IAppDbContext** | DbContext | Infrastructure | Interface truy vấn và lưu tạm tiến trình học viên |
| **SQL Server** | Database (SQL) | External | Cơ sở dữ liệu lưu trạng thái làm bài tạm thời |

---

## 2. Vòng đời dữ liệu & Luồng giao tiếp

### A. Phát hiện lượt thi dở dang (Detect Phase)
1. Khi học viên truy cập trang chi tiết đề thi (`/Exams/Detail/{id}`), hệ thống sẽ gửi yêu cầu lấy thông tin đề thi.
2. PageModel kiểm tra xem học viên đã đăng nhập chưa và gửi truy vấn tìm kiếm lượt thi chưa hoàn thành (`IsSubmitted == false`) của học viên này đối với đề thi hiện tại.
3. Nếu tồn tại, giao diện render một **Resume Banner** (hoặc hộp thông báo cảnh báo) phía trên cùng với thông tin: *Thời gian còn lại, số câu đã tích* kèm nút **"Resume Test"**.
4. Học viên click vào nút **"Resume Test"**. Hệ thống chuyển hướng sang `/Exams/Take?attemptId=...`.

### B. Tải lại trạng thái cũ (Restore Phase)
1. Giao diện `/Exams/Take` gửi yêu cầu HTTP GET kèm theo `attemptId` lên PageModel.
2. PageModel gửi `GetTestSessionQuery(attemptId, userId)` tới Handler qua MediatR.
3. Handler thực hiện truy vấn cơ sở dữ liệu lấy thông tin `ExamAttempt`, danh sách câu hỏi đề thi và toàn bộ các đáp án đã lưu trước đó trong bảng `AttemptAnswers`.
4. Dữ liệu được đóng gói thành `TestSessionDto` chứa các thuộc tính: `RemainingSeconds` (Thời gian còn lại), `SavedAnswers` (Đáp án đã lưu), và `CurrentQuestionIndex` (Câu hỏi đang làm dở).
5. PageModel nhận kết quả và render trang `Take.cshtml` với biến JSON `window.examSessionConfig` chứa toàn bộ cấu hình phiên.
6. Khi trang tải xong, `exam-session.js` hoạt động:
   - Đọc bản sao lưu dự phòng ở `localStorage` (nếu có và trùng khớp `attemptId`) để đồng bộ trạng thái mới nhất giữa client và server.
   - Khôi phục bộ đếm ngược bằng giá trị `remaining` và kích hoạt lại timer chạy liên tục.
   - Điền sẵn các lựa chọn đáp án đã làm vào giao diện trắc nghiệm (đáp án A, B, C, D được chọn).
   - Di chuyển giao diện đến đúng câu hỏi học viên đang làm dở trước đó.

### C. Thoát và lưu tiến độ tự động (Exit / Save Progress)
1. Khi học viên click **"Exit test"**, modal rời phòng thi hiện ra (`modal-leave`).
2. Học viên chọn **"Save & exit"** (Lưu và thoát).
3. Script gửi request POST đến handler `SaveProgress` chứa `remainingSeconds` hiện tại.
4. PageModel nhận request và lưu trữ thời gian còn lại vào database (`ExamAttempt.RemainingTime`).
5. Sau khi cập nhật thành công, client chuyển hướng học viên quay lại trang chi tiết đề thi.

---

## 3. Quy tắc nghiệp vụ (Business Rules) áp dụng

- **BR-Timer-Freeze:** Khi học viên chủ động chọn thoát và lưu bài, bộ đếm ngược thời gian sẽ tạm dừng (đóng băng) và lưu trữ lại vào CSDL để dùng cho lượt sau.
- **BR-State-Sync:** Ưu tiên đồng bộ trạng thái đáp án từ Server Db, bổ sung tham chiếu từ Local Storage nếu có độ lệch thời gian (giúp bảo toàn đáp án của học viên kể cả khi mất kết nối mạng đột ngột).

---

## 4. Hướng dẫn import vào Visual Paradigm

1. Tạo một **Communication Diagram** mới trong Visual Paradigm.
2. Sao chép code trong file [`CD-UC9.5_Resume_Test.puml`](./CD-UC9.5_Resume_Test.puml).
3. Nhập sơ đồ thông qua chức năng **Import from PlantUML**.
