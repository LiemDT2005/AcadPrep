# Communication Diagram – UC-14.1: View Study History (Xem lịch sử học tập cá nhân)

> **Diagram Source:** [`CD-UC14.1_View_Study_History.puml`](./CD-UC14.1_View_Study_History.puml)  
> **Tool:** PlantUML (tương thích Visual Paradigm, VS Code)  
> **Author:** Senior Developer  
> **Date:** 2026-07-19

---

## 1. Participants (Các thành phần tham gia)

| Participant | Class / Type | Layer | Vai trò |
|---|---|---|---|
| **Student** | Actor | – | Học viên muốn theo dõi quá trình học tập tổng quát của mình |
| **:Profile.cshtml (History Tab)** | View | WebUI | Giao diện hiển thị dòng thời gian (timeline) lịch sử các hoạt động |
| **:ProfileModel** | PageModel | WebUI | Nhận request HTTP GET `/Account/Profile` với tham số `tab=history` |
| **:GetStudyHistoryQuery** | Query | Application | Truy vấn lấy toàn bộ hoạt động học tập dựa trên `UserId` |
| **:GetStudyHistoryQueryHandler** | Query Handler | Application | Tổng hợp, phân loại, sắp xếp các hoạt động từ nhiều bảng dữ liệu khác nhau |
| **:IAppDbContext** | DbContext | Infrastructure | Interface truy cập cơ sở dữ liệu |
| **SQL Server** | Database (SQL) | External | Cơ sở dữ liệu chứa lịch sử làm bài thi, luyện tập, từ vựng và thành tích |

---

## 2. Vòng đời dữ liệu & Luồng giao tiếp

### A. Yêu cầu xem lịch sử học tập (Request Phase)
1. Học viên truy cập trang cá nhân và click chọn tab **"Lịch sử hoạt động"** (Study History).
2. Trình duyệt gửi yêu cầu HTTP GET đến `/Account/Profile?tab=history`.
3. PageModel `ProfileModel` nhận yêu cầu, kiểm tra thông tin đăng nhập và lấy `userId` của học viên.
4. PageModel gửi `GetStudyHistoryQuery(userId)` đi qua MediatR.

### B. Tổng hợp lịch sử từ nhiều nguồn (Processing Phase)
1. `GetStudyHistoryQueryHandler` nhận yêu cầu truy vấn.
2. Để vẽ nên một dòng thời gian hoạt động học tập tổng quát, Handler cần truy cập thông tin từ nhiều nguồn dữ liệu khác nhau trong CSDL một cách song song:
   - **ExamAttempts:** Lấy các lượt làm đề thi thử đã nộp (`IsSubmitted == true`) kèm theo điểm số đạt được.
   - **PracticeSessions:** Lấy các phiên luyện tập theo Part đã nộp kèm số câu đúng/tổng số câu.
   - **SavedVocabularies:** Lấy danh sách các từ vựng mới được thêm vào sổ tay cá nhân (`Notebook`).
   - **UserAchievements:** Lấy thông tin các huy hiệu thành tích mà học viên đã mở khóa trên hệ thống.
3. Cơ sở dữ liệu SQL Server thực thi các câu lệnh truy vấn và trả về danh sách các thực thể thô.
4. Handler thực hiện logic gom nhóm và chuẩn hóa:
   - Ánh xạ từng bản ghi thô về một kiểu DTO chung là `StudyActivityDto` (chứa các trường: `ActivityType` [Exam, Practice, Vocabulary, Achievement], `Title`, `Description`, `Timestamp`, `ScoreInfo`, `LinkUrl`).
   - Gộp tất cả danh sách hoạt động lại và sắp xếp theo trình tự thời gian đảo ngược (`Timestamp` giảm dần) để đưa hoạt động mới nhất lên đầu.
5. Trả cấu trúc dữ liệu dòng thời gian `StudyHistoryDto` về cho PageModel.

### C. Hiển thị dòng thời gian (Rendering Phase)
1. PageModel nhận dữ liệu lịch sử, chuẩn bị cấu hình giao diện và chuyển sang render mã HTML cho tab lịch sử.
2. Giao diện hiển thị dưới dạng một dòng thời gian trực quan (Vertical Timeline):
   - Mỗi mốc thời gian có một biểu tượng (Icon) đặc trưng dựa trên loại hoạt động (Ví dụ: đề thi hiển thị icon đề thi, từ vựng hiển thị hình quyển sách).
   - Hiển thị tóm tắt điểm số đạt được (Ví dụ: *"Đạt 720/990 điểm đề thi thử TOEIC 2024"*, *"Đã lưu từ 'ubiquitous' vào sổ tay"*).
   - Cung cấp các nút liên kết nhanh (Ví dụ: click vào lượt thi thử để đi đến trang xem lại câu trả lời chi tiết).

---

## 3. Quy tắc nghiệp vụ (Business Rules) áp dụng

- **BR-Chronological-Order:** Dòng thời gian phải được sắp xếp theo đúng trình tự thời gian giảm dần (hoạt động mới nhất luôn nằm trên cùng).
- **BR-Data-Privacy:** Học viên chỉ được xem dòng thời gian hoạt động của chính bản thân mình. Cơ chế phân quyền ngăn chặn tài khoản học viên khác đọc trộm timeline.

---

## 4. Hướng dẫn import vào Visual Paradigm

1. Tạo một **Communication Diagram** mới trong Visual Paradigm.
2. Sao chép nội dung code trong file [`CD-UC14.1_View_Study_History.puml`](./CD-UC14.1_View_Study_History.puml).
3. Import sơ đồ bằng chức năng **Import from PlantUML**.
