# UC-15.1_View Notifications

## a. Functional Description

* **UC ID and Name:** UC-15.1_View Notifications
* **Created By:** ThinhNQ
* **Date Created:** 03/06/2026
* **Primary Actor:** User, Moderator, Admin
* **Secondary Actors:** None
* **Trigger:** The actor clicks on the bell notification widget icon located within the primary system header bar.
* **Description:** As an authorized user, I want to access my system notifications panel tray so that I can keep track of recent study reminders, alerts, or system updates.

### Preconditions
* **PRE-1:** User authentication status validation tokens are verified and active.

### Postconditions
* **POST-1:** The system panel lists active alert rows, updating badge counters to clear unread counts.

### Normal Flow
1. The actor clicks on the bell layout navigation widget.
2. The application queries notification log rows matching the account ID.
3. The system processes records using a default descending chronological sort order based on timestamp data attributes.
4. The notification list panel displays alerts showing type icons, descriptive text titles, and elapsed time details.
5. Unread notification items display a subtle background highlight color indicator.
6. The actor reads the notification details and can click individual rows to navigate to related system areas.

### Alternative Flows
None

### Exceptions
None

### Priority
Low

### Frequency of Use
Frequently

### Business Rules
BR-14

### Other Information
The notification system runs automated backend cleanup workers every night to manage storage space.

### Assumptions
None

## b. Business Rules

| ID | Business Rule | Business Rule Description |
| :--- | :--- | :--- |
| **BR14** | Notification State and Auto-Purge | Once a system notification transitions to the "Read" state, it will be automatically and permanently purged from the user's inbox after 30 days to optimize storage space. |

---

# UC-15.2_Mark Notification as Read

## a. Functional Description

* **UC ID and Name:** UC-15.2_Mark Notification as Read
* **Created By:** ThinhNQ
* **Date Created:** 03/06/2026
* **Primary Actor:** User, Moderator, Admin
* **Secondary Actors:** None
* **Trigger:** The actor clicks a specific unread notification row item or selects the global "Mark All as Read" action link button widget.
* **Description:** As an authorized user, I want to mark unread notifications as read so that I can keep my inbox cleanly organized and reset indicator badge totals.

### Preconditions
* **PRE-1:** Unread notification records must actively exist in the notification panel view layer.

### Postconditions
* **POST-1:** Notification status attributes update to 'Read' in the database, starting expiration countdowns.

### Normal Flow
1. The actor selects the "Mark as Read" action trigger widget next to an alert row item.
2. The application sends an update query to modify the target notification record status field.
3. The backend changes the status attribute from 'Unread' to 'Read'.
4. The interface updates the row design to remove unread background highlight indicators.
5. The main bell icon's unread badge counter value decreases accordingly.
6. The system records the update timestamp to track the 30-day automatic deletion lifecycle.

### Alternative Flows
**A1: The actor clicks the global "Mark All as Read" link option**
1. The actor triggers the global cleanup link option.
2. The database updates all 'Unread' entries matching the account ID to a 'Read' status via a single batch transaction.
3. The interface updates all list rows simultaneously and removes the unread badge from the bell icon wrapper.

### Exceptions
None

### Priority
Low

### Frequency of Use
Frequently

### Business Rules
BR-14

### Other Information
None

### Assumptions
None

## b. Business Rules

| ID | Business Rule | Business Rule Description |
| :--- | :--- | :--- |
| **BR14** | Notification State and Auto-Purge | Once a system notification transitions to the "Read" state, it will be automatically and permanently purged from the user's inbox after 30 days to optimize storage space. |
