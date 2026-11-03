# Communication Diagram – UC-14.3: Review Incorrect Answers (Ôn tập câu trả lời sai)

> **Diagram Source:** [`CD-UC14.3_Review_Incorrect_Answers.puml`](./CD-UC14.3_Review_Incorrect_Answers.puml)  
> **Tool:** PlantUML (tương thích Visual Paradigm, VS Code)  
> **Author:** Senior Developer  
> **Date:** 2026-07-19

---

## 1. Participants (Các thành phần tham gia)

| Participant | Class / Type | Layer | Vai trò |
|---|---|---|---|
| **Student** | Actor | – | Học viên muốn xem và ôn tập lại các câu hỏi đã làm sai trong lịch sử |
| **:IncorrectAnswers.cshtml** | View | WebUI | Giao diện hiển thị danh sách các câu hỏi làm sai, phân chia theo nhóm |
| **:IncorrectAnswersModel** | PageModel | WebUI | Entry point server-side nhận yêu cầu HTTP GET truy vấn và lọc dữ liệu |
| **:GetIncorrectAnswersQuery** | Query | Application | Đối tượng truy vấn chứa `UserId` và các bộ lọc tự chọn (Part, Đề thi) |
| **:GetIncorrectAnswersQueryHandler** | Query Handler | Application | Lọc các câu làm sai từ CSDL và gom nhóm theo tiêu chuẩn |
| **:IAppDbContext** | DbContext | Infrastructure | Interface tương tác cơ sở dữ liệu qua EF Core |
| **SQL Server** | Database (SQL) | External | Cơ sở dữ liệu chứa lịch sử làm bài, các câu hỏi và đáp án chính xác |

---

## 2. Vòng đời dữ liệu & Luồng giao tiếp

### A. Yêu cầu danh sách câu sai (Request Phase)
1. Học viên click vào nút **"Review Incorrect Answers"** (Ôn tập câu sai) từ trang tổng quan học tập.
2. Trình duyệt gửi yêu cầu HTTP GET đến `/Exams/IncorrectAnswers` (hoặc `/Account/Profile?tab=incorrect`).
3. PageModel `IncorrectAnswersModel` kiểm tra quyền đăng nhập, xác định `userId` của học viên từ session.
4. Gửi `GetIncorrectAnswersQuery(userId, partFilter, examFilter)` đi qua MediatR.

### B. Truy vấn & Gom nhóm thông tin câu sai (Processing Phase)
1. `GetIncorrectAnswersQueryHandler` nhận yêu cầu truy vấn.
2. Handler thông qua `IAppDbContext` thực hiện kết nối và lọc dữ liệu:
   - Truy vấn bảng `AttemptAnswers` để tìm các câu trả lời của học viên mà giá trị cột `SelectedOption` (đáp án học viên chọn) khác với cột `CorrectOption` (đáp án đúng lưu tại bảng `Questions`).
   - Chỉ quét các câu thuộc về lượt thi đã nộp hoàn chỉnh (`IsSubmitted == true`) của chính học viên đó (`UserId == userId`).
   - Sử dụng các câu lệnh JOIN để lấy đầy đủ nội dung câu hỏi (`QuestionText`), hình ảnh đính kèm (`ImageUrl`), các phương án lựa chọn (`Options`), lời giải thích (`Explanation`), thông tin phần thi (`Part`), và tên đề thi (`ExamTitle`).
3. Cơ sở dữ liệu SQL Server xử lý câu lệnh SELECT phức tạp và trả về các bản ghi câu hỏi làm sai.
4. Handler xử lý nghiệp vụ trên tập dữ liệu trả về:
   - Gom nhóm các câu hỏi làm sai theo các tiêu chí để học viên dễ ôn tập: **Gom nhóm theo Part** (Part 1 đến Part 7) hoặc **Gom nhóm theo Đề thi** (Exam).
   - Đóng gói dữ liệu thành `IncorrectAnswersDto` có cấu trúc phân tầng.
5. Handler gửi DTO kết quả về cho PageModel.

### C. Hiển thị và tương tác ôn tập (Rendering Phase)
1. PageModel nhận DTO, đưa vào Model hiển thị và tiến hành render trang `IncorrectAnswers.cshtml`.
2. Trình duyệt hiển thị giao diện ôn tập dạng **Accordion** (Hộp nội dung co giãn):
   - Học viên nhìn thấy số lượng câu sai phân theo từng Part (Ví dụ: *"Part 5: Incomplete Sentences (12 câu sai)"*, *"Part 1: Photographs (2 câu sai)"*).
   - Khi click vào một nhóm Part, Accordion mở rộng ra hiển thị chi tiết từng câu hỏi:
     - Đề bài và hình ảnh/đoạn văn liên quan.
     - Phương án học viên đã tích (tô đỏ) và phương án đúng của hệ thống (tô xanh).
     - Khung giải thích lý do sai (`Explanation`) để học viên ghi nhớ và rút kinh nghiệm.

---

## 3. Quy tắc nghiệp vụ (Business Rules) áp dụng

- **BR-Wrong-Calculation:** Câu hỏi được xác định là làm sai khi đáp án đã nộp khác với đáp án chính thức (`SelectedOption != CorrectOption`) và không được bỏ trống (`SelectedOption != null`).
- **BR-Group-View:** Câu hỏi làm sai cần được gom nhóm rõ ràng (theo phần thi TOEIC hoặc theo đề thi cụ thể) thay vì hiển thị tràn lan, giúp tăng hiệu quả ôn tập có định hướng.

---

## 4. Hướng dẫn import vào Visual Paradigm

1. Tạo một **Communication Diagram** mới trong Visual Paradigm.
2. Sao chép nội dung code trong file [`CD-UC14.3_Review_Incorrect_Answers.puml`](./CD-UC14.3_Review_Incorrect_Answers.puml).
3. Import sơ đồ thông qua chức năng **Import from PlantUML**.
