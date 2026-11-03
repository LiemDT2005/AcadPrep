# CỘNG HÒA XÃ HỘI CHỦ NGHĨA VIỆT NAM
**Độc lập - Tự do - Hạnh phúc**

---

## TÀI LIỆU ĐẶC TẢ PHÂN CHIA KHỐI LƯỢNG CÔNG VIỆC DỰ ÁN ACADPREP
**Hệ thống Nền tảng Hỗ trợ Ôn luyện Thi TOEIC Trực tuyến**

*   **Mục đích:** Phân rã danh sách 60 Use Cases thành các nhóm phân hệ chức năng độc lập, cân bằng độ phức tạp kỹ thuật và tối ưu hóa năng suất làm việc cho nhóm 04 thành viên.
*   **Nguyên tắc phân chia:** Đảm bảo tính trọn vẹn của từng phân hệ (Module-based) để giảm thiểu tối đa sự chồng chéo khi thiết kế cơ sở dữ liệu và triển khai mã nguồn Backend (N-tier layers).

---

### I. BẢNG TỔNG HỢP PHÂN BỔ KHỐI LƯỢNG WORKLOAD

| STT | Thành viên phụ trách | Phân hệ chuyên trách (Modules) | Số lượng UC | Mức độ phức tạp |
| :---: | :--- | :--- | :---: | :--- |
| 1 | **Thành viên 01** | Authentication, Profile, Security & Account Management | 15 UCs | ⭐⭐⭐⭐☆ (Bảo mật & Phân quyền) |
| 2 | **Thành viên 02** | Testing Engine & Exam Simulation (Bộ lõi thi và luyện tập) | 14 UCs | ⭐⭐⭐⭐⭐ (Real-time & State xử lý) |
| 3 | **Thành viên 03** | Vocabulary, Analytics, Gamification & Reporting System | 16 UCs | ⭐⭐⭐⭐☆ (Thuật toán & Thống kê) |
| 4 | **Thành viên 04** | Content Management System (CMS - Quản trị Back-office) | 15 UCs | ⭐⭐⭐☆☆ (CRUD & Ràng buộc SQL) |

---

### II. BẢNG ĐẶC TẢ CHI TIẾT NHIỆM VỤ CỦA TỪNG THÀNH VIÊN

#### 1. Thành viên 01: Chuyên gia Bảo mật & Quản trị Tài khoản (Authentication & Security Specialist)
*   **Phạm vi công việc:** Thiết lập và chịu trách nhiệm toàn bộ các vấn đề liên quan đến xác thực người dùng, mã hóa dữ liệu, quản lý token/phiên làm việc (Session/JWT), cấu hình phân quyền hệ thống và phân hệ thông báo.
*   **Danh sách 15 Use Cases chi tiết:**
    *   **UC-1 (Register with Email):** Đăng ký tài khoản hệ thống thông qua Email truyền thống.
    *   **UC-2 (Verify OTP):** Xác thực mã OTP 6 chữ số gửi qua email để kích hoạt tài khoản.
    *   **UC-5.1 (Login with Email):** Đăng nhập vào hệ thống bằng Email và Mật khẩu.
    *   **UC-5.2 (Login with Google):** Đăng nhập qua bên thứ ba (OAuth2), tự động liên kết tài khoản trùng Email về một User ID duy nhất.
    *   **UC-6 (Logout):** Đăng xuất, hủy bỏ phiên làm việc và xóa token bảo mật.
    *   **UC-7.1 (View Profile):** Xem thông tin hồ sơ cá nhân và các thông số tổng quan.
    *   **UC-7.2 (Edit Profile):** Chỉnh sửa các thông tin cơ bản gồm tên hiển thị và ảnh đại diện.
    *   **UC-7.3 (Change Password):** Thay đổi mật khẩu đăng nhập hiện tại của tài khoản.
    *   **UC-8 (Forgot Password):** Khôi phục lại quyền truy cập tài khoản khi quên mật khẩu qua OTP.
    *   **UC-15 (View Notifications):** Xem danh sách thông báo hệ thống (nhắc học, kết quả thi...).
    *   **UC-15.1 (Mark Notification as Read):** Chuyển trạng thái thông báo để tối ưu hộp thư.
    *   **UC-19.1 (View Accounts List):** Màn hình Admin hiển thị danh sách toàn bộ tài khoản trong hệ thống.
    *   **UC-19.2 (View Account Detail):** Admin xem chi tiết thông tin và lịch sử hoạt động của một tài khoản.
    *   **UC-19.3 (Update Account Status):** Admin thực hiện khóa hoặc mở khóa tài khoản người dùng.
    *   **UC-19.4 (Assign Roles):** Admin quản lý phân quyền (User/Moderator/Admin) - Ràng buộc không cho phép hạ bệ tài khoản Master Admin gốc.

---

#### 2. Thành viên 02: Kỹ sư Phát triển Công cụ Thi (Testing Engine Simulation Engineer)
*   **Phạm vi công việc:** Triển khai bộ lõi cốt lõi và phức tạp nhất của hệ thống học tập bao gồm: Quản lý thời gian đếm ngược thời gian thực, cơ chế đóng băng trạng thái khi mất kết nối, kiểm soát tệp tin âm thanh và ma trận quy đổi điểm thi chuẩn TOEIC.
*   **Danh sách 14 Use Cases chi tiết:**
    *   **UC-3.1 (View Exams - Learner):** Hiển thị danh sách đề thi công khai kèm năm sản xuất và độ khó cho người học.
    *   **UC-3.2 (View Exam Detail - Learner):** Xem cấu trúc chi tiết, thời gian và số lượng câu hỏi của một đề thi cụ thể.
    *   **UC-3.3 (Search Exams - Learner):** Tìm kiếm đề thi nhanh bằng từ khóa.
    *   **UC-3.4 (Filter Exams - Learner):** Lọc danh sách đề thi theo các tiêu chí (năm, số lượt thi, độ khó).
    *   **UC-4 (View Leaderboard):** Hiển thị bảng xếp hạng tổng dựa trên điểm tích lũy hoặc chuỗi ngày học.
    *   **UC-9.1 (Start Full Test):** Khởi tạo một phiên thi Full Test đầy đủ 7 Parts với thời gian đếm ngược chuẩn.
    *   **UC-9.2 (Practice by Part):** Kích hoạt chế độ luyện tập riêng lẻ theo từng Part lựa chọn từ Part 1 đến Part 7.
    *   **UC-9.3 (Play Audio):** Điều khiển trình phát audio phần nghe - Áp dụng quy tắc nghiêm ngặt chống tua/tạm dừng ở chế độ Full Test.
    *   **UC-9.4 (Submit Test):** Xử lý nộp bài thi (chủ động hoặc khi hết giờ), tự động chấm điểm và lưu dữ liệu.
    *   **UC-9.5 (Resume Test):** Khôi phục và tiếp tục bài thi bị gián đoạn, tải lại toàn bộ đáp án đã tích và thời gian còn lại từ cơ sở dữ liệu.
    *   **UC-10.1 (Review Test Results):** Xem lại chi tiết từng câu đúng/sai, đáp án hệ thống và lời giải thích sau khi nộp bài.
    *   **UC-14.1 (View Study History):** Hiển thị dòng thời gian lịch sử hoạt động học tập tổng quát của học viên.
    *   **UC-14.2 (View Exam Attempts):** Liệt kê danh sách các lượt thi thử đề thi kèm theo điểm số thu được.
    *   **UC-14.3 (Review Incorrect Answers):** Gom nhóm và hiển thị toàn bộ những câu hỏi đã làm sai trong lịch sử để học viên ôn tập lại.

---

#### 3. Thành viên 03: Chuyên gia Phân tích Dữ liệu & Thuật toán Học thuật (Data Analytics & Smart Learning Algorithm Developer)
*   **Phạm vi công việc:** Chịu trách nhiệm thiết lập thuật toán học thông minh (Spaced Repetition) cho Flashcard, hệ thống Gamification (Tính chuỗi ngày học Study Streak, mở khóa danh hiệu) và kết xuất biểu đồ báo cáo thống kê hiệu suất học tập.
*   **Danh sách 16 Use Cases chi tiết:**
    *   **UC-10.2 (View Performance Analysis):** Kết xuất dữ liệu biểu đồ phân tích điểm mạnh, điểm yếu theo từng kỹ năng Nghe/Đọc của học viên.
    *   **UC-10.3 (View Score Progress):** Vẽ biểu đồ đường theo dõi tiến độ biến động điểm số qua các lượt thi.
    *   **UC-11.1 (Review Flashcards):** Hiển thị thẻ ghi nhớ từ vựng phục vụ việc ôn luyện.
    *   **UC-11.2 (Rate Memorization):** Tiếp nhận điểm tự đánh giá của User để thay đổi tham số $Interval$ (Khoảng cách thời gian lặp lại từ vựng).
    *   **UC-11.3 (Save Vocabulary):** Lưu từ vựng từ đề thi vào sổ tay cá nhân kèm ràng buộc chống trùng lặp bản ghi.
    *   **UC-11.4 (View Vocabulary List):** Hiển thị danh sách từ vựng đã lưu, hỗ trợ sắp xếp mặc định theo ngày lưu giảm dần.
    *   **UC-11.5 (Remove Saved Vocabulary):** Xóa từ vựng khỏi danh sách sổ tay cá nhân.
    *   **UC-12.1 (View Vocabulary Passage):** Xem đoạn văn ngữ cảnh đi kèm của từ vựng để hiểu cách dùng thực tế.
    *   **UC-12.2 (Look Up Vocabulary):** Tính năng tra cứu chi tiết nghĩa, từ loại, phát âm và ví dụ của từ.
    *   **UC-13.1 (View Achievements):** Xem danh sách các huy hiệu/danh hiệu đã đạt được hoặc điều kiện để mở khóa.
    *   **UC-13.2 (View Study Streak):** Kiểm tra và hiển thị chuỗi ngày học liên tục (Áp dụng bộ quét tự động reset chuỗi về 0 sau 23:59:59 theo múi giờ UTC+7 nếu không có hoạt động).
    *   **UC-20.1 (View Learning Progress Reports):** Màn hình Admin hiển thị báo cáo tổng hợp tiến độ học tập của toàn bộ hệ thống.
    *   **UC-20.2 (View User Statistics):** Thống kê lượng đăng ký mới, lượng User kích hoạt và tỷ lệ giữ chân (Retention rate).
    *   **UC-20.3 (View Exam Statistics):** Thống kê số lượt làm bài, điểm số trung bình và tỷ lệ hoàn thành của từng đề thi.
    *   *Đặc tả Logic bổ sung 01:* Áp dụng cơ chế lưu bộ nhớ đệm (Caching) tối thiểu 1 tiếng cho các truy vấn thống kê của Admin nhằm bảo vệ tài nguyên hệ thống.
    *   *Đặc tả Logic bổ sung 02:* Thiết lập ma trận ánh xạ quy đổi điểm số thô sang thang điểm TOEIC chuẩn (0 - 990).

---

#### 4. Thành viên 04: Kiến trúc sư Quản trị Nội dung & Toàn vẹn Dữ liệu (CMS & Data Integrity Architect)
*   **Phạm vi công việc:** Xây dựng toàn bộ phân hệ quản lý nội dung Back-office dành cho Moderator và Admin để cập nhật học liệu. Đảm bảo các quy tắc ràng buộc toàn vẹn dữ liệu nghiêm ngặt giữa Đề thi, Câu hỏi và Đoạn văn đọc hiểu.
*   **Danh sách 15 Use Cases chi tiết:**
    *   **UC-16.1 (View Exams - CMS):** Quản trị viên xem danh sách toàn bộ đề thi kèm trạng thái ẩn/hiện.
    *   **UC-16.2 (Create Exam):** Khởi tạo một đề thi mới vào hệ thống (gồm tên, cấu hình part, thời gian).
    *   **UC-16.3 (View Exam Detail - CMS):** Xem thông tin chi tiết cấu trúc câu hỏi và số liệu thống kê của đề thi.
    *   **UC-16.4 (Update Exam):** Cập nhật thông tin đề thi, chỉnh sửa danh sách câu hỏi hoặc chuyển trạng thái xuất bản.
    *   **UC-16.5 (Delete Exam):** Áp dụng cơ chế "Xóa mềm" (Soft Delete/Ẩn đề thi) nếu đề thi đã có lịch sử làm bài nhằm bảo vệ toàn vẹn dữ liệu.
    *   **UC-16.6 (Search Exams - CMS):** Tìm kiếm đề thi bằng từ khóa trong trang quản trị.
    *   **UC-16.7 (Filter Exams - CMS):** Bộ lọc danh mục đề thi theo trạng thái quản trị.
    *   **UC-17.1 (View Questions):** Xem danh sách toàn bộ câu hỏi hiện có trong ngân hàng câu hỏi.
    *   **UC-17.2 (Create Question):** Tạo câu hỏi mới (Bắt buộc cấu hình đầy đủ đáp án đúng, các lựa chọn gây nhiễu và gán cố định vào một Part từ 1 đến 7).
    *   **UC-17.3 (View Question Detail):** Xem chi tiết nội dung, đáp án, giải thích câu hỏi và danh sách các đề đang nhúng câu hỏi đó.
    *   **UC-17.4 (Update Question):** Chỉnh sửa nội dung văn bản, file âm thanh hoặc đáp án đúng của câu hỏi.
    *   **UC-17.5 (Delete Question):** Xóa câu hỏi khỏi hệ thống và đưa ra cảnh báo ngăn chặn nếu câu hỏi đó đang thuộc về một đề thi đang hoạt động.
    *   **UC-17.6 (Search Questions):** Tìm kiếm câu hỏi dựa trên nội dung văn bản text.
    *   **UC-17.7 (Filter Questions):** Lọc ngân hàng câu hỏi theo Part hoặc độ khó.
    *   **UC-18.1 đến UC-18.5 (Reading Passage Management Module):** Trọn bộ cụm tính năng CRUD đoạn văn (Xem danh sách, Tạo mới, Xem chi tiết, Cập nhật, Xóa đoạn văn đọc hiểu) - Áp dụng quy tắc bắt buộc liên kết câu hỏi Part 6 và Part 7 vào ID đoạn văn để loại bỏ hoàn toàn các thực thể mồ côi.

---

### III. QUY TRÌNH PHỐI HỢP VÀ GIAO TIẾP GIAO DIỆN (INTER-MODULE PIPELINE)

Để đảm bảo quá trình tích hợp mã nguồn (Integration) diễn ra thuận lợi, 4 thành viên thống nhất tuân thủ các giao thức kết nối sau:

1.  **Giao tiếp Security (Thành viên 01 🤝 Thành viên 02 & 03):** Thành viên 01 cung cấp cấu trúc đối tượng người dùng đăng nhập (User Session / Token). Từ đó, Thành viên 02 và 03 lấy ra chính xác `UserId` đang thao tác để ghi nhận lịch sử thi hoặc lưu từ vựng vào sổ tay cá nhân.
2.  **Đồng bộ Thiết kế Dữ liệu (Thành viên 02 🤝 Thành viên 04):** Giao diện làm bài thi của Thành viên 02 phụ thuộc hoàn toàn vào cấu trúc cây dữ liệu `EXAMS` -> `PASSAGES` -> `QUESTIONS` do Thành viên 04 thiết kế và quản lý. Hai thành viên cần chốt cứng kiểu dữ liệu (Data Type) ngay từ giai đoạn đầu tiên.
3.  **Mô hình Kích hoạt Sự kiện - Event Trigger (Thành viên 02 🤝 Thành viên 03):** Ngay khi Thành viên 02 xử lý hoàn tất hành động `Submit Test`, hệ thống sẽ kích hoạt một sự kiện chạy ngầm. Thành viên 03 sẽ đón nhận sự kiện này để tự động tính toán cập nhật lại dữ liệu chuỗi ngày học `Study Streak` và cập nhật biểu đồ phân tích `Performance Analytics` cho học viên.