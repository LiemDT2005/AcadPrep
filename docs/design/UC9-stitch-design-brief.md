# AcadPrep UC-9 — Stitch Design Brief

> Dùng file này làm context khi gen UI trên Google Stitch MCP.
> Project Stitch gợi ý: **AcadPrep — Test Engine**

## Design System (đồng bộ với codebase hiện tại)

| Token | Value |
|-------|-------|
| Primary | `#1e00a9` |
| Secondary | `#9d4300` |
| Surface | `#f9f9ff` |
| Surface container | `#f0ecf9` |
| On surface | `#1b1b24` |
| Font | Inter |
| Icons | Google Material Symbols Outlined |
| Framework | Tailwind CSS (CDN style) |
| Border radius | `rounded-2xl` cho cards, `rounded-lg` cho inputs |
| Layout | Mobile-first, desktop 2-column cho question area |

## Screens cần thiết kế (theo thứ tự)

### S1 — Exam Detail: Practice Tab (UC-9.2)
**Route:** `/Exams/Detail/{id}`

```
Design a TOEIC exam detail page — Practice Mode tab active.

Header: exam title, year badge, stats row (duration, questions, attempts).

Tabs: Practice Mode (active) | Full Test | Info

Main content:
- Grid 2-col of Part cards (Part 1–7), each with checkbox, icon, question count, time estimate, difficulty badge, short description
- Time limit dropdown: None / 15 / 30 / 45 / 60 / 90 / 120 minutes
- Sticky bottom bar: "3 Parts selected · 45 questions · ~35 mins" + orange CTA "LUYỆN TẬP"

Right sidebar (logged-in): profile card, performance sparkline, practice history list.

Style: clean edtech, purple primary, warm secondary accent. Match Google Material 3 inspired palette above.
```

---

### S2 — Exam Detail: Full Test Tab (UC-9.1)
**Route:** `/Exams/Detail/{id}` — tab Full Test

```
Design Full Test tab on exam detail page.

Purple hero card with psychology icon.
Headline: "Full Test Simulation"
Bullet list:
- 200 Questions (100 Listening / 100 Reading)
- 120 Minutes strict countdown
- Official TOEIC Flow — No pausing permitted
- Audio played once per question (Parts 1-4)

Full-width primary CTA: "Start Full Test" with rocket icon.

Minimal, confident, exam-serious tone.
```

---

### S3 — Exam Detail: Resume Banner (UC-9.5)
**Route:** `/Exams/Detail/{id}` — in-progress state

```
Design in-progress exam alert banner at top of exam detail page.

Warning callout:
"Bạn có bài thi đang làm dở — 01:12:08 còn lại · 67/200 câu · Part 3"

Two buttons: primary "Resume Test" | ghost "Abandon"

Below: practice history item showing "Full Test · In Progress · Mar 12" with Resume link on hover.
```

---

### S4 — Pre-start Confirmation Modal (UC-9.1)
**Modal overlay on Detail page**

```
Design modal dialog — "Bắt đầu Full Test?"

Body text + 4 rule bullets:
- 120 phút đếm ngược liên tục
- Phần Nghe P1-4: audio tự phát, không tua/tạm dừng
- Không quay lại Part đã qua
- Điểm chỉ ghi nhận khi Nộp bài hoặc hết giờ

Footer: [Hủy] [Bắt đầu ngay]

Centered modal, backdrop blur, rounded-2xl.
```

---

### S5 — Full Test Taking Page (UC-9.1, 9.3, 9.4, 9.5)
**Route:** `/Exams/Take?attemptId=` — MAIN SCREEN

```
Design fullscreen TOEIC test-taking interface. NO main site navbar.

FIXED HEADER:
- Left: badge "FULL TEST" + exam title
- Center: current part label "Part 3"
- Right: countdown timer "01:47:32" (tabular nums, red when <5min) + primary button "Nộp bài"
- Sub-row: pill "AUDIO LOCKED" when in listening section

PART STEPPER:
P1 P2 P3 P4 | P5 P6 P7 — current part highlighted, future parts muted, past parts checkmarked

AUDIO PLAYER (locked state):
- Auto-playing, no pause/seek buttons visible
- Read-only progress bar
- Label: "Playing once — no controls"
- Time: 0:42 / 1:15

MAIN AREA — 2 columns desktop:
Left 50%: question content (image for P1, text for P5, passage for P7)
Right 50%: 4 radio options A B C D, selected state with primary border

BOTTOM NAV:
[← Prev disabled] — "Question 23 / 200" — [Next →]

QUESTION PALETTE:
Grid of 200 numbered cells (20 cols), green=answered, blue=current, gray=unanswered
Collapsible on mobile

Dark-on-light, distraction-free, exam mode.
```

---

### S6 — Practice Taking Page (UC-9.2, 9.3, 9.4)
**Route:** `/Exams/Practice?sessionId=`

```
Same layout as Full Test (S5) but differences:
- Badge "PRACTICE" (orange/secondary) instead of FULL TEST
- Chips showing selected parts: P2 P5 P7
- Audio player WITH controls: play, pause, seek, replay
- Prev/Next both enabled
- Question palette smaller (30 cells for subset)
- Optional timer in header

Friendlier, learning-focused tone vs exam-serious.
```

---

### S7 — Part 1: Photographs (UC-9.3)
**Variant layout within Take page**

```
Part 1 question layout inside test shell.

Left: large 4:3 photograph placeholder with rounded corners
Right: "Question 1" + 4 answer options vertically stacked

Audio player above (locked, auto-play).
Footer hint: "Audio plays automatically. Next enabled after audio ends."

Clean, image-focused.
```

---

### S8 — Part 3: Conversations (UC-9.3)
**Variant — question group**

```
Part 3 layout: one shared audio clip for questions 32-34.

Below audio: 3 question cards in a row (desktop), stacked (mobile).
Each card: question number, stem text, 4 options A-D.

Group label: "Conversation — Questions 32-34"
```

---

### S9 — Part 7: Reading (passage layout)
**Variant — sticky passage**

```
Part 7 reading comprehension layout.

Left column (sticky on desktop): long passage text in scrollable panel — email/article format with subtle paper background
Right column: questions Q176, Q177, Q178 each with options

Passage takes ~45% width on desktop.
```

---

### S10 — Listening → Reading Transition (UC-9.1)
**Modal overlay mid-test**

```
Transition modal after Part 4 ends.

Icon + headline: "Phần Nghe đã kết thúc"
Subtext: "Bạn sắp bắt đầu Part 5 — Incomplete Sentences"
Timer note: "Thời gian còn lại: 00:52:10"
CTA: "Bắt đầu Part 5"

Celebratory but focused — not distracting.
```

---

### S11 — Submit Confirmation Modal (UC-9.4)
```
Modal: "Xác nhận nộp bài"

3 stat boxes: Đã làm 187 | Chưa làm 13 | Đánh dấu 5
Warning: "Sau khi nộp, bạn không thể sửa đáp án"
Buttons: [Tiếp tục làm] [Nộp bài]
```

---

### S12 — Time Expired Overlay (UC-9.4)
```
Fullscreen dim overlay.

Center card:
- Large "Hết giờ!" in error red
- "Đang chấm điểm…" + spinner
- "Chuyển đến kết quả trong giây lát"

No dismiss button — auto-redirect.
```

---

### S13 — Leave Warning Modal (UC-9.5)
```
Modal: "Rời khỏi bài thi?"

"Tiến độ sẽ được lưu tạm. Bạn có thể Resume sau."
Info note: "Timer đóng băng khi thoát"

[Tiếp tục làm] [Lưu & thoát]
```

---

### S14 — Resume Confirmation Modal (UC-9.5)
```
Modal: "Tiếp tục bài thi?"

2x2 grid stats:
- Thời gian còn: 01:12:08
- Đã trả lời: 67/200
- Part hiện tại: 3
- Bắt đầu: Mar 12, 14:30

[Hủy] [Tiếp tục]
```

---

### S15 — Results Page (UC-9.4 → UC-10.1)
**Route:** `/Exams/Results?attemptId=`

```
Post-test results page.

Centered layout:
- "Hoàn thành!" headline
- Hero score card: "720 / 990" large typography
- Two sub-cards: Listening 360 | Reading 360
- Part breakdown horizontal bar chart
- CTAs: [Xem chi tiết từng câu] [Làm lại]

Celebratory purple gradient accents (subtle, not garish).
```

---

## Stitch MCP workflow (sau khi kết nối)

1. `create_project` → tên: **AcadPrep Test Engine**
2. Gen từng screen với prompt ở trên (`generate_screen` / `generate_screen_from_text`)
3. Sau S5 (Full Test shell), dùng `extract_design_context` / design DNA cho các screen còn lại
4. `get_screen_code` → export HTML/Tailwind
5. Convert sang Razor Pages trong `Pages/Exams/`

## Lệnh gợi ý trong Cursor (sau khi MCP Stitch sẵn sàng)

```
Dùng Stitch MCP:
1. Tạo project "AcadPrep Test Engine"
2. Gen screen S5 (Full Test Taking Page) theo docs/design/UC9-stitch-design-brief.md
3. Gen S6 Practice variant cùng design DNA
4. Gen các modal S4, S11, S12, S13, S14
5. Export code và map sang Razor Pages
```
