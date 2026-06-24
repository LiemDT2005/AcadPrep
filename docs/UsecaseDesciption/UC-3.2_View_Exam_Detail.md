# Software Requirement Specification (SRS) - Use Case Detail

## 1. Use Case: UC-3.2_View Exam Detail

### a. Functional Description

| Thuộc tính (Attribute) | Chi tiết (Details) |
| :--- | :--- |
| **UC ID and Name** | UC-3.2 – View Exam Detail |
| **Created By** | LiemDT |
| **Date Created** | 29/05/2026 |
| **Primary Actor** | Guest (Khách) |
| **Secondary Actors** | System (Hệ thống) |
| **Trigger** | Guest nhấp chọn vào một thẻ đề thi (exam card) từ danh sách đề thi (UC-2.1). |
| **Description** | Guest xem thông tin chi tiết về một đề thi cụ thể, bao gồm cấu trúc đề thi, số liệu thống kê cộng đồng và các nút hành động tự động thay đổi dựa trên trạng thái đăng nhập của họ. |
| **Preconditions** | Đề thi phải tồn tại trong hệ thống và có trạng thái là 'published' (đã xuất bản).<br>Guest đang ở trang danh sách đề thi (UC-2.1). |
| **Postconditions** | **Thành công:** Trang chi tiết đề thi được hiển thị đầy đủ.<br>**Thất bại:** Guest bị điều hướng quay trở lại danh sách đề thi kèm theo thông báo lỗi. |
| **Priority** | High – Must Have (Bắt buộc phải có) |
| **Frequency of Use** | Frequent (Thường xuyên) — Guest thường xem trước chi tiết đề thi trước khi quyết định làm bài. |
| **Assumptions** | Guest không cần đăng nhập vẫn xem được chi tiết đề thi. Việc đăng nhập chỉ bắt buộc khi thực hiện làm bài thi. |

#### Luồng xử lý chuẩn (Normal Flow)
1. Guest nhấp chọn vào một thẻ đề thi từ danh sách đề thi.
2. System xác thực đề thi đó có tồn tại và đã được xuất bản hay không (**BR-15**: các đề thi đã bị xóa mềm/soft-deleted sẽ không thể truy cập).
3. System tải thông tin chi tiết của đề thi: tên, mô tả, năm, cấu trúc phần thi (**BR-16**: các câu hỏi phải được phân bổ chính xác vào Part 1–7), tổng thời gian, số lượt làm bài, và điểm số trung bình (**BR-13**: các số liệu thống kê được làm mới mỗi 30 phút).
4. System hiển thị trang chi tiết kèm theo nút "Log in to start" (Đăng nhập để bắt đầu) dành cho đối tượng Guest.
5. Guest xem và kiểm tra thông tin đề thi.

#### Luồng xử lý thay thế (Alternative Flows)
* **A1 – Guest đã đăng nhập (User):** Nút hành động sẽ tự động thay đổi dựa trên lịch sử làm bài thi của người dùng:
  * Trạng thái *Chưa từng làm bài*: Hiển thị nút **'Start'** $\rightarrow$ Chuyển hướng đến UC-8.1.
  * Trạng thái *Đang làm dở dang*: Hiển thị nút **'Resume'** (**BR-04**: bộ đếm thời gian và các câu trả lời sẽ được đóng băng khi bị gián đoạn) $\rightarrow$ Chuyển hướng đến UC-8.5.
  * Trạng thái *Đã hoàn thành*: Hiển thị nút **'Retake'** (Làm lại) hoặc **'View Result'** (Xem kết quả) (**BR-05**: điểm số chỉ được ghi nhận khi bấm Submit hoặc hết giờ) $\rightarrow$ Chuyển hướng đến UC-9.1.

#### Ngoại lệ (Exceptions)
* **E1: Đề thi không tồn tại hoặc đã bị xóa mềm (BR-15)**
  * System trả về lỗi 404, hiển thị thông báo lỗi và điều hướng người dùng quay lại UC-2.1.
* **E2: Lỗi kết nối máy chủ (Server connection error)**
  * System hiển thị một thông báo lỗi và yêu cầu người dùng thử lại.

#### Thông tin khác (Other Information)
* Trang này đóng vai trò là cầu nối giữa việc khám phá đề thi và thực thi bài thi. Có liên kết dẫn tới: UC-8.1 (Start Full Test), UC-8.5 (Resume Test), và UC-9.1 (Review Results).
* **Business Rules liên quan:** BR-15, BR-04, BR-05, BR-06, BR-16

---

### b. Business Rules (Quy tắc kinh doanh)

| ID | Tên Quy Tắc (Business Rule) | Mô Tả Quy Tắc Kinh Doanh (Business Rule Description) |
| :--- | :--- | :--- |
| **BR-15** | Referential Integrity on Exam Deletion | Một Moderator (Người kiểm duyệt) hoặc Admin không thể xóa cứng (hard-delete) một Đề thi nếu nó đã được liên kết với nhật ký học tập hiện có của người dùng (Các lượt làm bài - Exam Attempts). Trong trường hợp này, hệ thống phải áp dụng cơ chế "Xóa mềm" (Soft Delete - ẩn đề thi đi) để thay thế. |
| **BR-04** | Full Test Interruption Control | Khi một phiên làm bài thi toàn phần (Full Test) bị gián đoạn (do mất kết nối mạng hoặc chủ động thoát ra), hệ thống phải tự động đóng băng bộ đếm ngược thời gian và lưu lại tất cả các câu trả lời đã chọn tính đến thời điểm đó để hỗ trợ tính năng "Làm tiếp bài thi" (Resume Test). |
| **BR-05** | Full Test Scoring Trigger Constraints | Điểm số, biểu đồ phân tích hiệu suất và lộ trình theo dõi tiến độ điểm số chỉ được tính toán và ghi vào cơ sở dữ liệu khi người dùng kích hoạt hành động "Nộp bài" (Submit Test) hoặc khi đồng hồ đếm ngược chính thức về bằng không (0). |
| **BR-06** | Standardized Score Conversion Matrix | Điểm của bài thi toàn phần (Full Test) không được tính theo tỷ lệ phần trăm câu đúng đơn thuần; thay vào đó, số lượng câu trả lời đúng phải được quy đổi sang thang điểm TOEIC chuẩn (0 - 990) bằng cách sử dụng Bảng Quy Đổi Điểm (Score Conversion Table) được cấu hình sẵn trong hệ thống. |
| **BR-16** | Exam and Question Structural Validation | Khi tạo hoặc cập nhật một câu hỏi TOEIC, câu hỏi đó bắt buộc phải được gán chính xác vào một Đề thi cụ thể, thuộc một Phần rõ ràng (từ Part 1 đến Part 7), đồng thời phải cấu hình đầy đủ đáp án đúng cùng tất cả các phương án gây nhiễu (distractor choices). |