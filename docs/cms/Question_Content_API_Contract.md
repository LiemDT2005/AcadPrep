# API Contract — Lưu nội dung câu hỏi TOEIC (Composite)

Tài liệu mô tả **request/response** cho các API lưu block nội dung theo Part.  
Bám theo pattern hiện tại: `Result<T>`, MediatR Command, `api/Exams` controller.

**Liên quan:** [Question_Creation_Screens_Spec.md](./Question_Creation_Screens_Spec.md)

---

## 1. Quy ước chung

### Base URL

```
/api/exams/{examId}/content
```

### Response envelope

Mọi endpoint trả về `Result<T>` (camelCase):

```json
{
  "isSuccess": true,
  "error": null,
  "data": { ... }
}
```

Lỗi validation (400):

```json
{
  "isSuccess": false,
  "error": "Dữ liệu đầu vào không hợp lệ.",
  "code": 400,
  "validationErrors": [
    { "field": "questions[0].correctOption", "message": "Correct option is required." }
  ]
}
```

Lỗi business (400):

```json
{
  "isSuccess": false,
  "error": "Question number 131 already exists in this exam.",
  "data": null
}
```

### HTTP status

| Status | Khi nào |
|---|---|
| `201 Created` | Tạo mới thành công |
| `200 OK` | Cập nhật / xóa thành công |
| `400 BadRequest` | Validation / business rule |
| `404 NotFound` | Exam hoặc resource không tồn tại |

### Media (ảnh, audio)

Payload chỉ nhận **URL string** (upload file qua endpoint riêng trước):

```
POST /api/media/upload   → { "url": "/uploads/..." }
```

### Shared types

#### `QuestionOptionInput`

```json
{
  "letter": "A",
  "text": "will be"
}
```

| Field | Type | Required | Rule |
|---|---|---|---|
| `letter` | string | ✓ | `"A"` \| `"B"` \| `"C"` \| `"D"` |
| `text` | string | ✓ | max 2000 chars |

#### `QuestionInput`

```json
{
  "questionNumber": 131,
  "questionText": "Optional prompt text",
  "imageUrl": null,
  "audioUrl": null,
  "audioStartSecond": null,
  "audioEndSecond": null,
  "correctOption": "B",
  "questionType": null,
  "topicTag": "grammar",
  "passageId": null,
  "options": [
    { "letter": "A", "text": "will be" },
    { "letter": "B", "text": "is being" },
    { "letter": "C", "text": "has been" },
    { "letter": "D", "text": "was" }
  ]
}
```

| Field | Type | Required | Rule |
|---|---|---|---|
| `questionNumber` | int | ✓ | Unique trong exam |
| `questionText` | string? | | |
| `imageUrl` | string? | | max 500 |
| `audioUrl` | string? | | max 500 |
| `audioStartSecond` | int? | | ≥ 0 |
| `audioEndSecond` | int? | | > start |
| `correctOption` | string | ✓ | A/B/C/D |
| `questionType` | string? | | max 100 |
| `topicTag` | string? | | max 150 |
| `passageId` | int? | | Chỉ Part 7 optional — ref passage trong cùng request (by index) hoặc null = whole set |
| `options` | array | ✓ | Đúng 4 phần tử, letter A–D unique |

#### `PassageInput`

```json
{
  "displayOrder": 1,
  "content": "Dear Mr. Lee,\n\nThank you for...",
  "imageUrl": null
}
```

| Field | Type | Required | Rule |
|---|---|---|---|
| `displayOrder` | int | ✓ | 1–3, unique trong set |
| `content` | string? | | Ít nhất một trong `content` \| `imageUrl` |
| `imageUrl` | string? | | max 500 |

#### `ListeningGroupMediaInput`

```json
{
  "audioUrl": "/uploads/audio/part3-conv1.mp3",
  "audioStartSecond": null,
  "audioEndSecond": null,
  "imageUrl": "/uploads/images/part1-photo.jpg",
  "useExamFullAudio": false
}
```

| Field | Type | Required | Rule |
|---|---|---|---|
| `audioUrl` | string? | | Bắt buộc P2–4 nếu không dùng exam audio |
| `audioStartSecond` | int? | | Khi `useExamFullAudio = true` |
| `audioEndSecond` | int? | | Khi `useExamFullAudio = true` |
| `imageUrl` | string? | | Bắt buộc P1 |
| `useExamFullAudio` | bool | | default `false` |

---

## 2. Tạo mới (POST)

### 2.1 Part 5 — Câu đơn

**Screen:** S3  
**Endpoint:**

```
POST /api/exams/{examId}/content/part5-questions
```

**Request body:**

```json
{
  "question": {
    "questionNumber": 101,
    "questionText": "The report ___ submitted before Friday.",
    "correctOption": "C",
    "questionType": "grammar",
    "topicTag": "verb-tense",
    "options": [
      { "letter": "A", "text": "is" },
      { "letter": "B", "text": "are" },
      { "letter": "C", "text": "will be" },
      { "letter": "D", "text": "has been" }
    ]
  }
}
```

**Response `201` — `CreatePart5QuestionResult`:**

```json
{
  "isSuccess": true,
  "error": null,
  "data": {
    "questionId": 501,
    "questionNumber": 101,
    "part": 5
  }
}
```

**Backend transaction:** insert `Question` (part=5) + 4 `QuestionOption`.

---

### 2.2 Part 1–4 — Listening Group

**Screen:** S2  
**Endpoint:**

```
POST /api/exams/{examId}/content/listening-groups
```

**Query:**

| Param | Type | Required |
|---|---|---|
| `part` | int | ✓ | 1–4 |

**Request body:**

```json
{
  "name": "Part 3 - Conversation 1",
  "media": {
    "audioUrl": "/uploads/audio/conv1.mp3",
    "audioStartSecond": null,
    "audioEndSecond": null,
    "imageUrl": null,
    "useExamFullAudio": false
  },
  "questions": [
    {
      "questionNumber": 32,
      "questionText": "What are the speakers mainly discussing?",
      "correctOption": "A",
      "options": [
        { "letter": "A", "text": "A project deadline" },
        { "letter": "B", "text": "A vacation plan" },
        { "letter": "C", "text": "A new employee" },
        { "letter": "D", "text": "A software update" }
      ]
    },
    {
      "questionNumber": 33,
      "questionText": "What does the woman suggest?",
      "correctOption": "D",
      "options": [
        { "letter": "A", "text": "Cancel the meeting" },
        { "letter": "B", "text": "Hire a consultant" },
        { "letter": "C", "text": "Postpone the launch" },
        { "letter": "D", "text": "Work overtime" }
      ]
    }
  ]
}
```

**Response `201` — `CreateListeningGroupResult`:**

```json
{
  "isSuccess": true,
  "error": null,
  "data": {
    "questionGroupId": 10,
    "part": 3,
    "name": "Part 3 - Conversation 1",
    "questionIds": [32, 33, 34]
  }
}
```

**Validation theo Part:**

| Part | Media rule | Gợi ý số câu |
|---|---|---|
| 1 | `imageUrl` required (group hoặc per question) | 6 |
| 2 | `audioUrl` hoặc exam full audio | ~5/set |
| 3 | `audioUrl` hoặc exam full audio | 3/set |
| 4 | `audioUrl` hoặc exam full audio | 3/set |

**Backend transaction:** insert `QuestionGroup` → insert `Question` × n (cùng `questionGroupId`, `part`) → insert `QuestionOption` × 4n.

---

### 2.3 Part 6 — Text Completion Set

**Screen:** S4  
**Endpoint:**

```
POST /api/exams/{examId}/content/text-completion-sets
```

**Request body:**

```json
{
  "passage": {
    "displayOrder": 1,
    "content": "The conference will be held on (131) at the downtown office. All employees are expected to (132) on time.",
    "imageUrl": null
  },
  "questions": [
    {
      "questionNumber": 131,
      "questionText": null,
      "correctOption": "B",
      "options": [
        { "letter": "A", "text": "Monday" },
        { "letter": "B", "text": "Tuesday" },
        { "letter": "C", "text": "Wednesday" },
        { "letter": "D", "text": "Thursday" }
      ]
    },
    {
      "questionNumber": 132,
      "questionText": null,
      "correctOption": "A",
      "options": [
        { "letter": "A", "text": "arrive" },
        { "letter": "B", "text": "arriving" },
        { "letter": "C", "text": "arrived" },
        { "letter": "D", "text": "arrives" }
      ]
    }
  ]
}
```

**Response `201` — `CreateTextCompletionSetResult`:**

```json
{
  "isSuccess": true,
  "error": null,
  "data": {
    "passageId": 20,
    "questionIds": [131, 132, 133, 134],
    "part": 6
  }
}
```

**Validation:**

- Đúng **4 câu** / set (configurable constant)
- `passage.content` hoặc `passage.imageUrl` phải có
- `passage.questionGroupId` = null
- Marker `(131)` trong content khớp `questionNumber` (warning hoặc error tùy policy)

**Backend transaction:** insert `Passage` → insert `Question` × 4 (`part=6`, `passageId`) → insert options.

---

### 2.4 Part 7 — Reading Set

**Screen:** S5  
**Endpoint:**

```
POST /api/exams/{examId}/content/reading-sets
```

**Request body:**

```json
{
  "name": "Set 05 - Double passage",
  "passages": [
    {
      "displayOrder": 1,
      "content": "Dear Mr. Lee,\n\nWe are writing to confirm your interview...",
      "imageUrl": null
    },
    {
      "displayOrder": 2,
      "content": null,
      "imageUrl": "/uploads/images/job-application-form.png"
    }
  ],
  "questions": [
    {
      "questionNumber": 165,
      "questionText": "What is the purpose of the email?",
      "correctOption": "B",
      "passageId": null,
      "options": [
        { "letter": "A", "text": "To cancel an interview" },
        { "letter": "B", "text": "To confirm an interview" },
        { "letter": "C", "text": "To request a reference" },
        { "letter": "D", "text": "To announce a promotion" }
      ]
    },
    {
      "questionNumber": 166,
      "questionText": "What is indicated about the form?",
      "correctOption": "C",
      "passageId": null,
      "options": [
        { "letter": "A", "text": "It must be notarized" },
        { "letter": "B", "text": "It is optional" },
        { "letter": "C", "text": "It should be completed in advance" },
        { "letter": "D", "text": "It is available online only" }
      ]
    }
  ]
}
```

**Ghi chú `passageId` trong question:** Khi create, client gửi `null` (câu hỏi về cả set) hoặc `passageIndex` (0-based) — khuyến nghị dùng field rõ hơn:

```json
"linkedPassageDisplayOrder": 2
```

**Response `201` — `CreateReadingSetResult`:**

```json
{
  "isSuccess": true,
  "error": null,
  "data": {
    "questionGroupId": 15,
    "name": "Set 05 - Double passage",
    "passageIds": [30, 31],
    "questionIds": [165, 166, 167, 168, 169],
    "part": 7
}
```

**Validation** (`Part7ReadingSetValidator`):

- 1–3 passages
- 2–5 questions
- Mỗi passage: `content` hoặc `imageUrl`
- `displayOrder` unique

**Backend transaction:** insert `QuestionGroup` → insert `Passage` × n (`questionGroupId`) → insert `Question` × m (`part=7`, `questionGroupId`) → insert options.

---

## 3. Cập nhật (PUT)

Cùng payload với POST, thêm ID resource ở route. Backend **replace** toàn bộ block trong transaction.

| Loại | Endpoint |
|---|---|
| Part 5 | `PUT /api/exams/{examId}/content/part5-questions/{questionId}` |
| Listening | `PUT /api/exams/{examId}/content/listening-groups/{groupId}` |
| Part 6 | `PUT /api/exams/{examId}/content/text-completion-sets/{passageId}` |
| Part 7 | `PUT /api/exams/{examId}/content/reading-sets/{groupId}` |

**Request:** Giống POST tương ứng.

**Response `200`:** Cùng shape `data` như POST create.

**Chiến lược update:**

1. Validate exam + ownership IDs
2. Transaction:
   - Update parent (group/passage)
   - Upsert questions (match by `questionNumber` hoặc `questionId` nếu client gửi)
   - Xóa questions không còn trong payload
   - Replace options per question

**Optional:** Client gửi thêm `questionId` trong `QuestionInput` khi edit để backend biết update vs insert:

```json
{
  "questionId": 501,
  "questionNumber": 101,
  ...
}
```

---

## 4. Xóa (DELETE)

| Loại | Endpoint | Xóa kèm |
|---|---|---|
| 1 câu Part 5 | `DELETE .../part5-questions/{questionId}` | options |
| Listening group | `DELETE .../listening-groups/{groupId}` | questions + options |
| Part 6 set | `DELETE .../text-completion-sets/{passageId}` | 4 questions + options |
| Part 7 set | `DELETE .../reading-sets/{groupId}` | passages + questions + options |

**Response `200`:**

```json
{
  "isSuccess": true,
  "error": null,
  "data": {
    "deletedQuestionIds": [165, 166],
    "deletedPassageIds": [30, 31],
    "deletedQuestionGroupId": 15
  }
}
```

**Guard:** Nếu exam đã có `ExamAttempt` submitted → trả `400` hoặc yêu cầu flag `force=true` (policy tùy product).

---

## 5. Mapping Command (MediatR) gợi ý

```
Features/Admin/ExamContent/
├── Commands/
│   ├── CreatePart5Question/
│   │   ├── CreatePart5QuestionCommand.cs
│   │   ├── CreatePart5QuestionDto.cs
│   │   ├── CreatePart5QuestionCommandHandler.cs
│   │   └── CreatePart5QuestionCommandValidator.cs
│   ├── CreateListeningGroup/
│   ├── CreateTextCompletionSet/
│   ├── CreateReadingSet/
│   ├── UpdatePart5Question/
│   ├── UpdateListeningGroup/
│   ├── UpdateTextCompletionSet/
│   ├── UpdateReadingSet/
│   └── DeleteExamContent/...
```

**Controller** (mở rộng `ExamsController` hoặc tách `ExamContentController`):

```csharp
[Route("api/exams/{examId:int}/content")]
public class ExamContentController : ApiControllerBase
{
    [HttpPost("part5-questions")]
    [HttpPost("listening-groups")]
    [HttpPost("text-completion-sets")]
    [HttpPost("reading-sets")]
    // PUT, DELETE tương ứng
}
```

---

## 6. Bảng tóm tắt nhanh

| Part | Screen | Method | Path suffix | Entities tạo |
|---|---|---|---|---|
| 5 | S3 | POST | `part5-questions` | Question ×1 |
| 1–4 | S2 | POST | `listening-groups?part={n}` | QuestionGroup + Questions |
| 6 | S4 | POST | `text-completion-sets` | Passage + Questions ×4 |
| 7 | S5 | POST | `reading-sets` | QuestionGroup + Passages + Questions |

**Mỗi lần Save trên UI = 1 HTTP request = 1 DB transaction.**

---

## 7. Upload media (phụ trợ)

```
POST /api/media/upload
Content-Type: multipart/form-data
```

**Form field:** `file`  
**Response:**

```json
{
  "isSuccess": true,
  "data": {
    "url": "/uploads/2026/07/audio-abc123.mp3",
    "contentType": "audio/mpeg",
    "sizeBytes": 1048576
  }
}
```

Client upload trước → lấy `url` → gắn vào `audioUrl` / `imageUrl` trong composite request.

---

## 8. Checklist implement

- [ ] 4 POST commands + validators
- [ ] 4 PUT commands (replace strategy)
- [ ] DELETE variants + attempt guard
- [ ] Transaction wrapper (`IAppDbContext` + `SaveChangesAsync` once)
- [ ] Unique `QuestionNumber` check per exam
- [ ] `Part7ReadingSetValidator` reuse
- [ ] Swagger examples cho từng endpoint
- [ ] Razor Pages gọi API hoặc MediatR trực tiếp (giống `CreateExam`)
