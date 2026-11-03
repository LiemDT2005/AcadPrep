# Communication Diagram – UC-9.3: Play Audio (Điều khiển trình phát audio)

> **Diagram Source:** [`CD-UC9.3_Play_Audio.puml`](./CD-UC9.3_Play_Audio.puml)  
> **Tool:** PlantUML (tương thích Visual Paradigm, VS Code)  
> **Author:** Senior Developer  
> **Date:** 2026-07-19

---

## 1. Participants (Các thành phần tham gia)

| Participant | Class / Type | Layer | Vai trò |
|---|---|---|---|
| **Student** | Actor | – | Học viên làm bài thi (Full Test hoặc Practice) |
| **:Take.cshtml / :Practice.cshtml** | View | WebUI | Giao diện HTML hiển thị bài làm và trình phát audio |
| **exam-session.js** | Client Script | WebUI (Static Asset) | Xử lý logic phát nhạc, kiểm soát sự kiện audio và chống tua ở client |
| **audio : HTMLAudioElement** | Web API | Browser Core | Đối tượng thẻ `<audio>` quản lý phát tập tin âm thanh |
| **:Take.cshtml.cs** | PageModel | WebUI | Entry point server-side của trang làm bài thi |
| **:IAppDbContext** | DbContext | Infrastructure | Interface truy vấn thông tin đề thi và phân đoạn audio |
| **SQL Server** | Database (SQL) | External | Cơ sở dữ liệu lưu trữ liên kết file audio và thời gian bắt đầu/kết thúc (seconds) của từng câu |

---

## 2. Vòng đời dữ liệu & Luồng giao tiếp

### A. Chuẩn bị (Loading Phase)
1. Học viên mở giao diện thi/luyện tập. Trình duyệt gửi yêu cầu trang.
2. PageModel nhận yêu cầu và thông qua `IAppDbContext` tải thông tin Attempt/Practice Session kèm danh sách câu hỏi chứa phân đoạn âm thanh (`audioStartSecond`, `audioEndSecond`).
3. Dữ liệu thô được trả về từ cơ sở dữ liệu SQL Server.
4. PageModel render trang và tiêm cấu hình JSON `window.examSessionConfig` xuống Client.
5. Client tải và chạy `exam-session.js`, khởi tạo cấu trúc và chuẩn bị tập tin âm thanh trên thẻ `<audio>`.

### B. Chế độ thi thử nghiêm ngặt (Full Test Mode) - Chống tua/tạm dừng
1. `exam-session.js` thiết lập thuộc tính `controls = false` trên thẻ `<audio>` (ẩn toàn bộ thanh điều khiển mặc định của trình duyệt để học viên không thể tua hoặc tạm dừng).
2. Khi câu hỏi nghe xuất hiện, script kích hoạt lệnh phát tự động (`audioEl.play()`).
3. Trong quá trình phát, script lắng nghe sự kiện `timeupdate` của thẻ audio:
   - Cập nhật tiến trình làm bài.
   - Kiểm tra nếu `currentTime` của audio đạt hoặc vượt quá thời điểm kết thúc câu hỏi (`audioEndSecond`), script sẽ tự động gọi phương thức tạm dừng và khóa phân đoạn đó.
4. Khi audio kết thúc, sự kiện `ended` kích hoạt, script tiến hành lưu tiến trình và tự động chuyển tiếp giao diện sang câu hỏi/nhóm câu hỏi tiếp theo (Auto-Advance).
5. Học viên không thể click tạm dừng hoặc kéo thả thanh trượt thời gian (Audio chỉ được nghe duy nhất 1 lần).

### C. Chế độ luyện tập (Practice Mode)
1. `exam-session.js` hiển thị thanh giao diện điều khiển tùy biến (Play/Pause/Seek/Replay).
2. Học viên có thể click vào các nút điều khiển hoặc thanh tiến trình để chuyển đổi vị trí phát âm thanh.
3. Khi click tua, script cập nhật `currentTime` của thẻ audio và cập nhật lại giao diện nhãn thời gian.

---

## 3. Quy tắc nghiệp vụ (Business Rules) áp dụng

- **BR-Listening-Lock:** Trong chế độ Full Test, audio cho các phần nghe (Part 1-4) chỉ được phát **đúng 1 lần**. Mọi hành vi can thiệp vào trình phát âm thanh đều bị ngăn chặn bằng cách ẩn nút controls và xử lý sự kiện `seeked`/`timeupdate` chặt chẽ ở client.
- **BR-Auto-Advance:** Tự động chuyển câu khi audio của phân đoạn đó chạy hết.

---

## 4. Hướng dẫn import vào Visual Paradigm

1. Tạo một **Communication Diagram** mới trong dự án Visual Paradigm.
2. Sao chép nội dung code trong file [`CD-UC9.3_Play_Audio.puml`](./CD-UC9.3_Play_Audio.puml).
3. Sử dụng chức năng **Import from PlantUML** của Visual Paradigm để tự động vẽ sơ đồ cộng tác theo đúng trình tự thông điệp.
