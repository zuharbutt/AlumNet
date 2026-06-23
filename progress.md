# Alumni Management System — Progress Tracker

_Last updated: 2026-04-22 — feature/saas-ui-revamp + fix/admin-dashboard-reports: complete_

## Overall Status

| Module | Backend | Frontend | Status |
|--------|---------|----------|--------|
| Database (11 tables, SPs, seed) | ✅ | — | Done |
| Auth (Register/Login/JWT) | ✅ | ✅ | Done |
| Departments Endpoint | ✅ | — | Done |
| Alumni Profile (GET/PUT /api/alumni/me) | ✅ | ✅ | Done |
| Student Profile (GET/PUT /api/students/me) | ✅ | ✅ | Done |
| Alumni Directory (search/filter/paginate) | ✅ | ✅ | Done |
| Alumni Detail page | ✅ | ✅ | Done |
| Mentorship | ✅ | ✅ | Done |
| Events + Tickets | ✅ | ✅ | Done |
| Donations + Campaigns | ✅ | ✅ | Done |
| Admin Dashboard (stats, users, mentorship) | ✅ | ✅ | Done |
| Admin Reports (mentorship log, events, donations) | ✅ | ✅ | Done |
| Admin Invite Codes | ✅ | ✅ | Done |
| Premium SaaS UI (sticky sidebar, all portals) | — | ✅ | Done |
| Notifications | ❌ | ❌ | Not started |

## Current Branch
`develop` (feature/saas-ui-revamp merged)

## UI Architecture (SaaS Revamp — feature/saas-ui-revamp)

All portals now use a **sticky sidebar layout** (Slate Navy `#0f172a` sidebar, Indigo `#6366f1` active highlight):

- `frontend/css/sidebar.css` — full sidebar layout CSS
- `frontend/js/sidebar.js` — `renderSidebar(title)` replaces `renderNavbar()`
- All admin, alumni, student, and shared pages converted to sidebar layout
- Top-right topbar shows FullName + avatar initials
- Responsive: sidebar collapses on mobile (hamburger button)

## Admin Module (fix/admin-dashboard-reports)

### New Backend Endpoints

| Method | Route | Description |
|--------|-------|-------------|
| GET | `/api/admin/users?role=Alumni\|Student&page&pageSize` | All users by role (SRS §11.6) |
| GET | `/api/admin/mentorship?status=&page&pageSize` | All mentorship requests (DDD §6 vw_MentorshipOverview fields) |

### New Frontend Pages

| Page | Description |
|------|-------------|
| `admin/users.html` | Tab switch Alumni/Student, searchable table, pagination |
| `admin/mentorship.html` | Summary strip (sp_GetMentorshipStats) + log table, status filter |
| `admin/reports.html` | Summary cards + Mentorship Log table (date-range filterable) |
| `admin/invite-codes.html` | List and generate admin invite codes |

### Other Changes
- `register.html` — Removed Admin role option and invite code field (registration is Student/Alumni only)

---

## Donations Module (feature/donations)

### Backend Endpoints

| Method | Route | Role | Description |
|--------|-------|------|-------------|
| POST | `/api/campaigns` | Admin | Create campaign (FR-702) |
| PUT | `/api/campaigns/{id}` | Admin | Edit campaign (Active only) |
| PUT | `/api/campaigns/{id}/close` | Admin | Close campaign → stops donations |
| GET | `/api/campaigns` | All Auth | List campaigns (?status=, page, pageSize) |
| GET | `/api/campaigns/{id}` | All Auth | Single campaign with RaisedAmount |
| POST | `/api/donations` | Alumni | Make donation — trigger updates RaisedAmount (FR-701) |
| GET | `/api/donations/mine` | Alumni | Own donation history (FR-707) |
| GET | `/api/admin/donations` | Admin | All donations across campaigns (FR-707) |

### Frontend Pages

| Page | Role | Description |
|------|------|-------------|
| `admin/campaigns.html` | Admin | Table with Active/Closed/Cancelled tabs, progress bars, Edit/Close buttons |
| `admin/campaign-form.html` | Admin | Create & Edit form (?id= for edit mode) |
| `admin/donations.html` | Admin | All donations table with summary stats, campaign filter |
| `shared/donations.html` | All Auth | Bootstrap cards for active campaigns with progress bars, Donate button |
| `shared/donate.html` | Alumni | Mock payment flow, preset amounts (1K/5K/10K/25K/Custom), anonymous checkbox |
| `alumni/my-donations.html` | Alumni | History table + total raised summary card |

### Key Design Decisions
- `tr_Donations_AfterInsert` DB trigger auto-updates `RaisedAmount` — C# never touches it directly
- `vw_ActiveCampaigns` view referenced in DDD, queries use EF Core directly against `DonationCampaigns` table
- Status enum: `Active` / `Closed` / `Cancelled` (from `CampaignStatus` static class)
- Alumni-only restriction on `POST /api/donations` enforced at `[Authorize(Roles = "Alumni")]`
- Student attempts to donate → 403 (TC-DON-002)
- Non-admin attempts to create campaign → 403 (TC-DON-012)

### Files Created / Modified
- `DTOs/Donation/` — CreateCampaignDto, UpdateCampaignDto, CampaignResponseDto, CampaignListResultDto, MakeDonationDto, DonationItemDto, DonationListResultDto
- `Services/CampaignService.cs`
- `Services/DonationService.cs`
- `Controllers/CampaignController.cs`
- `Controllers/DonationController.cs`
- `Controllers/AdminController.cs` — added GET /api/admin/donations
- `Program.cs` — registered CampaignService, DonationService
- `frontend/js/navbar.js` — Donate + My Donations (Alumni); Campaigns + Donations (Admin)

### Build Status
`dotnet build` → **0 errors, 0 warnings**

### Test Results (TC-DON-001 through TC-DON-012)

| ID | Description | Status |
|----|-------------|--------|
| TC-DON-001 | Alumni makes valid donation → 200, donationId returned, RaisedAmount updated | ✅ Pass |
| TC-DON-002 | Student tries POST /api/donations → 403 | ✅ Pass |
| TC-DON-003 | Donation amount = 0 → 400 "Amount must be greater than 0" | ✅ Pass |
| TC-DON-004 | Donation amount = -1000 → 400 validation error | ✅ Pass |
| TC-DON-005 | Donation amount = 1,000,001 → 400 "exceeds maximum of 1,000,000" | ✅ Pass |
| TC-DON-006 | Donation to expired campaign (EndDate past) → 400 "Campaign has ended" | ✅ Pass |
| TC-DON-007 | Donation to Cancelled campaign → 400 "Campaign not accepting donations" | ✅ Pass |
| TC-DON-008 | GET /api/donations/mine → only caller's donations | ✅ Pass |
| TC-DON-009 | GET /api/admin/donations (Admin) → all donations | ✅ Pass |
| TC-DON-010 | 3 alumni donate 1000+2500+500 → RaisedAmount = initial+4000 (trigger verified) | ✅ Pass |
| TC-DON-011 | Admin creates campaign → 200, Status=Active | ✅ Pass |
| TC-DON-012 | Alumnus tries POST /api/campaigns → 403 | ✅ Pass |

---

## Events + Tickets Module (feature/events-tickets)

### Backend Endpoints

| Method | Route | Role | Description |
|--------|-------|------|-------------|
| GET | `/api/events` | All Auth | List events (filters: status, category, fromDate, toDate, includeAll, page, pageSize) |
| GET | `/api/events/{id}` | All Auth | Single event detail with isRegistered flag |
| POST | `/api/events` | Admin | Create event (FR-501–504) |
| PUT | `/api/events/{id}` | Admin | Edit event — blocked if registrations exist (FR-503) |
| PUT | `/api/events/{id}/cancel` | Admin | Cancel event → DB trigger cascades (FR-506) |
| DELETE | `/api/events/{id}` | Admin | Delete event — blocked if registrations exist (FR-505) |
| POST | `/api/events/{id}/register` | All Auth | Register via sp_RegisterForEvent SP (FR-601) |
| GET | `/api/events/{id}/attendees` | Admin | List attendees |
| PUT | `/api/events/{id}/attendance` | Admin | Bulk mark attendance (TC-TKT-010) |
| GET | `/api/tickets/mine` | All Auth | My tickets with event info (FR-604) |
| DELETE | `/api/tickets/{id}` | All Auth | Cancel registration + full refund (FR-606/607) |

### Frontend Pages

| Page | Role | Description |
|------|------|-------------|
| `admin/events.html` | Admin | Table with Upcoming/Past/Cancelled tabs, Edit/Cancel/Attendees actions |
| `admin/event-form.html` | Admin | Create & Edit form (detects ?id= for edit mode) |
| `admin/attendees.html` | Admin | Attendance marking with checkboxes, bulk save |
| `shared/events.html` | All | Cards with Upcoming/Past tabs, category filter, register link |
| `shared/event-detail.html` | All | Event info, seats bar, state-aware action (Register/Full/Registered) |
| `shared/event-register.html` | All | Free: confirm button; Paid: mock card form + Pay Now |
| `shared/my-tickets.html` | All | Upcoming/Past/Cancelled tabs, Cancel button (>24h rule) |

### Build Status
`dotnet build` → **0 C# errors** (file-lock warning only — app was running)

### Test Results

| ID | Description | Status |
|----|-------------|--------|
| TC-EVT-001 | Admin creates event with valid data → 200, eventId returned | ✅ Pass |
| TC-EVT-002 | Create event with date < 24h ahead → 400 | ✅ Pass |
| TC-EVT-003 | Create event with invalid category → 400 | ✅ Pass |
| TC-EVT-004 | Non-admin tries POST /api/events → 403 | ✅ Pass |
| TC-EVT-005 | Admin edits event (no registrations) → 200 | ✅ Pass |
| TC-EVT-006 | Edit event with existing registrations → 400 | ✅ Pass |
| TC-EVT-007 | Admin cancels event → Status=Cancelled, trigger cascades | ✅ Pass |
| TC-EVT-008 | Cancel already-cancelled event → 400 | ✅ Pass |
| TC-EVT-009 | GET /api/events returns paginated Scheduled events | ✅ Pass |
| TC-TKT-001 | User registers for Scheduled event → ticket issued | ✅ Pass |
| TC-TKT-002 | Duplicate registration blocked (SP raises error) | ✅ Pass |
| TC-TKT-003 | Register for full event → 400 | ✅ Pass |
| TC-TKT-004 | Register for Cancelled event → 400 | ✅ Pass |
| TC-TKT-005 | GET /api/tickets/mine returns user's tickets | ✅ Pass |
| TC-TKT-006 | Cancel registration >24h before event → 200, refunded | ✅ Pass |
| TC-TKT-007 | Cancel registration ≤24h before event → 400 | ✅ Pass |
| TC-TKT-008 | Cancel already-cancelled ticket → 400 | ✅ Pass |
| TC-TKT-009 | Cancel another user's ticket → 403 | ✅ Pass |
| TC-TKT-010 | Admin marks attendance for event → 200 | ✅ Pass |

## Mentorship Module — Backend (feature/mentorship)

### Endpoints Implemented

| Method | Route | Role | Description |
|--------|-------|------|-------------|
| POST | `/api/mentorship/request` | Student | Send mentorship request (FR-401–403) |
| PUT | `/api/mentorship/{id}/respond` | Alumni | Accept or Reject request (FR-404, FR-406) |
| GET | `/api/mentorship/sent` | Student | My sent requests with status filter + pagination |
| GET | `/api/mentorship/received` | Alumni | Inbox with status filter + pagination |
| GET | `/api/mentorship/status?alumniId=X` | Student | Button state on alumni detail page |
| DELETE | `/api/mentorship/{id}` | Student | Cancel own Pending request |

### Previous Tests (All Still Passing)

| ID | Description | Status |
|----|-------------|--------|
| TC-PROF-001 | Alumni: GET /api/alumni/me with valid JWT | ✅ Pass |
| TC-PROF-002 | Alumni: PUT /api/alumni/me valid update | ✅ Pass |
| TC-PROF-003 | Alumni: PUT with invalid GraduationYear | ✅ Pass |
| TC-PROF-004 | Alumni: PUT with invalid DepartmentId | ✅ Pass |
| TC-PROF-005 | Student: GET /api/students/me with valid JWT | ✅ Pass |
| TC-PROF-006 | Student: PUT /api/students/me valid update | ✅ Pass |
| TC-PROF-007 | Student: PUT with invalid CGPA | ✅ Pass |
| TC-DIR-001 | GET /api/alumni with Student JWT — returns list | ✅ Pass |
| TC-DIR-002 | GET /api/alumni with Alumni JWT — 403 | ✅ Pass |
| TC-DIR-003 | GET /api/alumni?keyword=... — filters correctly | ✅ Pass |
| TC-DIR-004 | GET /api/alumni?department=... — filters correctly | ✅ Pass |
| TC-DIR-005 | GET /api/alumni?page=1&pageSize=3 — pagination works | ✅ Pass |
| TC-DIR-006 | GET /api/alumni/{id} — public profile returned | ✅ Pass |
| TC-DIR-007 | GET /api/alumni/{id} — email/phone hidden without mentorship | ✅ Pass |
| TC-DIR-008 | GET /api/departments — returns 8 departments (no auth) | ✅ Pass |
| TC-MENT-001 | Student sends valid request (20-500 char message) | ✅ Pass |
| TC-MENT-002 | Alumni accepts pending request → status = Accepted | ✅ Pass |
| TC-MENT-003 | Alumni rejects pending request → status = Rejected | ✅ Pass |
| TC-MENT-004 | Duplicate Pending request blocked (409-equivalent 400) | ✅ Pass |
| TC-MENT-005 | Message < 20 chars → 400 "at least 20 characters" | ✅ Pass |
| TC-MENT-006 | Message > 500 chars → 400 "cannot exceed 500 characters" | ✅ Pass |
| TC-MENT-007 | Exactly 20 char message → accepted (boundary inclusive) | ✅ Pass |
| TC-MENT-008 | Alumni A responds to Alumni B's request → 403 | ✅ Pass |
| TC-MENT-009 | Accept, then try to change status → 400 immutability | ✅ Pass |
| TC-MENT-010 | Alumni tries POST /api/mentorship/request → 403 | ✅ Pass |
| TC-MENT-011 | After acceptance, contact info visible in GET /api/alumni/{id} | ✅ Pass |

## Completed Commits
- `f98b093` feat: profiles + alumni directory complete (feature/profiles-directory)
- `97cfc39` Merge feature/profiles-directory → develop (pushed to GitHub)
- `680ed81` feat: donations module complete (feature/donations)
- `ca5319d` Merge feature/donations → develop (pushed to GitHub)

## Next Up
- Admin panel (reports, invite codes management)
- Notifications
