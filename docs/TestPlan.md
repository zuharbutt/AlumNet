# Alumni Management System — Test Plan Document

**Project:** Alumni Management System
**Course:** CL2005 — Database Systems
**Team:** Zuhar Faisal, Ramsha Khalid, M. Waleed
**Semester:** Spring 2026
**Section:** BSE-4A
**Document Version:** 1.0

---

## 1. Introduction

### 1.1 Purpose
This document defines the testing strategy, test cases, and quality verification workflow for the Alumni Management System. It ensures every feature works as specified in the SRS and that regressions are caught before production.

### 1.2 Scope
This test plan covers:
- Backend API endpoint testing
- Database integrity and constraint testing
- Frontend page behavior testing
- Role-based access control testing
- Input validation testing
- Security testing (unauthorized access, injection attempts)
- End-to-end user flow testing

### 1.3 Related Documents
- **SRS** — Alumni Management System Software Requirements Specification
- **DDD** — Database Design Document (to be created)
- **API Reference** — Endpoint documentation (to be created)

---

## 2. Testing Strategy

The project will use **four layers of testing**, each catching different types of bugs.

### 2.1 Unit Testing
- **What:** Test individual methods/functions in isolation.
- **Where:** Backend services, validators, utilities.
- **Tool:** xUnit (C#), with Moq for mocking dependencies.
- **Coverage Target:** All business logic services, all validators.
- **Example:** Test `PasswordHasher.Hash()` returns a valid hash and `Verify()` correctly matches.

### 2.2 Integration Testing
- **What:** Test API endpoints end-to-end with a real (test) database.
- **Where:** Every controller endpoint.
- **Tool:** xUnit + `WebApplicationFactory` + in-memory or test MSSQL DB.
- **Coverage Target:** Every endpoint must have at least one happy-path and one negative test.
- **Example:** `POST /api/auth/register` with valid data returns 200; with duplicate email returns 400.

### 2.3 Database Testing
- **What:** Test schema constraints, triggers, and stored procedures directly in SQL.
- **Where:** MSSQL Management Studio or automated test scripts.
- **Coverage Target:** Every CHECK constraint, UNIQUE constraint, and FK must be verified.
- **Example:** Attempt to insert `CGPA = 5.00` → must be rejected by CHECK constraint.

### 2.4 Manual UI / Acceptance Testing
- **What:** Test the frontend by actually clicking through pages.
- **Where:** Every user-facing page.
- **Tool:** Manual testing in Chrome/Firefox, documented in a checklist.
- **Coverage Target:** Every page in Section 11 of the SRS must be manually verified.
- **Example:** Log in as student, filter alumni by department, send mentorship request, verify it appears in alumnus inbox.

### 2.5 Security Testing (the "Red Team" Equivalent)
- **What:** Actively try to break access controls and validation.
- **Test examples:**
  - Call admin endpoints with a student's JWT → must return 403.
  - Call endpoints with no JWT → must return 401.
  - Try SQL injection in search fields → must be sanitized.
  - Try XSS payloads in bio fields → must be escaped in display.
  - Try registering as admin without invite code → must fail.
  - Try modifying another user's profile by changing URL parameter → must be blocked.

---

## 3. Testing Workflow for Developer / AI Assistant

**This section is CRITICAL.** These rules must be followed throughout development.

### 3.1 The Golden Rule
> **No feature is "done" until its test cases pass.** Writing code without running tests against it is incomplete work.

### 3.2 The Development Cycle

For every feature being built, the developer / AI assistant must follow this cycle:

```
  1. READ the spec  →  understand what to build
  2. BUILD the code →  implement the feature
  3. WRITE tests    →  create unit + integration tests
  4. RUN tests      →  execute them and show results
  5. FIX failures   →  iterate until all tests pass
  6. REPORT         →  summarize what was built and what was verified
```

### 3.3 Instruction Template for AI Assistant

When starting a new feature with an AI assistant, use this phrasing:

> *"Implement Feature X per SRS Section Y. After implementation:
> 1. Write integration tests covering all test cases listed in Test Plan Section Z.
> 2. Run the tests and show me the results.
> 3. If any test fails, fix the code (not the test) and re-run.
> 4. Only mark the feature complete when all tests pass.
> 5. Report which test cases were covered and which were skipped with reasoning."*

### 3.4 Rules to Follow
1. **Never weaken a test to make it pass.** If a test fails, the code is wrong — not the test.
2. **Never delete failing tests.** They must be fixed or flagged as known issues.
3. **Never skip writing tests to "save time".** Tests are part of the feature.
4. **Always test negative cases.** A feature isn't done if only the happy path works.
5. **Run all existing tests after changes.** Catch regressions immediately.
6. **Show test output in the response.** Don't just claim tests pass — show the output.

### 3.5 Feature Completion Checklist

Before marking a feature as complete, verify:

- [ ] All functional requirements (FR-xxx) from SRS covered
- [ ] Happy path test passes
- [ ] At least 2 negative tests pass (invalid input, unauthorized access)
- [ ] All validation rules tested at boundaries
- [ ] Role-based access tested (403 for wrong role)
- [ ] Database constraints verified
- [ ] Regression tests (existing tests still pass)
- [ ] Frontend manually verified on the corresponding page

---

## 4. Test Case Template

Every test case in this document follows this format:

```
TC-[MODULE]-[NUMBER]: [Short Descriptive Title]
- Feature:        [Feature name]
- Type:           [Happy Path / Edge Case / Negative / Security]
- Priority:       [Critical / High / Medium / Low]
- FR Reference:   [SRS requirement ID, e.g. FR-101]
- Preconditions:  [Required state before test]
- Test Steps:     [Numbered list of actions]
- Expected Result:[What should happen]
- Automated:      [Yes - Unit/Integration, No - Manual]
```

**Priority Levels:**
- **Critical** — Feature doesn't work without it (auth, core CRUD)
- **High** — Important but not blocking (filters, validation)
- **Medium** — Nice-to-have (UI polish, edge cases)
- **Low** — Rarely hit scenarios

---

## 5. Test Cases by Feature

### 5.1 Authentication Module (AUTH)

---

**TC-AUTH-001: Valid Student Registration**
- **Feature:** User Registration
- **Type:** Happy Path
- **Priority:** Critical
- **FR Reference:** FR-101
- **Preconditions:** User is on `/register.html`; email `student1@test.com` is not in DB.
- **Test Steps:**
  1. Enter Full Name: "Ali Khan"
  2. Enter Email: "student1@test.com"
  3. Enter Password: "Secure@123"
  4. Enter Confirm Password: "Secure@123"
  5. Select Role: Student
  6. Enter Phone: "+923001234567"
  7. Enter DOB: "2002-01-15"
  8. Click Register
- **Expected Result:** 200 OK; user created; redirected to `/login.html`; `Users` table has new row; `StudentProfiles` has linked row.
- **Automated:** Yes (Integration Test)

---

**TC-AUTH-002: Valid Alumni Registration**
- **Feature:** User Registration
- **Type:** Happy Path
- **Priority:** Critical
- **FR Reference:** FR-101
- **Preconditions:** User is on `/register.html`; email is unique.
- **Test Steps:** Same as TC-AUTH-001 but Role = Alumni.
- **Expected Result:** User created with Role = Alumni; empty `AlumniProfiles` row linked.
- **Automated:** Yes

---

**TC-AUTH-003: Valid Admin Registration with Invite Code**
- **Feature:** User Registration
- **Type:** Happy Path
- **Priority:** Critical
- **FR Reference:** FR-102
- **Preconditions:** A valid, unused invite code exists in `AdminInviteCodes`.
- **Test Steps:**
  1. Fill all required fields.
  2. Select Role: Admin.
  3. Enter valid Invite Code.
  4. Click Register.
- **Expected Result:** User created with Role = Admin; invite code marked `IsUsed = 1`; `UsedByUserId` populated.
- **Automated:** Yes

---

**TC-AUTH-004: Admin Registration with Invalid Invite Code**
- **Feature:** User Registration
- **Type:** Negative
- **Priority:** Critical
- **FR Reference:** FR-102
- **Preconditions:** Invite code "FAKE123" does not exist.
- **Test Steps:**
  1. Select Role: Admin.
  2. Enter Code: "FAKE123".
  3. Fill other fields, click Register.
- **Expected Result:** 400 Bad Request; error message "Invalid or expired invite code"; no user created.
- **Automated:** Yes

---

**TC-AUTH-005: Admin Registration with Already-Used Invite Code**
- **Feature:** User Registration
- **Type:** Negative
- **Priority:** Critical
- **FR Reference:** FR-102
- **Preconditions:** Invite code exists with `IsUsed = 1`.
- **Test Steps:** Attempt admin registration with that code.
- **Expected Result:** 400 Bad Request; "Invite code already used"; no user created.
- **Automated:** Yes

---

**TC-AUTH-006: Registration with Duplicate Email**
- **Feature:** User Registration
- **Type:** Negative
- **Priority:** Critical
- **FR Reference:** FR-101
- **Preconditions:** Email "existing@test.com" already registered.
- **Test Steps:** Attempt to register with that email.
- **Expected Result:** 400 Bad Request; "Email already exists"; no row inserted.
- **Automated:** Yes

---

**TC-AUTH-007: Registration with Weak Password**
- **Feature:** Registration Validation
- **Type:** Negative (Validation)
- **Priority:** High
- **FR Reference:** FR-106
- **Preconditions:** User on register page.
- **Test Steps:**
  1. Enter password "abc123" (no uppercase, no special char).
  2. Submit.
- **Expected Result:** 400 Bad Request; error message listing password requirements; no user created.
- **Automated:** Yes
- **Boundary variations to also test:**
  - 7 characters (below minimum)
  - No uppercase
  - No digit
  - No special character

---

**TC-AUTH-008: Registration with Mismatched Passwords**
- **Feature:** Registration Validation
- **Type:** Negative
- **Priority:** High
- **FR Reference:** FR-101
- **Test Steps:** Enter Password "Secure@123" and Confirm "Secure@124".
- **Expected Result:** Frontend validation error before API call; form blocked.
- **Automated:** Yes (UI-level validation test)

---

**TC-AUTH-009: Successful Login**
- **Feature:** User Login
- **Type:** Happy Path
- **Priority:** Critical
- **FR Reference:** FR-103, FR-104
- **Preconditions:** Registered user exists.
- **Test Steps:**
  1. Enter correct email and password.
  2. Click Login.
- **Expected Result:** 200 OK; response contains JWT token; token stored in localStorage; redirected to role-appropriate dashboard.
- **Automated:** Yes

---

**TC-AUTH-010: Login with Wrong Password**
- **Feature:** User Login
- **Type:** Negative
- **Priority:** Critical
- **FR Reference:** FR-103
- **Test Steps:** Login with correct email, wrong password.
- **Expected Result:** 401 Unauthorized; error "Invalid credentials"; no token returned; `FailedAttempts` incremented.
- **Automated:** Yes

---

**TC-AUTH-011: Login with Non-Existent Email**
- **Feature:** User Login
- **Type:** Negative
- **Priority:** High
- **FR Reference:** FR-103
- **Test Steps:** Login with email not in DB.
- **Expected Result:** 401 Unauthorized; same generic "Invalid credentials" message (don't reveal which is wrong).
- **Automated:** Yes

---

**TC-AUTH-012: Account Lockout After 5 Failed Attempts**
- **Feature:** Login Security
- **Type:** Security
- **Priority:** High
- **FR Reference:** FR-105
- **Test Steps:**
  1. Attempt login with wrong password 5 times within 15 minutes.
  2. Attempt 6th login with correct password.
- **Expected Result:** 6th attempt rejected with "Account temporarily locked"; `LockedUntil` timestamp set in DB.
- **Automated:** Yes

---

**TC-AUTH-013: Accessing Protected Endpoint Without Token**
- **Feature:** JWT Authorization
- **Type:** Security
- **Priority:** Critical
- **FR Reference:** NFR-05
- **Test Steps:** Call `GET /api/alumni` without `Authorization` header.
- **Expected Result:** 401 Unauthorized.
- **Automated:** Yes

---

**TC-AUTH-014: Accessing Protected Endpoint with Expired Token**
- **Feature:** JWT Authorization
- **Type:** Security
- **Priority:** Critical
- **FR Reference:** FR-104
- **Test Steps:** Call endpoint with JWT issued 25+ hours ago.
- **Expected Result:** 401 Unauthorized; "Token expired".
- **Automated:** Yes

---

**TC-AUTH-015: Password Stored as Hash, Not Plaintext**
- **Feature:** Password Security
- **Type:** Security
- **Priority:** Critical
- **FR Reference:** FR-106
- **Test Steps:** Register user, then query DB: `SELECT PasswordHash FROM Users WHERE Email = 'x@test.com'`.
- **Expected Result:** Value is a BCrypt hash (starts with `$2a$` or similar), not the plaintext password.
- **Automated:** Yes (DB-level test)

---

### 5.2 Profile Management Module (PROF)

---

**TC-PROF-001: Alumni Completes Profile (Happy Path)**
- **Feature:** Alumni Profile Edit
- **Type:** Happy Path
- **Priority:** Critical
- **FR Reference:** FR-201
- **Preconditions:** Logged in as Alumni.
- **Test Steps:** Fill all fields (grad year 2015, department, degree, company, title, industry, LinkedIn URL, bio) → Save.
- **Expected Result:** 200 OK; `AlumniProfiles` row updated.
- **Automated:** Yes

---

**TC-PROF-002: Student Completes Profile (Happy Path)**
- **Feature:** Student Profile Edit
- **Type:** Happy Path
- **Priority:** Critical
- **FR Reference:** FR-201
- **Preconditions:** Logged in as Student.
- **Test Steps:** Enrollment year 2022, expected grad 2026, CGPA 3.50, semester 5, department, degree.
- **Expected Result:** Saved successfully.
- **Automated:** Yes

---

**TC-PROF-003: Alumni Graduation Year in the Future**
- **Feature:** Profile Validation
- **Type:** Negative
- **Priority:** High
- **FR Reference:** FR-202
- **Test Steps:** Try to save with grad year 2030.
- **Expected Result:** 400 Bad Request; "Graduation year must be between 1950 and current year".
- **Automated:** Yes

---

**TC-PROF-004: Student CGPA Out of Range**
- **Feature:** Profile Validation
- **Type:** Negative
- **Priority:** High
- **FR Reference:** FR-203
- **Test Steps:** Enter CGPA = 5.00. Save.
- **Expected Result:** Validation error: "CGPA must be between 0.00 and 4.00".
- **Automated:** Yes

---

**TC-PROF-005: Student Expected Grad Year Before Enrollment**
- **Feature:** Profile Validation
- **Type:** Negative
- **Priority:** High
- **FR Reference:** FR-204
- **Test Steps:** Enrollment 2024, Expected Grad 2022. Save.
- **Expected Result:** 400 Bad Request; "Expected graduation year must be after enrollment year".
- **Automated:** Yes

---

**TC-PROF-006: User Tries to Edit Another User's Profile**
- **Feature:** Profile Security
- **Type:** Security
- **Priority:** Critical
- **FR Reference:** FR-201
- **Test Steps:** Logged in as UserID=5. Call `PUT /api/alumni/8` to modify UserID=8's profile.
- **Expected Result:** 403 Forbidden.
- **Automated:** Yes

---

**TC-PROF-007: Student Role Tries to Edit Alumni Profile**
- **Feature:** Profile Security
- **Type:** Security
- **Priority:** Critical
- **Test Steps:** Logged in as Student. Call `PUT /api/alumni/me`.
- **Expected Result:** 403 Forbidden.
- **Automated:** Yes

---

### 5.3 Alumni Directory Module (DIR)

---

**TC-DIR-001: Student Views Full Directory**
- **Feature:** Alumni Directory
- **Type:** Happy Path
- **Priority:** Critical
- **FR Reference:** FR-301
- **Preconditions:** Logged in as Student; DB has multiple alumni.
- **Test Steps:** Open `/student/alumni-directory.html`.
- **Expected Result:** Page shows paginated list of alumni (10 or 20 per page).
- **Automated:** Yes

---

**TC-DIR-002: Filter by Department**
- **Feature:** Directory Filtering
- **Type:** Happy Path
- **Priority:** High
- **FR Reference:** FR-302
- **Test Steps:** Select Department "Computer Science" → Apply.
- **Expected Result:** Only alumni from CS department shown.
- **Automated:** Yes

---

**TC-DIR-003: Filter by Multiple Criteria**
- **Feature:** Directory Filtering
- **Type:** Happy Path
- **Priority:** High
- **FR Reference:** FR-302
- **Test Steps:** Department = CS, Grad Year 2015–2020, Industry = "Software".
- **Expected Result:** Only alumni matching ALL criteria shown.
- **Automated:** Yes

---

**TC-DIR-004: Keyword Search Returns Matching Names**
- **Feature:** Directory Search
- **Type:** Happy Path
- **Priority:** High
- **FR Reference:** FR-302
- **Test Steps:** Search keyword "Ali".
- **Expected Result:** Alumni with "Ali" in name OR company shown.
- **Automated:** Yes

---

**TC-DIR-005: No Results Found**
- **Feature:** Directory Search
- **Type:** Edge Case
- **Priority:** Medium
- **Test Steps:** Filter with combination that matches no alumni.
- **Expected Result:** Page displays "No alumni match your filters" message; no error.
- **Automated:** Yes

---

**TC-DIR-006: Alumni Contact Info Hidden by Default**
- **Feature:** Directory Privacy
- **Type:** Security
- **Priority:** Critical
- **FR Reference:** FR-304
- **Test Steps:** Student views alumni detail page without accepted mentorship.
- **Expected Result:** Email and Phone fields not shown or show "Connect to view".
- **Automated:** Yes

---

**TC-DIR-007: Alumni Cannot Access Other Alumni Directory**
- **Feature:** Directory Access Control
- **Type:** Security
- **Priority:** Critical
- **FR Reference:** FR-305
- **Test Steps:** Logged in as Alumni. Call `GET /api/alumni`.
- **Expected Result:** 403 Forbidden OR only return self.
- **Automated:** Yes

---

**TC-DIR-008: SQL Injection in Search Field**
- **Feature:** Directory Security
- **Type:** Security
- **Priority:** Critical
- **FR Reference:** NFR-06
- **Test Steps:** Enter search keyword: `'; DROP TABLE Users;--`
- **Expected Result:** Request succeeds harmlessly; no SQL error; `Users` table intact.
- **Automated:** Yes

---

### 5.4 Mentorship Module (MENT)

---

**TC-MENT-001: Student Sends Valid Mentorship Request**
- **Feature:** Send Request
- **Type:** Happy Path
- **Priority:** Critical
- **FR Reference:** FR-401
- **Preconditions:** Student logged in; target alumnus exists.
- **Test Steps:** Navigate to alumnus → click Request Mentorship → enter 50-char message → submit.
- **Expected Result:** 200 OK; request created with Status = Pending; shown in alumnus's inbox.
- **Automated:** Yes

---

**TC-MENT-002: Alumnus Accepts Request**
- **Feature:** Respond to Request
- **Type:** Happy Path
- **Priority:** Critical
- **FR Reference:** FR-404
- **Preconditions:** Pending request exists for this alumnus.
- **Test Steps:** Open inbox → click Accept → confirm.
- **Expected Result:** Status = Accepted; RespondedAt timestamp set; student sees status change.
- **Automated:** Yes

---

**TC-MENT-003: Alumnus Rejects Request**
- **Feature:** Respond to Request
- **Type:** Happy Path
- **Priority:** High
- **FR Reference:** FR-404
- **Test Steps:** Open Pending request → click Reject.
- **Expected Result:** Status = Rejected; student notified.
- **Automated:** Yes

---

**TC-MENT-004: Student Cannot Send Duplicate Pending Request**
- **Feature:** Duplicate Prevention
- **Type:** Negative
- **Priority:** Critical
- **FR Reference:** FR-402
- **Preconditions:** Student already has Pending request to this alumnus.
- **Test Steps:** Attempt to send another request to same alumnus.
- **Expected Result:** 400 Bad Request; "You already have a pending request with this alumnus".
- **Automated:** Yes

---

**TC-MENT-005: Message Too Short (< 20 chars)**
- **Feature:** Request Validation
- **Type:** Negative
- **Priority:** High
- **FR Reference:** FR-403
- **Test Steps:** Send request with message "Hi." (3 chars).
- **Expected Result:** 400 Bad Request; "Message must be at least 20 characters".
- **Automated:** Yes

---

**TC-MENT-006: Message Too Long (> 500 chars)**
- **Feature:** Request Validation
- **Type:** Edge Case
- **Priority:** Medium
- **FR Reference:** FR-403
- **Test Steps:** Send 501-character message.
- **Expected Result:** 400 Bad Request; "Message cannot exceed 500 characters".
- **Automated:** Yes

---

**TC-MENT-007: Boundary — Exactly 20 Characters**
- **Feature:** Request Validation
- **Type:** Edge Case
- **Priority:** Medium
- **Test Steps:** Send message exactly 20 chars.
- **Expected Result:** Accepted (boundary inclusive).
- **Automated:** Yes

---

**TC-MENT-008: Alumnus Tries to Respond to Someone Else's Request**
- **Feature:** Mentorship Security
- **Type:** Security
- **Priority:** Critical
- **FR Reference:** FR-404
- **Test Steps:** Alumnus A tries to accept a request addressed to Alumnus B via `PUT /api/mentorship/{id}/respond`.
- **Expected Result:** 403 Forbidden.
- **Automated:** Yes

---

**TC-MENT-009: Alumnus Cannot Change Status After Response**
- **Feature:** Status Immutability
- **Type:** Negative
- **Priority:** High
- **FR Reference:** FR-406
- **Test Steps:** Accept a request, then try to change status back to Pending.
- **Expected Result:** 400 Bad Request; "Status cannot be changed once responded".
- **Automated:** Yes

---

**TC-MENT-010: Alumnus Role Tries to Send Mentorship Request**
- **Feature:** Role Restriction
- **Type:** Security
- **Priority:** High
- **FR Reference:** FR-401
- **Test Steps:** Alumnus tries to `POST /api/mentorship/request`.
- **Expected Result:** 403 Forbidden (only students can send).
- **Automated:** Yes

---

**TC-MENT-011: Contact Info Revealed After Acceptance**
- **Feature:** Privacy After Connection
- **Type:** Happy Path
- **Priority:** High
- **FR Reference:** FR-304
- **Preconditions:** Mentorship accepted between Student S and Alumnus A.
- **Test Steps:** Student S views Alumnus A's profile.
- **Expected Result:** Email and phone now visible.
- **Automated:** Yes

---

### 5.5 Events Module (EVT)

---

**TC-EVT-001: Admin Creates Valid Event**
- **Feature:** Event Creation
- **Type:** Happy Path
- **Priority:** Critical
- **FR Reference:** FR-501, FR-502
- **Preconditions:** Logged in as Admin.
- **Test Steps:** Fill title, description, category, future date (+7 days), location, capacity 100, price 500 → Save.
- **Expected Result:** 200 OK; event created with Status = Scheduled.
- **Automated:** Yes

---

**TC-EVT-002: Free Event Creation (Price = 0)**
- **Feature:** Event Creation
- **Type:** Edge Case
- **Priority:** High
- **FR Reference:** FR-504
- **Test Steps:** Create event with TicketPrice = 0.
- **Expected Result:** Event created; no payment flow required for registration.
- **Automated:** Yes

---

**TC-EVT-003: Create Event with Past Date**
- **Feature:** Event Validation
- **Type:** Negative
- **Priority:** Critical
- **FR Reference:** FR-502
- **Test Steps:** Try to create event with date = yesterday.
- **Expected Result:** 400 Bad Request; "Event date must be in the future".
- **Automated:** Yes

---

**TC-EVT-004: Create Event with Zero Capacity**
- **Feature:** Event Validation
- **Type:** Negative
- **Priority:** High
- **FR Reference:** FR-503
- **Test Steps:** Create event with capacity = 0.
- **Expected Result:** 400 Bad Request; "Capacity must be at least 1".
- **Automated:** Yes

---

**TC-EVT-005: Create Event with Negative Price**
- **Feature:** Event Validation
- **Type:** Negative
- **Priority:** High
- **Test Steps:** Create event with price = -100.
- **Expected Result:** 400 Bad Request; "Price cannot be negative".
- **Automated:** Yes

---

**TC-EVT-006: Non-Admin Tries to Create Event**
- **Feature:** Event Security
- **Type:** Security
- **Priority:** Critical
- **FR Reference:** FR-501
- **Test Steps:** Logged in as Student → `POST /api/events`.
- **Expected Result:** 403 Forbidden.
- **Automated:** Yes

---

**TC-EVT-007: Admin Edits Event Before Start Date**
- **Feature:** Event Edit
- **Type:** Happy Path
- **Priority:** High
- **FR Reference:** FR-501
- **Test Steps:** Open event, change description, save.
- **Expected Result:** Event updated; `UpdatedAt` timestamp set.
- **Automated:** Yes

---

**TC-EVT-008: Admin Cancels Event**
- **Feature:** Event Cancellation
- **Type:** Happy Path
- **Priority:** High
- **FR Reference:** FR-506
- **Test Steps:** Open event with registered attendees → click Cancel → confirm.
- **Expected Result:** Status = Cancelled; attendees' registrations marked cancelled.
- **Automated:** Yes

---

**TC-EVT-009: Admin Tries to Delete Event with Registrations**
- **Feature:** Event Deletion Protection
- **Type:** Negative
- **Priority:** High
- **FR Reference:** FR-505
- **Test Steps:** Attempt `DELETE /api/events/{id}` where registrations exist.
- **Expected Result:** 400 Bad Request; "Cannot delete event with registrations. Cancel it instead."
- **Automated:** Yes

---

### 5.6 Event Registration & Tickets Module (TKT)

---

**TC-TKT-001: User Registers for Free Event**
- **Feature:** Event Registration
- **Type:** Happy Path
- **Priority:** Critical
- **FR Reference:** FR-601, FR-604
- **Preconditions:** Free event with available seats.
- **Test Steps:** Click Register on event page.
- **Expected Result:** 200 OK; ticket issued with unique code, PaymentStatus = Paid, Price = 0.
- **Automated:** Yes

---

**TC-TKT-002: User Registers for Paid Event (Mocked Payment)**
- **Feature:** Mock Payment Flow
- **Type:** Happy Path
- **Priority:** Critical
- **FR Reference:** FR-605
- **Test Steps:**
  1. Register for paid event.
  2. Mock payment page appears.
  3. Click "Pay Now".
- **Expected Result:** Ticket created with PaymentStatus = Paid; no real payment processed.
- **Automated:** Yes

---

**TC-TKT-003: Duplicate Registration Prevented**
- **Feature:** Registration Validation
- **Type:** Negative
- **Priority:** Critical
- **FR Reference:** FR-602
- **Preconditions:** User already registered for event.
- **Test Steps:** Attempt to register again.
- **Expected Result:** 400 Bad Request; "Already registered"; no duplicate ticket.
- **Automated:** Yes

---

**TC-TKT-004: Registration When Event is Full**
- **Feature:** Capacity Enforcement
- **Type:** Negative
- **Priority:** Critical
- **FR Reference:** FR-603
- **Preconditions:** Event capacity = 2, already 2 registered.
- **Test Steps:** 3rd user tries to register.
- **Expected Result:** 400 Bad Request; "Event is full"; no ticket issued.
- **Automated:** Yes

---

**TC-TKT-005: Registration After Event Date**
- **Feature:** Registration Window
- **Type:** Negative
- **Priority:** High
- **FR Reference:** FR-603
- **Preconditions:** Event date is yesterday.
- **Test Steps:** Attempt to register.
- **Expected Result:** 400 Bad Request; "Registration closed".
- **Automated:** Yes

---

**TC-TKT-006: Cancel Registration >24 Hours Before Event (Full Refund)**
- **Feature:** Cancellation & Refund
- **Type:** Happy Path
- **Priority:** High
- **FR Reference:** FR-606, FR-607
- **Preconditions:** Paid ticket; event is 48 hours away.
- **Test Steps:** Click Cancel Registration.
- **Expected Result:** Ticket status = Refunded; full refund noted.
- **Automated:** Yes

---

**TC-TKT-007: Cancel Registration <24 Hours Before Event (No Refund)**
- **Feature:** Cancellation & Refund
- **Type:** Edge Case
- **Priority:** High
- **FR Reference:** FR-607
- **Preconditions:** Event is 12 hours away.
- **Test Steps:** Cancel.
- **Expected Result:** Ticket status = Refunded flag false; user notified no refund.
- **Automated:** Yes

---

**TC-TKT-008: Ticket Code is Unique**
- **Feature:** Ticket Uniqueness
- **Type:** Edge Case
- **Priority:** High
- **FR Reference:** FR-604
- **Test Steps:** Register 100 users; query all ticket codes.
- **Expected Result:** All 100 codes are unique; no collision.
- **Automated:** Yes (DB-level test)

---

**TC-TKT-009: User Views Only Own Tickets**
- **Feature:** Ticket Privacy
- **Type:** Security
- **Priority:** Critical
- **Test Steps:** User A calls `GET /api/tickets/mine` → should only see their tickets.
- **Expected Result:** Response contains only User A's tickets, not User B's.
- **Automated:** Yes

---

**TC-TKT-010: Admin Marks Attendance**
- **Feature:** Attendance Tracking
- **Type:** Happy Path
- **Priority:** Medium
- **Test Steps:** Admin opens event attendees page → marks 5 as Attended → Save.
- **Expected Result:** `Attended = 1` in DB for those registrations.
- **Automated:** Yes

---

### 5.7 Donations Module (DON)

---

**TC-DON-001: Alumnus Makes Valid Donation**
- **Feature:** Donation
- **Type:** Happy Path
- **Priority:** Critical
- **FR Reference:** FR-701, FR-705, FR-706
- **Preconditions:** Alumnus logged in; active campaign exists.
- **Test Steps:**
  1. Open Donations page.
  2. Select campaign.
  3. Enter amount 5000.
  4. Click Donate → Confirm.
- **Expected Result:** Donation created (Status = Completed); campaign's `RaisedAmount` increased by 5000.
- **Automated:** Yes

---

**TC-DON-002: Student Tries to Donate**
- **Feature:** Donation Role Restriction
- **Type:** Security
- **Priority:** Critical
- **FR Reference:** FR-701
- **Test Steps:** Logged in as Student → `POST /api/donations`.
- **Expected Result:** 403 Forbidden.
- **Automated:** Yes

---

**TC-DON-003: Donation with Zero Amount**
- **Feature:** Donation Validation
- **Type:** Negative
- **Priority:** High
- **FR Reference:** FR-703
- **Test Steps:** Enter amount = 0.
- **Expected Result:** 400 Bad Request; "Amount must be greater than 0".
- **Automated:** Yes

---

**TC-DON-004: Donation with Negative Amount**
- **Feature:** Donation Validation
- **Type:** Negative
- **Priority:** Critical
- **FR Reference:** FR-703
- **Test Steps:** Enter amount = -1000.
- **Expected Result:** 400 Bad Request; validation error.
- **Automated:** Yes

---

**TC-DON-005: Donation Exceeds Maximum Limit**
- **Feature:** Donation Validation
- **Type:** Edge Case
- **Priority:** Medium
- **FR Reference:** FR-703
- **Test Steps:** Enter amount = 1,000,001.
- **Expected Result:** 400 Bad Request; "Amount exceeds maximum of 1,000,000".
- **Automated:** Yes

---

**TC-DON-006: Donation to Expired Campaign**
- **Feature:** Campaign Status Check
- **Type:** Negative
- **Priority:** Critical
- **FR Reference:** FR-704
- **Preconditions:** Campaign with EndDate in the past.
- **Test Steps:** Attempt donation.
- **Expected Result:** 400 Bad Request; "Campaign has ended".
- **Automated:** Yes

---

**TC-DON-007: Donation to Cancelled Campaign**
- **Feature:** Campaign Status Check
- **Type:** Negative
- **Priority:** High
- **FR Reference:** FR-704
- **Preconditions:** Campaign Status = Cancelled.
- **Test Steps:** Attempt donation.
- **Expected Result:** 400 Bad Request; "Campaign not accepting donations".
- **Automated:** Yes

---

**TC-DON-008: Alumnus Views Only Own Donation History**
- **Feature:** Donation Privacy
- **Type:** Security
- **Priority:** Critical
- **FR Reference:** FR-707
- **Test Steps:** Alumnus A calls `GET /api/donations/mine`.
- **Expected Result:** Only A's donations returned.
- **Automated:** Yes

---

**TC-DON-009: Admin Sees All Donations**
- **Feature:** Admin Oversight
- **Type:** Happy Path
- **Priority:** High
- **FR Reference:** FR-707
- **Test Steps:** Admin calls `GET /api/admin/donations`.
- **Expected Result:** All donations across all campaigns returned.
- **Automated:** Yes

---

**TC-DON-010: Campaign RaisedAmount Updates Correctly After Multiple Donations**
- **Feature:** Campaign Progress
- **Type:** Integration
- **Priority:** Critical
- **FR Reference:** FR-706
- **Test Steps:** 3 alumni donate 1000, 2500, 500 to same campaign.
- **Expected Result:** RaisedAmount = initial + 4000.
- **Automated:** Yes

---

**TC-DON-011: Admin Creates Campaign**
- **Feature:** Campaign Creation
- **Type:** Happy Path
- **Priority:** Critical
- **FR Reference:** FR-702
- **Test Steps:** Admin fills campaign form with valid dates and target.
- **Expected Result:** Campaign created with Status = Active.
- **Automated:** Yes

---

**TC-DON-012: Non-Admin Tries to Create Campaign**
- **Feature:** Campaign Security
- **Type:** Security
- **Priority:** Critical
- **FR Reference:** FR-702
- **Test Steps:** Alumnus tries `POST /api/campaigns`.
- **Expected Result:** 403 Forbidden.
- **Automated:** Yes

---

### 5.8 Dashboard & Reports Module (DASH)

---

**TC-DASH-001: Admin Dashboard Shows Correct Counts**
- **Feature:** Admin Dashboard
- **Type:** Happy Path
- **Priority:** Critical
- **FR Reference:** FR-802
- **Preconditions:** DB has 50 alumni, 100 students, 10 events, 3 campaigns.
- **Test Steps:** Admin opens `/admin/dashboard.html`.
- **Expected Result:** Cards display: Alumni=50, Students=100, Events=10, Active Campaigns=3.
- **Automated:** Yes

---

**TC-DASH-002: Student Dashboard Shows Personal Data Only**
- **Feature:** Student Dashboard
- **Type:** Security + Happy Path
- **Priority:** Critical
- **FR Reference:** FR-804
- **Test Steps:** Student with 2 pending requests, 1 registered event opens dashboard.
- **Expected Result:** Cards show 2 pending, 1 upcoming; no system-wide totals.
- **Automated:** Yes

---

**TC-DASH-003: Alumni Dashboard Shows Personal Data Only**
- **Feature:** Alumni Dashboard
- **Type:** Security + Happy Path
- **Priority:** Critical
- **FR Reference:** FR-803
- **Test Steps:** Alumnus with 3 incoming requests, 2 active mentees, total donated 15000.
- **Expected Result:** Shows exactly those numbers.
- **Automated:** Yes

---

**TC-DASH-004: Mentorship Report Aggregation**
- **Feature:** Report Generation
- **Type:** Happy Path
- **Priority:** High
- **FR Reference:** FR-805
- **Preconditions:** 10 Pending, 15 Accepted, 5 Rejected requests.
- **Test Steps:** Admin views mentorship report.
- **Expected Result:** Counts correctly grouped: Pending=10, Accepted=15, Rejected=5.
- **Automated:** Yes

---

**TC-DASH-005: Student Cannot Access Admin Dashboard**
- **Feature:** Dashboard Security
- **Type:** Security
- **Priority:** Critical
- **Test Steps:** Student navigates to `/admin/dashboard.html`.
- **Expected Result:** Redirected to `/unauthorized.html`; API returns 403.
- **Automated:** Yes

---

### 5.9 Admin Invite Code Module (INV)

---

**TC-INV-001: Admin Generates New Invite Code**
- **Feature:** Invite Code Generation
- **Type:** Happy Path
- **Priority:** High
- **Test Steps:** Admin clicks "Generate Code" with 30-day expiry.
- **Expected Result:** New code created; `IsUsed = 0`; `ExpiryDate = today + 30 days`; `CreatedByUserId` set.
- **Automated:** Yes

---

**TC-INV-002: Non-Admin Tries to Generate Invite Code**
- **Feature:** Invite Code Security
- **Type:** Security
- **Priority:** Critical
- **Test Steps:** Student/Alumnus calls `POST /api/admin/invite-codes`.
- **Expected Result:** 403 Forbidden.
- **Automated:** Yes

---

**TC-INV-003: Expired Invite Code Cannot Be Used**
- **Feature:** Invite Code Expiry
- **Type:** Negative
- **Priority:** High
- **Preconditions:** Code with `ExpiryDate` in the past.
- **Test Steps:** New user tries to register as admin with that code.
- **Expected Result:** 400 Bad Request; "Invite code expired".
- **Automated:** Yes

---

**TC-INV-004: Invite Code is Single-Use**
- **Feature:** Invite Code Uniqueness
- **Type:** Negative
- **Priority:** Critical
- **Test Steps:** First user registers with code successfully; second user tries same code.
- **Expected Result:** First succeeds; second gets 400 "Code already used".
- **Automated:** Yes

---

## 6. End-to-End Integration Test Scenarios

These are full user-journey tests that combine multiple features.

---

**TC-E2E-001: Full Student Mentorship Journey**
- **Scenario:** Student registers, completes profile, finds alumnus, sends request, alumnus accepts, student views contact.
- **Steps:**
  1. Register new student.
  2. Login.
  3. Complete profile.
  4. Open alumni directory, filter by CS department.
  5. Open first alumnus profile.
  6. Send mentorship request with valid message.
  7. Logout.
  8. Login as that alumnus.
  9. Open inbox, accept request.
  10. Logout.
  11. Login as student, verify status = Accepted and contact info visible.
- **Expected Result:** Every step succeeds; DB state reflects all changes correctly.

---

**TC-E2E-002: Full Event Flow**
- **Scenario:** Admin creates event, user registers, pays (mocked), attends, admin marks attendance.
- **Steps:**
  1. Admin creates paid event (capacity 5, price 500).
  2. 5 users register and complete mock payment.
  3. 6th user tries — gets "Event full".
  4. Admin opens attendees page.
  5. Marks 4 as Attended.
  6. Query DB: 4 records with Attended=1, 1 with Attended=0.
- **Expected Result:** All counts correct; no double bookings; tickets all have unique codes.

---

**TC-E2E-003: Donation Campaign Lifecycle**
- **Scenario:** Admin creates campaign, multiple alumni donate, campaign reaches target, admin closes.
- **Steps:**
  1. Admin creates campaign with target 10,000.
  2. Alumni donate 2000, 3000, 5000.
  3. Verify `RaisedAmount` = 10,000.
  4. Admin closes campaign (Status = Closed).
  5. Alumnus tries to donate — rejected.
- **Expected Result:** Accurate tracking; closed campaign blocks donations.

---

**TC-E2E-004: Admin Invite Code Full Cycle**
- **Scenario:** First admin generates code, second admin registers with it, second admin generates code for third.
- **Steps:**
  1. Seed first admin in DB.
  2. First admin logs in, generates code INV-001.
  3. New user registers as admin with INV-001.
  4. New admin logs in, generates INV-002.
  5. Third admin registers with INV-002.
- **Expected Result:** Chain of admin creation works; invite codes properly tracked.

---

## 7. Acceptance Criteria per Feature

A feature is considered **DONE** only when **all** these criteria are met.

### Feature: Authentication
- [ ] User can register as Student, Alumni, or Admin (with code)
- [ ] User can login and receive JWT
- [ ] JWT is validated on every protected endpoint
- [ ] Password is hashed in DB
- [ ] All 15 AUTH test cases pass
- [ ] Frontend login/register pages match UI spec

### Feature: Profile Management
- [ ] Alumni can edit own profile; not others'
- [ ] Student can edit own profile; not others'
- [ ] All validation rules enforced
- [ ] All 7 PROF test cases pass
- [ ] Frontend profile pages functional

### Feature: Alumni Directory
- [ ] Students can browse with all filter options
- [ ] Pagination works
- [ ] Contact info hidden until mentorship accepted
- [ ] Alumni cannot browse other alumni
- [ ] All 8 DIR test cases pass

### Feature: Mentorship
- [ ] Students can send requests
- [ ] Alumni can accept/reject
- [ ] No duplicate pending requests
- [ ] Message validation enforced
- [ ] All 11 MENT test cases pass
- [ ] Status is immutable after response

### Feature: Events
- [ ] Admin can create, edit, cancel
- [ ] Validation enforced (date, capacity, price)
- [ ] Non-admin blocked from create/edit
- [ ] All 9 EVT test cases pass

### Feature: Tickets & Registration
- [ ] Users register for events
- [ ] Unique ticket codes issued
- [ ] Mock payment flow works
- [ ] Duplicate registration blocked
- [ ] Capacity enforced
- [ ] Cancellation + refund logic works
- [ ] All 10 TKT test cases pass

### Feature: Donations
- [ ] Only alumni can donate
- [ ] Admin creates campaigns
- [ ] Amount validation enforced
- [ ] Expired/cancelled campaigns reject donations
- [ ] RaisedAmount updates correctly
- [ ] All 12 DON test cases pass

### Feature: Dashboard & Reports
- [ ] Each role sees appropriate data
- [ ] Cross-role access blocked
- [ ] Aggregations are correct
- [ ] All 5 DASH test cases pass

### Feature: Invite Codes
- [ ] Admin can generate codes
- [ ] Codes are single-use
- [ ] Expired codes rejected
- [ ] Non-admin blocked from generation
- [ ] All 4 INV test cases pass

---

## 8. Bug Reporting Template

When a test fails, use this format to document the bug.

```
BUG-[NUMBER]: [Short Title]
- Severity:        Critical / High / Medium / Low
- Test Case:       TC-XXX-NNN that caught it
- Environment:     Dev / Staging / Prod
- Date Found:      YYYY-MM-DD
- Found By:        [Team member name]
- Steps to Reproduce:
    1. ...
    2. ...
- Expected Behavior:  ...
- Actual Behavior:    ...
- Screenshots/Logs:   [attached]
- Assigned To:        [Team member]
- Status:             Open / In Progress / Fixed / Verified / Closed
```

### Severity Definitions
- **Critical:** System down, data loss, security breach, blocker for other work
- **High:** Major feature broken but workaround exists
- **Medium:** Minor feature broken, cosmetic issues in key flows
- **Low:** Typos, polish issues, rarely hit edge cases

---

## 9. Test Data Requirements

Before running the full test suite, the test database should contain:

### Seed Data
- **Departments:** At least 5 (Computer Science, Electrical Engineering, Mechanical, Business, Economics)
- **First Admin:** Hardcoded email `admin@university.edu`, password hashed
- **Admin Invite Codes:** 2–3 unused codes for testing admin registration

### Test Users
- **Students:** 20+ across multiple departments and years
- **Alumni:** 30+ with varied graduation years (2010–2024), industries, companies, locations
- **Admins:** 2 (beyond the seeded first admin)

### Test Events
- **Upcoming events:** 5 (mix of free and paid, various categories)
- **Past events:** 3 (with some attendees)
- **Cancelled events:** 1

### Test Campaigns
- **Active:** 2
- **Closed:** 1
- **Expired:** 1

### Test Mentorship Requests
- **Pending:** 5
- **Accepted:** 8
- **Rejected:** 3

### Test Donations
- **Completed:** 15 across various campaigns and alumni

---

## 10. Test Execution Schedule

### Phase 1: Unit Tests (During Development)
- Run **on every commit** during development
- Must all pass before proceeding to next feature

### Phase 2: Integration Tests (After Each Module)
- Run after completing each module in the SRS
- Includes all test cases for that module

### Phase 3: Regression Tests (Before Final Submission)
- Run ALL test cases
- No test should fail
- Any bug found must be logged and fixed

### Phase 4: Manual UI Testing (Before Final Submission)
- Walk through every page in Section 11 of SRS
- Verify responsive design on desktop and tablet
- Verify all navigation flows

### Phase 5: Demo Dry Run (Before Presentation)
- Run all E2E scenarios manually
- Time the demo to fit within presentation window
- Prepare fallback test data in case of issues

---

## 11. Summary: Test Coverage Metrics

Target metrics for the final project:

| Metric | Target |
|---|---|
| Unit test coverage (backend) | ≥ 70% of business logic |
| Integration tests per endpoint | ≥ 2 (happy + negative) |
| Total documented test cases | ≥ 70 |
| Critical-priority tests passing | 100% |
| All-priority tests passing | ≥ 95% |
| Manual UI pages verified | 100% of SRS Section 11 |

---

## 12. Revision History

| Version | Date | Author | Changes |
|---|---|---|---|
| 1.0 | April 2026 | Team (Zuhar, Ramsha, Waleed) | Initial Test Plan |

---

## 13. Approval

- **Prepared by:** Zuhar Faisal, Ramsha Khalid, M. Waleed
- **Course:** CL2005 — Database Systems
- **Instructor:** Sir Umar Farooq
- **Semester:** Spring 2026
- **Section:** BSE-4A

---

*End of Test Plan Document*
