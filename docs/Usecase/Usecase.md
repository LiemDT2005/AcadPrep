# TÀI LIỆU KHẢO SÁT CHỨC NĂNG THỰC TẾ & BẢN ĐỒ PHÂN QUYỀN (GAP ANALYSIS & AUTHORIZATION SPECIFICATION)

Tài liệu này ghi nhận kết quả rà soát thực tế mã nguồn dự án **AcadPrep** (gồm Application layer, API Controllers, và Razor Pages) để đối chiếu xem:
1. Các chức năng nào **đã có code thực tế** (Implemented) và chức năng nào **chưa có code** (Missing).
2. Tình trạng phân quyền hiện tại trong code.
3. Kế hoạch áp dụng phân quyền cụ thể cho từng chức năng đã được code dựa trên các Use Cases.

---

## I. TỔNG QUAN TÌNH TRẠNG PHÂN QUYỀN HIỆN TẠI (SECURITY AUDIT)

*   **Chưa có cơ chế thực thi phân quyền (No Enforcement):**
    *   Hệ thống **hoàn toàn chưa sử dụng** bất kỳ thuộc tính `[Authorize]` hay `[Authorize(Roles = "...")]` nào trong các API Controller (`ExamsController.cs`, `AchievementsController.cs`) cũng như các Razor Pages trong thư mục `Pages/`.
    *   Tất cả các trang quản trị nguy hiểm như `/Admin/Dashboard`, `/Admin/Exams/Index`, `/Admin/Exams/Edit` và các API tạo/xóa đề thi đều đang ở trạng thái **Public** (cho phép Guest truy cập tự do).
*   **Cơ chế Nhận diện Người dùng (User Identity):**
    *   Khi đăng nhập thành công (`Login.cshtml.cs` và `GoogleCallback.cshtml.cs`), hệ thống sẽ phát một Cookie chứa các Claims: `NameIdentifier` (UserId), `Email`, `Name`, và `Role` (nhận giá trị `"Admin"` hoặc `"User"`).
    *   Hệ thống có dịch vụ `ICurrentUserService` để lấy ra `UserId` của phiên đăng nhập hiện tại.
    *   *Lỗ hổng tạm thời:* Tại một số API như `AchievementsController.cs`, nếu không tìm thấy `UserId` (do Guest truy cập), code đang tự động fallback về `UserId = 2` (tài khoản Test User) để phục vụ demo.

---

## II. ĐỐI CHIẾU CHỨC NĂNG: GIỮA ĐẶC TẢ (UC) & MÃ NGUỒN THỰC TẾ (CODE)

Ký hiệu trạng thái:
*   `🟢 Đã code (Implemented)`: Đã có giao diện Razor Page (`.cshtml`) + Endpoint API / Application Handler (`Queries`/`Commands`).
*   `🔴 Chưa code (Missing)`: Chưa tìm thấy triển khai trong dự án.

### 1. Phân hệ 1: Xác thực, Tài khoản & Thông báo (Authentication & Accounts)

| UC ID | Tên chức năng | Trạng thái Code | File mã nguồn liên quan | Quyền truy cập đề xuất |
| :---: | :--- | :---: | :--- | :---: |
| **UC-1** | Đăng ký tài khoản | 🟢 Đã code | `Pages/Account/Register.cshtml`<br>`RegisterCommand.cs` | **Guest** (Public) |
| **UC-2** | Xác thực OTP kích hoạt | 🟢 Đã code | `Pages/Account/VerifyOtp.cshtml`<br>`VerifyOtpCommand.cs` | **Guest** (Public) |
| **UC-5.1**| Đăng nhập bằng Email | 🟢 Đã code | `Pages/Account/Login.cshtml`<br>`LoginCommand.cs` | **Guest** (Public) |
| **UC-5.2**| Đăng nhập bằng Google | 🟢 Đã code | `Pages/Account/GoogleCallback.cshtml`<br>`GoogleCallbackCommand.cs` | **Guest** (Public) |
| **UC-6** | Đăng xuất | 🟢 Đã code | `Pages/Account/Logout.cshtml` | **User, Admin** |
| **UC-7.1**| Xem hồ sơ cá nhân | 🔴 Chưa code | Chưa có trang/API xem Profile riêng biệt | **User, Admin** |
| **UC-7.2**| Chỉnh sửa hồ sơ | 🔴 Chưa code | Chưa có API chỉnh sửa hồ sơ | **User, Admin** |
| **UC-7.3**| Thay đổi mật khẩu | 🔴 Chưa code | Chưa có API thay đổi mật khẩu | **User, Admin** |
| **UC-8** | Khôi phục mật khẩu | 🔴 Chưa code | Chưa triển khai quy trình Forgot Password | **Guest** (Public) |
| **UC-15** | Xem thông báo hệ thống | 🔴 Chưa code | Chưa có API danh sách thông báo | **User, Admin** |
| **UC-15.1**| Đánh dấu thông báo đã đọc | 🔴 Chưa code | Chưa có API tương ứng | **User, Admin** |
| **UC-19.1**| Xem danh sách tài khoản | 🔴 Chưa code | Chưa có UI/API quản trị danh sách User | **Admin** |
| **UC-19.2**| Xem chi tiết tài khoản | 🔴 Chưa code | Chưa có UI/API xem chi tiết User | **Admin** |
| **UC-19.3**| Cập nhật trạng thái tài khoản| 🔴 Chưa code | Chưa có API Khóa/Mở khóa User | **Admin** |
| **UC-19.4**| Phân quyền tài khoản | 🔴 Chưa code | Chưa có API Assign Roles | **Admin** |

---

### 2. Phân hệ 2: Trình thi thử & Luyện tập (Testing Engine)

| UC ID | Tên chức năng | Trạng thái Code | File mã nguồn liên quan | Quyền truy cập đề xuất |
| :---: | :--- | :---: | :--- | :---: |
| **UC-3.1**| Xem danh sách đề thi | 🟢 Đã code | `Pages/Exams/Index.cshtml`<br>`GetExamListQuery.cs`<br>`ExamsController.GetList()` | **Guest, User, Admin** (Public) |
| **UC-3.2**| Xem chi tiết cấu trúc đề | 🟢 Đã code | `Pages/Exams/Detail.cshtml`<br>`GetExamDetailQuery.cs`<br>`ExamsController.GetDetail()` | **Guest, User, Admin** (Public) |
| **UC-3.3**| Tìm kiếm đề thi | 🟢 Đã code | Tích hợp trong `GetExamListQuery.cs` | **Guest, User, Admin** (Public) |
| **UC-3.4**| Lọc danh sách đề thi | 🟢 Đã code | Tích hợp trong `GetExamListQuery.cs` | **Guest, User, Admin** (Public) |
| **UC-4** | Xem bảng xếp hạng | 🟢 Đã code | `Pages/Performance/Leaderboard.cshtml`<br>`GetLeaderboardQuery.cs` | **Guest, User, Admin** (Public) |
| **UC-9.1**| Bắt đầu thi Full Test | 🟢 Đã code | `Pages/Exams/Take.cshtml`<br>`StartFullTestCommand.cs` | **User, Admin** |
| **UC-9.2**| Luyện tập theo từng Part | 🟢 Đã code | `Pages/Exams/Practice.cshtml`<br>`StartPracticeCommand.cs` | **User, Admin** |
| **UC-9.3**| Nghe Audio bài thi | 🟢 Đã code | Tích hợp trong giao diện thi (Take/Practice) | **User, Admin** |
| **UC-9.4**| Nộp bài thi | 🟢 Đã code | `Pages/Exams/Results.cshtml`<br>`SubmitTestCommand.cs` | **User, Admin** |
| **UC-9.5**| Làm tiếp bài thi (Resume) | 🟢 Đã code | `GetTestSessionQuery.cs` (gọi khi vào lại trang Take) | **User, Admin** |
| **UC-10.1**| Xem giải thích kết quả chi tiết| 🟢 Đã code | `Pages/Exams/Results.cshtml` | **User, Admin** |
| **UC-14.1**| Xem lịch sử học tập cá nhân| 🔴 Chưa code | Chưa có UI/API dòng thời gian lịch sử tổng | **User, Admin** |
| **UC-14.2**| Xem danh sách lượt thi thử | 🟢 Đã code | Hiện đang tích hợp hiển thị ngay trong chi tiết đề thi (`GetExamDetailQuery.cs` trả về danh sách attempts của user trên đề đó). Chưa có trang danh sách attempts độc lập. | **User, Admin** |
| **UC-14.3**| Ôn tập các câu trả lời sai | 🔴 Chưa code | Chưa có UI/API lọc danh sách câu sai để ôn lại | **User, Admin** |

---

### 3. Phân hệ 3: Từ vựng & Phân tích cá nhân (Vocabulary & Personal Analytics)

| UC ID | Tên chức năng | Trạng thái Code | File mã nguồn liên quan | Quyền truy cập đề xuất |
| :---: | :--- | :---: | :--- | :---: |
| **UC-10.2**| Xem biểu đồ điểm mạnh/yếu | 🟢 Đã code | `Pages/Performance/PerformanceAnalysis.cshtml`<br>`GetPerformanceAnalysisQuery.cs` | **User, Admin** |
| **UC-10.3**| Xem biểu đồ tiến độ điểm số | 🟢 Đã code | `Pages/Performance/ScoreProgress.cshtml`<br>`GetScoreProgressQuery.cs` | **User, Admin** |
| **UC-11.1**| Học từ vựng qua Flashcards | 🟢 Đã code | `Pages/Vocabulary/Review.cshtml`<br>`GetReviewFlashcardsQuery.cs` | **User, Admin** |
| **UC-11.2**| Đánh giá mức độ ghi nhớ | 🟢 Đã code | `RateVocabularyCommand.cs` | **User, Admin** |
| **UC-11.3**| Lưu từ vựng vào sổ tay | 🟢 Đã code | `SaveVocabularyCommand.cs` | **User, Admin** |
| **UC-11.4**| Xem danh sách từ vựng cá nhân | 🟢 Đã code | `Pages/Vocabulary/Notebook.cshtml`<br>`GetSavedVocabulariesQuery.cs` | **User, Admin** |
| **UC-11.5**| Xóa từ vựng khỏi sổ tay | 🟢 Đã code | `RemoveVocabularyCommand.cs` | **User, Admin** |
| **UC-12.1**| Xem ngữ cảnh của từ | 🟢 Đã code | `GetVocabPassageQuery.cs` | **User, Admin** |
| **UC-12.2**| Tra cứu từ điển tích hợp | 🟢 Đã code | `Pages/Vocabulary/DictionarySearch.cshtml`<br>`LookupVocabularyQuery.cs` | **User, Admin** |
| **UC-13.1**| Xem danh sách thành tích | 🟢 Đã code | `Pages/Performance/Achievements.cshtml`<br>`GetAchievementsQuery.cs`<br>`AchievementsController.cs` | **User, Admin** |
| **UC-13.2**| Xem chuỗi ngày học (Streak) | 🟢 Đã code | `Pages/Performance/Dashboard.cshtml`<br>`GetStudyStreakQuery.cs` | **User, Admin** |

---

### 4. Phân hệ 4: Quản trị học liệu & Thống kê hệ thống - CMS (Back-office & Admin Analytics)

*Lưu ý:* Cơ sở dữ liệu hiện tại chỉ gieo hạt 2 roles: **`Admin`** và **`User`**. Do đó, các tác vụ kiểm duyệt của Moderator tạm thời được quy hoạch chung vào nhóm quyền của **`Admin`** (hoặc sẽ gán cho role `Moderator` nếu sau này phát triển thêm).

| UC ID | Tên chức năng | Trạng thái Code | File mã nguồn liên quan | Quyền truy cập đề xuất |
| :---: | :--- | :---: | :--- | :---: |
| **UC-16.1**| Xem danh sách đề thi (CMS) | 🟢 Đã code | `Pages/Admin/Exams/Index.cshtml`<br>`GetAdminExamListQuery.cs`<br>`ExamsController.GetAdminList()` | **Admin** (hoặc Moderator) |
| **UC-16.2**| Tạo đề thi mới | 🟢 Đã code | `Pages/Admin/Exams/_CreateExamModal.cshtml`<br>`CreateExamCommand.cs`<br>`ExamsController.Create()` | **Admin** (hoặc Moderator) |
| **UC-16.3**| Xem chi tiết đề thi (CMS) | 🟢 Đã code | `Pages/Admin/Exams/Detail.cshtml`<br>`GetExamDetailQuery.cs` (admin namespace) | **Admin** (hoặc Moderator) |
| **UC-16.4**| Cập nhật thông tin đề thi | 🟢 Đã code | `Pages/Admin/Exams/Edit.cshtml`<br>`UpdateExamCommand.cs` | **Admin** (hoặc Moderator) |
| **UC-16.5**| Xóa đề thi (Xóa mềm) | 🟢 Đã code | `SoftDeleteExamCommand.cs`<br>`ExamsController.SoftDelete()` | **Admin** (hoặc Moderator) |
| **UC-16.6**| Tìm kiếm/Lọc đề thi (CMS) | 🟢 Đã code | Tích hợp trong `GetAdminExamListQuery.cs` | **Admin** (hoặc Moderator) |
| **UC-17.1**| Xem danh sách câu hỏi | 🟢 Đã code | Tích hợp trong trang chi tiết đề thi admin | **Admin** (hoặc Moderator) |
| **UC-17.2**| Tạo câu hỏi mới | 🟢 Đã code | `Admin/Exams/Questions/` (Part5, Part6, Part7, Listening)<br>Commands trong `Admin/Exams/Commands` | **Admin** (hoặc Moderator) |
| **UC-17.3**| Xem chi tiết câu hỏi | 🟢 Đã code | Tích hợp trong trang chỉnh sửa đề | **Admin** (hoặc Moderator) |
| **UC-17.4**| Cập nhật câu hỏi | 🟢 Đã code | `UpdateQuestionCommand.cs` hoặc tương đương | **Admin** (hoặc Moderator) |
| **UC-17.5**| Xóa câu hỏi | 🟢 Đã code | `DeleteQuestionCommand.cs` | **Admin** (hoặc Moderator) |
| **UC-18.1**<br>đến **18.5**| Quản lý Đoạn văn (Passages) | 🟢 Đã code (Lồng ghép) | Không có phân hệ CRUD Passage độc lập. Đoạn văn được tạo lồng ghép trực tiếp khi tạo câu hỏi Part 6 & Part 7. | **Admin** (hoặc Moderator) |
| **UC-20.1**| Xem báo cáo tiến độ hệ thống | 🟢 Đã code | `Pages/Admin/Report.cshtml`<br>`GetProgressReportQuery.cs` | **Admin** |
| **UC-20.2**| Xem thống kê người dùng | 🟢 Đã code | `Pages/Admin/Dashboard.cshtml`<br>`GetUserStatsQuery.cs` | **Admin** |
| **UC-20.3**| Xem thống kê chất lượng đề thi | 🟢 Đã code | `Pages/Admin/ExamStats.cshtml`<br>`GetExamStatsQuery.cs` | **Admin** |

---

## III. KẾ HOẠCH TRIỂN KHAI PHÂN QUYỀN TRÊN CODE (AUTHORIZATION IMPLEMENTATION PLAN)

Để bảo mật các chức năng đã có code ở trên, bạn cần áp dụng các bước cấu hình sau trong dự án:

### 1. Phân quyền cấp Giao diện (Razor Pages Authorization)
Cấu hình trong file **`Program.cs`** để tự động chặn truy cập vào các thư mục trang cụ thể dựa trên trạng thái đăng nhập hoặc vai trò:

```csharp
builder.Services.AddRazorPages(options =>
{
    // 1. Chỉ cho phép các tài khoản có vai trò "Admin" truy cập toàn bộ thư mục /Admin/
    options.Conventions.AuthorizeFolder("/Admin", "RequireAdminRole");

    // 2. Yêu cầu đăng nhập (bất kể role nào: User hoặc Admin) đối với việc làm bài thi và xem phân tích cá nhân
    options.Conventions.AuthorizeFolder("/Vocabulary");
    options.Conventions.AuthorizeFolder("/Performance");
    options.Conventions.AuthorizePage("/Exams/Take");
    options.Conventions.AuthorizePage("/Exams/Practice");
    options.Conventions.AuthorizePage("/Exams/Results");

    // 3. Các trang công khai (Guest truy cập được)
    options.Conventions.AllowAnonymousToPage("/Exams/Index");
    options.Conventions.AllowAnonymousToPage("/Exams/Detail");
    options.Conventions.AllowAnonymousToPage("/Performance/Leaderboard");
});

// Định nghĩa Policy cho Admin
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdminRole", policy => policy.RequireRole("Admin"));
});
```

### 2. Phân quyền cấp API (API Controllers Authorization)
Bảo vệ các đầu API của các controller bằng thuộc tính `[Authorize]`.

*   **`AchievementsController.cs`:**
    *   Thêm `[Authorize]` lên cấp class. Điều này giúp loại bỏ hoàn toàn đoạn code fallback nguy hiểm `int userId = 2;` vì khi có `[Authorize]`, thuộc tính `_currentUserService.UserId` chắc chắn sẽ có giá trị của người dùng đã đăng nhập hợp lệ.
*   **`ExamsController.cs`:**
    *   Các Action công khai: `GetList` (danh sách đề học viên), `GetDetail` (chi tiết đề học viên) $\rightarrow$ Thêm `[AllowAnonymous]`.
    *   Các Action quản trị: `GetAdminList`, `GetAdminDetail`, `Create`, `SoftDelete`, `Restore` $\rightarrow$ Thêm `[Authorize(Roles = "Admin")]`.

### 3. Điều chỉnh Header/Navigation theo Trạng thái Đăng nhập
Trong file **`Pages/Shared/_Layout.cshtml`** và **`_AdminLayout.cshtml`**, cập nhật UI để ẩn/hiện các liên kết tương ứng:
*   Nếu `User.IsInRole("Admin")` $\rightarrow$ Hiển thị nút truy cập trang **Dashboard Admin** và **CMS Quản lý đề**.
*   Nếu người dùng chưa đăng nhập (`!User.Identity.IsAuthenticated`) $\rightarrow$ Ẩn nút "Hồ sơ", "Sổ tay từ vựng", "Kết quả ôn tập" và hiển thị nút "Đăng nhập".
