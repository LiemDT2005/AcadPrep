# Stitch Design Prompts — Question Content CMS

Paste each prompt into **Stitch -> Generate from text** in the **AcadPrep** project (`12759125668144641551`).

**References:** [Question_Content_API_Contract.md](./Question_Content_API_Contract.md) · [Question_Creation_Screens_Spec.md](./Question_Creation_Screens_Spec.md)

**Shared style (applies to every screen):**
- Material Design 3, light theme, primary blue `#1e00a9` / `#3525cd`
- Font Inter + Material Symbols Outlined
- Left moderator sidebar matching the existing **Admin Dashboard** / **Create New Exam Modal**
- Desktop 1280px, all visible UI text in English
- Each form save maps to 1 composite API request

| Setting | Value |
|---|---|
| `deviceType` | `DESKTOP` |
| `modelId` | `GEMINI_3_1_PRO` |
| `projectId` | `12759125668144641551` |

---

## UX rule (important for all screens)

**Contextual navigation — do NOT ask Part twice.**

| User action | Next screen |
|---|---|
| Click add button **inside Part N accordion** on S0 | Go **directly** to S2/S3/S4/S5 for that Part — **skip S1** |
| Click **"+ Add Content"** in S0 **header** (global, no Part context) | Open **S1** modal to pick Part |

Per-Part button labels on S0:

| Part | Button label on S0 | Goes to |
|---|---|---|
| 1–4 | `+ Add Listening Group` | S2 |
| 5 | `+ Add Question` | S3 |
| 6 | `+ Add Passage` | S4 |
| 7 | `+ Add Reading Set` | S5 |

---

## S0 — Exam Edit Hub (Question Management)

**API context:** Hub displays saved content blocks; each block maps to one `POST /api/exams/{examId}/content/*` endpoint

**Đặt tên screen:** `S0 - Exam Edit Hub - Question Management`

```
Design S0 — Exam Edit Question Management Hub for AcadPrep TOEIC CMS.

Match existing AcadPrep Admin Dashboard and Create New Exam Modal style (MD3, primary blue, Inter, sidebar).

HEADER:
- Back "Back to Exam List" + exam title "ETS TOEIC 2024 Test 1"
- Badge EXAM ID #42, duration 120 minutes
- Outline button "View Details"
- Primary outline button in header: "+ Add Content" (GLOBAL ONLY — opens S1 modal when user has not chosen a Part yet)

PROGRESS BAR (7 segments):
Part 1 Photographs 4/6 · Part 2 Q-Response 18/25 · Part 3 Conversations 12/39 · Part 4 Talks 8/30 · Part 5 Incomplete 22/30 · Part 6 Text Completion 8/16 · Part 7 Reading 30/54
Each segment: colored fill by completion %, tooltip with part name.

ACCORDION BY PART (7 sections) — each section has its OWN contextual add button (NOT the generic "+ Add Content"):

Part 3 expanded example:
- Section header: "Part 3: Conversations" chip + "12/39 questions"
- Primary button in section header: "+ Add Listening Group" (NOT "+ Add Content" — goes directly to S2 wizard for Part 3, no Part picker modal)
- Content cards:
  • "Part 3 - Conversation 1" · 3 questions · headphone icon · Edit · Delete
  • "Part 3 - Conversation 2" · 3 questions · headphone icon · Edit · Delete

Part 1 collapsed empty state: icon + "No questions yet" + button "+ Add Listening Group"

Part 5 section: button "+ Add Question" (goes to S3)
Part 5 cards: "Q101: The report ___ submitted before Friday." · grammar tag

Part 6 section: button "+ Add Passage" (goes to S4)
Part 6 cards: "Passage #2 · 4 blanks (131-134)"

Part 7 section: button "+ Add Reading Set" (goes to S5)
Part 7 cards: "Set 05 - Double Passage · 2 passages · 5 questions"

Use subtle drag handle icons on cards. All visible UI text must be in English.

DO NOT put "+ Add Content" inside each Part accordion — use the per-Part labels above.
Only the header has "+ Add Content" for the optional global Part picker flow.
```

---

## S1 — Part Picker Modal (GLOBAL ADD ONLY — optional)

**When shown:** Only when user clicks header **"+ Add Content"** on S0.  
**When NOT shown:** User clicked "+ Add Listening Group" / "+ Add Question" / "+ Add Passage" / "+ Add Reading Set" inside a Part section.

**API context:** Choose flow → S2/S3/S4/S5 and the corresponding endpoint

**Đặt tên screen:** `S1 - Add Content Part Picker Modal (Global)`

```
Design S1 — Modal "Add Content" overlay on the Exam Edit page (dimmed backdrop).

IMPORTANT UX: This modal is ONLY shown from the HEADER button "+ Add Content" on S0.
It is NOT shown when the user already clicked a per-Part button inside an accordion (those go directly to S2-S5).

Modal ~640px centered, title "Add Content to ETS TOEIC 2024 Test 1", subtitle "Choose which Part to add content to", close X.

Grid 2 columns × 4 rows = 7 Part cards (last row 1 card centered):

| Part | Label | Description | Badge |
| Part 1 | Photographs | 1 image + multiple picture-based questions | 4/6 |
| Part 2 | Question-Response | 1 audio prompt + response questions | 18/25 |
| Part 3 | Conversations | 1 conversation + 3 questions per set | 12/39 |
| Part 4 | Talks | 1 talk + 3 questions per set | 8/30 |
| Part 5 | Incomplete Sentences | Standalone sentence completion question | 22/30 |
| Part 6 | Text Completion | 1 passage + 4 blanks | 8/16 |
| Part 7 | Reading Comprehension | 1-3 passages + 2-5 questions per set | 30/54 |

Each card: icon (photo, headphones, chat, mic, edit_note, article, menu_book), hover border primary, click affordance.
Clicking a card navigates to the matching wizard (S2-S5).
Footer: outline button "Cancel".

Match Create New Exam Modal styling exactly.
```

---

## S2 — Listening Group Wizard (Part 1–4)

**Entry:** Direct from S0 Part 1–4 accordion button "+ Add Listening Group", OR from S1 after picking Part 1–4.  
**API:** `POST /api/exams/{examId}/content/listening-groups?part={1-4}`

**Fields:** `name`, `media.audioUrl`, `media.imageUrl`, `media.useExamFullAudio`, `media.audioStartSecond`, `media.audioEndSecond`, `questions[]`

**Screen name:** `S2 - Create Listening Group Wizard - Part 3`

```
Design S2 — 3-step wizard "Create Listening Group" for Part 3 Conversations.

Entry context: user already chose Part 3 (from S0 accordion or S1). Do NOT show a Part picker inside this wizard.

Layout: left form 60% + right live preview 40% (learner exam view mock).

BREADCRUMB: Exam Edit > Part 3 > New Listening Group

STEPPER: Step 1/3 Group & Media · Step 2/3 Questions · Step 3/3 Review

STEP 1 — Group & Media:
- Badge read-only "Part 3: Conversations" (fixed, not editable)
- Text input "Group Name" * placeholder "Part 3 - Conversation 1"
- Radio "Audio Source": Upload File | Use Full Exam Audio
- File upload zone MP3 (dashed border, cloud icon) + URL fallback field
- Number inputs "Start Second" / "End Second" (shown when full exam audio is selected)
- Mini audio player preview bar

STEP 2 — Questions (show 2 of 3 questions expanded):
Repeater cards, button "+ Add Question"
Per question:
- Number "Question Number" (32, 33)
- Textarea "Question Text"
- 4 option inputs A B C D with labels
- Radio row "Correct Answer" A/B/C/D
- Tag input "Topic Tag" optional

STEP 3 — Review:
Summary table: group name, audio file, 3 questions with correct answer highlighted green.
Validation error box example: "questions[0].correctOption: Required"
Buttons: Back | primary "Save Group" (maps to POST listening-groups)

Preview panel: conversation audio UI mock with question 32 and 4 options.
All visible UI text in English. MD3 AcadPrep admin style.
```

---

## S3 — Part 5 Single Question

**Entry:** Direct from S0 Part 5 "+ Add Question", OR from S1 Part 5 card.  
**API:** `POST /api/exams/{examId}/content/part5-questions`

**Fields:** `question.questionNumber`, `question.questionText`, `question.correctOption`, `question.questionType`, `question.topicTag`, `question.options[4]`

**Screen name:** `S3 - Create Part 5 Question Form`

```
Design S3 — Single page form "Create Part 5 Question" with split layout.

BREADCRUMB: Exam Edit > Part 5 > New Question

LEFT FORM:
- Badge "Part 5: Incomplete Sentences" (fixed, read-only)
- Number input "Question Number" * value 101
- Textarea "Question Text" * with sample "The report ___ submitted before Friday."
- Section "Answer Options" with 4 labeled text inputs A B C D
- Radio group "Correct Answer" with C selected
- Dropdown "Question Type" optional: Grammar / Vocabulary
- Tag chip input "Topic tag": verb-tense

RIGHT PREVIEW (learner view):
TOEIC Part 5 question card showing sentence with blank and 4 radio options, timer bar subtle.

FOOTER: Cancel | Save | primary "Save & Add Another"

Inline validation example under option B: red text "Required"
AcadPrep MD3 moderator style. No Part picker on this page.
```

---

## S4 — Text Completion Set (Part 6)

**Entry:** Direct from S0 Part 6 "+ Add Passage", OR from S1 Part 6 card.  
**API:** `POST /api/exams/{examId}/content/text-completion-sets`

**Fields:** `passage.content`, `passage.imageUrl`, `passage.displayOrder`, `questions[4]`

**Screen name:** `S4 - Create Part 6 Text Completion Wizard`

```
Design S4 — 2-step wizard "Create Part 6 Text Completion Set".

BREADCRUMB: Exam Edit > Part 6 > New Passage Set

STEPPER: Step 1/2 Passage · Step 2/2 Questions

STEP 1 — Passage:
- Badge read-only "Part 6: Text Completion"
- Radio "Passage Type": Text | Image | Both
- Large textarea with sample: "The conference will be held on (131) at the downtown office. All employees are expected to (132) on time."
- Helper chips: [Insert (131)] [Insert (132)] [Insert (133)] [Insert (134)]
- Preview below: passage with blanks (131)(132) highlighted in primary color boxes

STEP 2 — 4 Questions (auto-generated cards):
Card Q131: number 131, 4 options A-D, correct B selected
Card Q132: number 132, 4 options, correct A
Cards Q133 Q134 collapsed/summary

Full-width passage preview at the top of step 2.
Buttons: Back | primary "Save Part 6 Set"

Maps to POST text-completion-sets. All visible UI text in English. MD3 AcadPrep.
No Part picker — user already came from Part 6 context.
```

---

## S5 — Reading Set (Part 7)

**Entry:** Direct from S0 Part 7 "+ Add Reading Set", OR from S1 Part 7 card.  
**API:** `POST /api/exams/{examId}/content/reading-sets`

**Fields:** `name`, `passages[1-3]`, `questions[2-5]`, `linkedPassageDisplayOrder`

**Screen name:** `S5 - Create Part 7 Reading Set Wizard`

```
Design S5 — 3-step wizard "Create Part 7 Reading Set".

BREADCRUMB: Exam Edit > Part 7 > New Reading Set

STEPPER: Step 1/3 Set Info · Step 2/3 Passages · Step 3/3 Questions

STEP 1:
- Badge read-only "Part 7: Reading Comprehension"
- Text "Set Name" * "Set 05 - Double Passage"
- Segmented control "Number of Passages": [1] [2 active] [3]

STEP 2 — Tabs Passage 1 | Passage 2:
Tab 1: textarea long email text "Dear Mr. Lee, We are writing to confirm your interview..."
Tab 2: image upload zone + preview of job application form PNG
Preview column: two passages stacked like exam reading section

STEP 3 — Questions (show 3 of 5):
Repeater + "Add Question" (max 5)
Per question:
- Number 165
- Textarea "What is the purpose of the email?"
- 4 options A-D
- Correct B
- Dropdown "Linked Passage": Whole Set | Passage 1 | Passage 2

Buttons: Back | primary "Save Reading Set"
POST reading-sets. All visible UI text in English. MD3 AcadPrep admin.
No Part picker on this wizard.
```

---

## S11 — Delete Confirm Modal

**API:** `DELETE /api/exams/{examId}/content/reading-sets/{groupId}` (variants)

**Screen name:** `S11 - Delete Content Block Confirm Modal`

```
Design S11 — Danger confirm modal on Exam Edit page.

Red warning icon, title "Confirm Delete Reading Set?"
Body list:
- QuestionGroup: Set 05 - Double passage
- 2 passages will be deleted
- 5 questions (165-169) will be deleted

Warning banner amber: "This exam already has 12 attempts. Type DELETE to confirm."
Text input placeholder "Type DELETE"

Footer: Cancel | destructive red "Confirm Delete"

Match AcadPrep modal style (Create New Exam Modal). All visible UI text in English.
```

---

## Shared component screens (optional)

### QuestionOptionsEditor component

```
Design reusable component "QuestionOptionsEditor" for AcadPrep CMS.
4 rows: circle letter badge A/B/C/D + text input + radio "correct"
Selected correct answer row has primary border highlight.
Show inline error state on empty option.
MD3, compact, for embedding in wizards.
```

### MediaUploader component

```
Design "MediaUploader" component: drag-drop zone, browse button, URL fallback input, file name chip with remove, thumbnail/audio preview.
States: empty, uploading progress bar, success with preview, error.
English labels: "Upload" / "or paste URL"
AcadPrep MD3.
```

---

## Flow diagram (for Stitch project notes)

```
S0 [Part 6: + Add Passage] ──────────────────────► S4 (no S1)
S0 [Part 7: + Add Reading Set] ────────────────► S5 (no S1)
S0 [header: + Add Content] ──► S1 (pick Part) ──► S2/S3/S4/S5
```

---

## Recommended generation order

1. **S0** — hub with per-Part buttons (highest priority)
2. **S3** — Part 5 (simplest direct flow)
3. **S4** — Part 6 direct from accordion
4. **S2** — Listening direct from accordion
5. **S5** — Part 7 direct from accordion
6. **S1** — global Part picker only (lower priority / optional)
7. **S11** — delete modal

---

## After generation

1. Name each screen using the convention above (prefix `S0`, `S1`, ...)
2. Group the screens into a **CMS Question Content** folder in Stitch
3. Export HTML → `backend/src/` (similar to `create.html`, `manage.html`)
4. Implement Razor partials that map fields to the API contract
5. Wire S0 per-Part buttons to navigate directly — only header "+ Add Content" opens S1
