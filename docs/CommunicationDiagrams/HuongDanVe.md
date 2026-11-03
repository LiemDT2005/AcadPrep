# Hướng dẫn vẽ Communication Diagram (PlantUML)

Tài liệu chuẩn hoá cách vẽ **Communication Diagram** cho dự án AcadPrep để sơ đồ dễ đọc,
các đường không bị sát nhau và chữ không bị chồng lên nhau.

---

## 1. Khối cấu hình chuẩn (copy vào đầu mỗi file `.puml`)

```plantuml
@startuml
skinparam monochrome true
skinparam shadowing false
skinparam linetype ortho
skinparam backgroundColor #FFFFFF
skinparam defaultFontName "Inter"
skinparam ObjectFontSize 14
skinparam ArrowFontSize 13
skinparam nodesep 130
skinparam ranksep 200
skinparam wrapWidth 150
left to right direction
```

Giải thích các tham số quyết định độ dễ đọc:

| Tham số | Tác dụng | Ghi chú |
|---|---|---|
| `linetype ortho` | Vẽ đường gấp khúc vuông góc (nét thẳng) | Theo yêu cầu style của dự án |
| `nodesep 130` | Khoảng cách giữa các node cùng hàng | Tăng lên nếu các nhãn còn chạm nhau |
| `ranksep 200` | Khoảng cách giữa các hàng (các bước) | Đây là tham số quan trọng nhất để giãn đường |
| `wrapWidth 150` | Tự động xuống dòng nhãn dài | Giữ nhãn hẹp để không đè lên node/đường khác |
| `left to right direction` | Xếp các object theo chiều dọc | Cho layout gọn, dễ đọc hơn (đừng dùng `top to bottom`, sẽ bị dàn ngang và chồng chữ) |
| `monochrome true` | Toàn bộ đen trắng | Theo yêu cầu "tất cả màu trắng, đừng để màu" |

---

## 2. Khai báo thành phần

Dùng `object` cho mọi thành phần (kể cả actor) để tương thích khi import vào Visual Paradigm / draw.io:

```plantuml
object "Actor: Student" as student
object ":Take.cshtml" as view
object ":SubmitTestCommandHandler" as handler
object "SQL Server" as db
```

- Class/PageModel/Handler: dùng tiền tố `:` (ví dụ `:ReviewModel`).
- Client script/asset tĩnh: ghi tên file (ví dụ `exam-session.js`).

---

## 3. Quy tắc đánh số message

Mỗi message được đánh số tuần tự theo thứ tự thực thi, đặt ngay đầu nhãn:

```plantuml
student -> view    : 1: Click "Submit" button
view -> pageModel  : 2: Page Request (attemptId)
pageModel --> view : 11: Pass DTO to View
```

- `->` : lời gọi/đi tới (nét liền).
- `-->`: giá trị trả về/return (nét đứt).
- `x -> x` : self-message (xử lý nội bộ trong cùng một object).
- **Không** dùng gợi ý hướng thủ công (`-right->`, `-down->`) — chúng phá layout tự động và làm chữ chồng nhau.

---

## 4. Mẹo khi các đường vẫn sát nhau / chữ bị chồng

1. **Tăng `ranksep`** (200 → 260 → 320…) trước tiên — hiệu quả nhất để giãn đường.
2. **Tăng `nodesep`** nếu các node cùng hàng chạm nhau.
3. **Rút gọn nhãn**: khi có nhiều message giữa 2 object (>6 đường song song), auto-layout
   sẽ đặt nhãn ở giữa đường và dễ chồng nhau. Hãy viết nhãn ngắn gọn trên sơ đồ và để
   phần mô tả chi tiết trong file `.md` đi kèm.
4. **Giảm số đường song song** giữa hai object nếu có thể (gộp các bước không cần thiết).

---

## 5. Xem trước nhanh (không cần cài Java)

Có thể render qua server công khai để kiểm tra bố cục:

- PlantUML online: <https://www.plantuml.com/plantuml/uml/>
- Kroki: `https://kroki.io/plantuml/png/<encoded>`

Dán code `.puml` vào trình soạn online, kiểm tra không còn cảnh báo và chữ không chồng nhau
trước khi import vào Visual Paradigm (Import from PlantUML) hoặc draw.io.
