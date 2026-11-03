# TOEIC CMS Question Creation Infrastructure - Implementation Walkthrough

We have successfully implemented the full Question Content Management system in the backend and UI layers for AcadPrep. This walkthrough explains the architectural design, features, and folder layout of the implementation.

---

## 1. Directory Structure

All files have been placed in the appropriate folders to align with the modular architectural guidelines of the project:

### Backend Commands (Application Layer)
Located under: `backend/src/AcadPrep.Application/Features/Admin/Exams/Commands/`
- **`CreatePart5Question/`**: Command, Handler, Validator, and Dto for single question creation.
- **`CreateListeningGroup/`**: Command, Handler, Validator, and Dto for listening group (Part 1-4) media + question bundle creation.
- **`CreateTextCompletionSet/`**: Command, Handler, Validator, and Dto for Part 6 passages + exactly 4 questions creation.
- **`CreateReadingSet/`**: Command, Handler, Validator, and Dto for Part 7 multiple passages (1-3) + questions (2-5) creation.
- **`DeleteExamContent/`**: Unified delete command handler that cleans up media files, cascading options, questions, and passages, guarded against modification on exams with student attempts.

### UI Screens (Razor Pages)
Located under: `backend/src/AcadPrep.WebUI/Pages/Admin/Exams/`
- **`Edit.cshtml` & `Edit.cshtml.cs`**: **S0 - Exam Edit Hub**. Houses the 7 Part accordions, progress indicators, the **S1 - Add Content Part Picker Modal**, and the **S11 - Delete Confirm Modal**.
- **`Questions/`** (Wizard Pages):
  - **`Listening.cshtml` & `Listening.cshtml.cs`**: **S2 - Create Listening Group Wizard (Part 1-4)**. Supports 3 steps: Media setup, Question repeater, and live confirmation with simulated player.
  - **`Part5.cshtml` & `Part5.cshtml.cs`**: **S3 - Create Part 5 Question Form**. Split-screen form with an interactive real-time learner preview panel.
  - **`Part6.cshtml` & `Part6.cshtml.cs`**: **S4 - Create Part 6 Text Completion Set Wizard**. Supports 2 steps: Passage layout text + helper chips to insert blank tokens, and 4 question cards configuration.
  - **`Part7.cshtml` & `Part7.cshtml.cs`**: **S5 - Create Part 7 Reading Set Wizard**. Supports 3 steps: Set Info (1-3 passages count selector), Passage content tabs, and 2-5 questions list with passage linking.

---

## 2. Key Features

### 🌟 Interactive Wow Aesthetics & Live Previews
All wizard pages feature a side-by-side or stacked layout:
- **Part 5**: Typing in the form inputs instantly updates the learner's card. Choosing the correct answer radio updates the highlights in the mock card.
- **Part 6**: Uses JavaScript regular expressions to parse blank indicators like `(131)` and automatically replaces them with a highlighted primary blue token badge `[131]` in the live reader preview.
- **Part 7**: Displays the passages stacked exactly like they will be rendered for the student, updating in real-time as text or media is added.

### 📁 Cloudinary Media Upload Integration
The media upload system is integrated with the `IFileStorageService` (Cloudinary):
- Admin can upload custom MP3 audio files or image files directly via standard drag-and-drop file inputs on the wizard pages.
- The PageModel uploads these files to Cloudinary, extracts the secure URL, and attaches it to the command payload before dispatching the MediatR request.
- Alternative URL fallback fields are retained for flexibility.

### 🛡️ Attempt Guard & Transaction-Safe Operations
- Operates on a **single-transaction model** via `SaveChangesAsync` within command handlers, ensuring that a set of questions (e.g. 4 questions in a Part 6 text completion set) are committed atomically.
- Includes a business guard that prevents deletion of content if the exam has already been attempted by students, preventing database integrity exceptions.

---

## 3. UI Navigation Logic

As specified in the spec:
- Clicking **"+ Thêm nội dung"** in the **global header** of S0 displays the **S1 Part Picker Modal** so the user can choose which Part they want to add.
- Clicking the contextual **"+ Thêm [tên loại]"** button inside a specific **Part accordion** skips the Part Picker modal and navigates the user **directly** to the matching wizard page with pre-populated parameters.
