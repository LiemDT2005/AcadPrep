# Software Requirement Specification (SRS) - Use Case Detail

## 1. Use Case: UC-3.1_View Exams

### a. Functional Description

| Attribute | Details |
| :--- | :--- |
| **UC ID and Name** | UC-3.1 – View Exams *(Note: Referred to as UC-3.1 in the heading)* |
| **Created By** | LiemDT |
| **Date Created** | 29/05/2026 |
| **Primary Actor** | Guest |
| **Secondary Actors** | System |
| **Trigger** | Guest navigates to the exam list page. |
| **Description** | The guest views a list of published TOEIC exams, each showing summary information such as name, year, number of parts, duration, and attempt count. |
| **Preconditions** | The system is running and accessible. |
| **Postconditions** | The exam list is displayed. The guest may select an exam to view its details (UC-2.2). |
| **Priority** | High – Must Have |
| **Frequency of Use** | Very frequent — this is the primary entry point of the system. |
| **Assumptions** | Guest does not need to be logged in to view the exam list. |

#### Normal Flow
1. Guest accesses the exam list page.
2. System queries all published exams from the database (**BR-15**: only 'published' exams are shown; soft-deleted exams are excluded).
3. System displays the exams as cards showing: name, year, parts, duration, and attempt count.
4. Guest browses and scrolls through the list (**BR-13**: leaderboard and aggregate stats are refreshed every 30 minutes).

#### Alternative Flows
* **A1: No published exams exist**
  * System displays an empty state message.

#### Exceptions
* **E1: Server connection error**
  * System displays an error message and prompts the user to try again.

#### Other Information
* Supports pagination or infinite scroll when the number of exams is large.
* **Business Rules Associated:** BR-15, BR-22, BR-23

---

### b. Business Rules

| ID | Business Rule | Business Rule Description |
| :--- | :--- | :--- |
| **BR-15** | Referential Integrity on Exam Deletion | A Moderator or Admin cannot hard-delete an Exam if it is linked to existing user learning logs (Exam Attempts). In this case, the system must enforce a "Soft Delete" (hiding the exam) mechanism instead. |
| **BR-22** | Minimum Search Keyword Length | For free-text searching (excluding dropdown filters), the user must input a minimum of 2 characters to trigger the search query. Inputs of a single character will be ignored to prevent excessive and irrelevant data retrieval. |
| **BR-23** | Filter State Persistence | The state of the currently applied filters must be persisted when the user navigates through pagination pages, or when they click to view a specific question's details and subsequently return to the list view. |
