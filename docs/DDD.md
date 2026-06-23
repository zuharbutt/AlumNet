# Alumni Management System — Database Design Document (DDD)

**Project:** Alumni Management System
**Course:** CL2005 — Database Systems
**Team:** Zuhar Faisal, Ramsha Khalid, M. Waleed
**Semester:** Spring 2026
**Section:** BSE-4A
**Instructor:** Sir Umar Farooq
**Document Version:** 1.0

---

## Table of Contents
1. Introduction
2. Database Overview & Conventions
3. Entity-Relationship Model
4. Complete MSSQL Schema
5. Indexes Strategy
6. Views
7. Stored Procedures
8. Triggers
9. Normalization Analysis
10. Data Dictionary
11. Sample Complex Queries
12. Seed Data
13. Backup & Restore Strategy
14. Appendix

---

## 1. Introduction

### 1.1 Purpose
This document defines the complete database design for the Alumni Management System. It specifies every table, column, constraint, relationship, index, view, stored procedure, and trigger that will make up the `AlumniMS` database in Microsoft SQL Server.

### 1.2 Scope
- Physical database schema in MSSQL
- Data integrity rules (constraints, triggers)
- Query optimization (indexes, views)
- Business logic encapsulated in stored procedures
- Normalization compliance up to 3NF

### 1.3 Related Documents
- **SRS** — Software Requirements Specification
- **Test Plan** — Testing strategy and test cases

### 1.4 Target DBMS
**Microsoft SQL Server 2019 or later** (SQL Server Express Edition is sufficient for development).

---

## 2. Database Overview & Conventions

### 2.1 Database Name
`AlumniMS`

### 2.2 Naming Conventions

| Object | Convention | Example |
|---|---|---|
| Database | PascalCase | `AlumniMS` |
| Table names | PascalCase, plural | `Users`, `AlumniProfiles`, `MentorshipRequests` |
| Column names | PascalCase | `UserId`, `CreatedAt`, `GraduationYear` |
| Primary keys | `<EntityName>Id` | `UserId`, `EventId` |
| Foreign keys | Same as referenced PK | `UserId` in `AlumniProfiles` references `Users.UserId` |
| Constraints (CHECK) | `CK_<Table>_<Column>` | `CK_Users_Role` |
| Constraints (FK) | `FK_<Table>_<Reference>` | `FK_Alumni_Department` |
| Constraints (UNIQUE) | `UQ_<Table>_<Column>` | `UQ_EventReg_EventUser` |
| Indexes | `IX_<Table>_<Column(s)>` | `IX_Users_Email` |
| Views | `vw_<Purpose>` | `vw_AlumniDirectory` |
| Stored Procedures | `sp_<Action>` | `sp_SearchAlumni` |
| Triggers | `tr_<Table>_<Event>` | `tr_Donations_AfterInsert` |

### 2.3 Data Type Standards

| Purpose | Data Type | Rationale |
|---|---|---|
| Primary keys | `INT IDENTITY(1,1)` | Auto-incrementing, efficient |
| Short text (names, titles) | `NVARCHAR(n)` | Unicode support for international names |
| Long text (descriptions, bios) | `NVARCHAR(500–2000)` | Sized to purpose |
| Email | `NVARCHAR(150)` | Standard email length |
| Phone | `NVARCHAR(20)` | Handles international formats |
| Password hash | `NVARCHAR(500)` | Accommodates BCrypt and future algorithms |
| Date only | `DATE` | No time component needed (e.g., DOB) |
| Date + time | `DATETIME2` | More precise than `DATETIME` (0.1μs vs 3.33ms) |
| Money | `DECIMAL(10,2)` / `DECIMAL(14,2)` | Never use `FLOAT` for money |
| Enums (status, role) | `NVARCHAR(20)` + CHECK constraint | Human-readable; preferable to TINYINT for maintainability |
| Boolean | `BIT` | MSSQL's native boolean |
| GUIDs (ticket codes) | `NVARCHAR(50)` | Stored as string for readability |

### 2.4 Design Principles
1. **Referential integrity** enforced via foreign keys everywhere.
2. **Domain integrity** enforced via CHECK constraints.
3. **Entity integrity** enforced via primary keys.
4. **Soft deletes preferred** over hard deletes for historical data (e.g., events Status = `Cancelled`, not deleted).
5. **Timestamps on every table** (`CreatedAt`, `UpdatedAt` where applicable).
6. **No nulls where avoidable** — use defaults or NOT NULL.

---

## 3. Entity-Relationship Model

### 3.1 Entities Summary

| # | Entity | Purpose | Role in System |
|---|---|---|---|
| 1 | `Users` | Authentication for all roles | Core user table |
| 2 | `Departments` | Academic departments | Lookup table |
| 3 | `AdminInviteCodes` | One-time admin registration codes | Admin onboarding |
| 4 | `AlumniProfiles` | Extended profile for alumni | 1:1 with Users |
| 5 | `StudentProfiles` | Extended profile for students | 1:1 with Users |
| 6 | `MentorshipRequests` | Student-to-alumni requests | M:N between users |
| 7 | `Events` | University events | Owned by admin |
| 8 | `EventRegistrations` | User-event junction | M:N, with attendance |
| 9 | `Tickets` | Issued on registration | 1:1 with registration |
| 10 | `DonationCampaigns` | Fundraising campaigns | Owned by admin |
| 11 | `Donations` | Individual donation transactions | M:N between alumni and campaigns |

### 3.2 Relationships

```
Departments (1) ────────< (M) AlumniProfiles
Departments (1) ────────< (M) StudentProfiles

Users (1) ──────────────── (1) AlumniProfiles      [for Role = 'Alumni']
Users (1) ──────────────── (1) StudentProfiles     [for Role = 'Student']
Users (1) ────────────────< (M) AdminInviteCodes   [as CreatedByUserId]
Users (1) ────────────────< (M) AdminInviteCodes   [as UsedByUserId]

Users (1) ────────────────< (M) MentorshipRequests [as StudentUserId]
Users (1) ────────────────< (M) MentorshipRequests [as AlumniUserId]

Users (1) ────────────────< (M) Events             [as CreatedByUserId, admin]
Users (1) ────────────────< (M) EventRegistrations
Events (1) ───────────────< (M) EventRegistrations

EventRegistrations (1) ──── (1) Tickets

Users (1) ────────────────< (M) DonationCampaigns  [as CreatedByUserId, admin]
Users (1) ────────────────< (M) Donations          [as DonorUserId, alumni]
DonationCampaigns (1) ────< (M) Donations
```

### 3.3 Conceptual ER Diagram (Text Representation)

```
                    ┌──────────────┐
                    │ Departments  │
                    └──────┬───────┘
                           │ 1:M
          ┌────────────────┼────────────────┐
          ▼                                  ▼
   ┌─────────────┐                    ┌──────────────┐
   │AlumniProfiles│                   │StudentProfiles│
   └──────┬──────┘                    └──────┬───────┘
          │ 1:1                              │ 1:1
          ▼                                  ▼
   ┌─────────────────────────────────────────────┐
   │                   Users                      │
   │   (UserId, FullName, Email, Role, ...)       │
   └─┬───────────┬──────────┬────────┬───────────┘
     │           │          │        │
     │           │          │        │ M:M (via MentorshipRequests)
     │           │          │        ▼
     │           │          │  ┌───────────────────┐
     │           │          │  │MentorshipRequests │
     │           │          │  └───────────────────┘
     │           │          │
     │           │          │ M:M (via EventRegistrations)
     │           │          ▼
     │           │   ┌──────────────────┐        ┌──────────┐
     │           │   │EventRegistrations│ ◄────► │ Tickets  │
     │           │   └────────┬─────────┘  1:1   └──────────┘
     │           │            │ M:1
     │           │            ▼
     │           │       ┌────────┐
     │           │       │ Events │
     │           │       └────────┘
     │           │
     │           │ M:M (via Donations)
     │           ▼
     │    ┌───────────┐        ┌───────────────────┐
     │    │ Donations │ ─────► │ DonationCampaigns │
     │    └───────────┘  M:1   └───────────────────┘
     │
     │ M:M (self-reference for admin invite codes)
     ▼
┌──────────────────┐
│ AdminInviteCodes │
└──────────────────┘
```

> **Note:** You should create a proper visual ER diagram using [dbdiagram.io](https://dbdiagram.io), [draw.io](https://draw.io), or MS Visio and save it as `database/ER_Diagram.png`.

### 3.4 Cardinality Summary

| Relationship | Type | Notes |
|---|---|---|
| Users ↔ AlumniProfiles | 1:1 | Only when Role = 'Alumni' |
| Users ↔ StudentProfiles | 1:1 | Only when Role = 'Student' |
| Departments ↔ AlumniProfiles | 1:M | Many alumni per department |
| Departments ↔ StudentProfiles | 1:M | Many students per department |
| Users (Student) ↔ Users (Alumni) | M:M | Via MentorshipRequests |
| Users ↔ Events | M:M | Via EventRegistrations |
| EventRegistrations ↔ Tickets | 1:1 | Ticket generated per registration |
| Users (Alumni) ↔ DonationCampaigns | M:M | Via Donations |

---

## 4. Complete MSSQL Schema

Full `CREATE TABLE` scripts with all constraints. Execute in the order presented (dependencies matter).

### 4.1 Create Database

```sql
-- ========================================================================
-- 01_create_database.sql
-- ========================================================================
IF DB_ID('AlumniMS') IS NOT NULL
BEGIN
    ALTER DATABASE AlumniMS SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE AlumniMS;
END
GO

CREATE DATABASE AlumniMS;
GO

USE AlumniMS;
GO
```

### 4.2 Lookup Tables

```sql
-- ========================================================================
-- Table: Departments
-- Purpose: Stores academic departments (pre-seeded lookup table)
-- ========================================================================
CREATE TABLE Departments (
    DepartmentId    INT IDENTITY(1,1) NOT NULL,
    Name            NVARCHAR(100) NOT NULL,
    Description     NVARCHAR(500) NULL,
    CreatedAt       DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),

    CONSTRAINT PK_Departments       PRIMARY KEY (DepartmentId),
    CONSTRAINT UQ_Departments_Name  UNIQUE (Name)
);
GO
```

### 4.3 Users Table

```sql
-- ========================================================================
-- Table: Users
-- Purpose: Central authentication table for all roles
-- ========================================================================
CREATE TABLE Users (
    UserId          INT IDENTITY(1,1) NOT NULL,
    FullName        NVARCHAR(150)   NOT NULL,
    Email           NVARCHAR(150)   NOT NULL,
    PasswordHash    NVARCHAR(500)   NOT NULL,
    Phone           NVARCHAR(20)    NULL,
    DateOfBirth     DATE            NULL,
    Role            NVARCHAR(20)    NOT NULL,
    IsActive        BIT             NOT NULL DEFAULT 1,
    FailedAttempts  INT             NOT NULL DEFAULT 0,
    LockedUntil     DATETIME2       NULL,
    CreatedAt       DATETIME2       NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt       DATETIME2       NULL,

    CONSTRAINT PK_Users         PRIMARY KEY (UserId),
    CONSTRAINT UQ_Users_Email   UNIQUE (Email),
    CONSTRAINT CK_Users_Role    CHECK (Role IN ('Student', 'Alumni', 'Admin')),
    CONSTRAINT CK_Users_Email   CHECK (Email LIKE '%_@_%._%'),
    CONSTRAINT CK_Users_Attempts CHECK (FailedAttempts >= 0)
);
GO
```

### 4.4 Admin Invite Codes

```sql
-- ========================================================================
-- Table: AdminInviteCodes
-- Purpose: One-time codes used to register admin accounts
-- ========================================================================
CREATE TABLE AdminInviteCodes (
    CodeId          INT IDENTITY(1,1) NOT NULL,
    Code            NVARCHAR(50)    NOT NULL,
    IsUsed          BIT             NOT NULL DEFAULT 0,
    ExpiryDate      DATETIME2       NOT NULL,
    CreatedByUserId INT             NULL,
    UsedByUserId    INT             NULL,
    CreatedAt       DATETIME2       NOT NULL DEFAULT SYSUTCDATETIME(),
    UsedAt          DATETIME2       NULL,

    CONSTRAINT PK_InviteCodes           PRIMARY KEY (CodeId),
    CONSTRAINT UQ_InviteCodes_Code      UNIQUE (Code),
    CONSTRAINT FK_InviteCodes_CreatedBy FOREIGN KEY (CreatedByUserId)
        REFERENCES Users(UserId) ON DELETE NO ACTION,
    CONSTRAINT FK_InviteCodes_UsedBy    FOREIGN KEY (UsedByUserId)
        REFERENCES Users(UserId) ON DELETE NO ACTION,
    CONSTRAINT CK_InviteCodes_Expiry    CHECK (ExpiryDate > CreatedAt),
    CONSTRAINT CK_InviteCodes_UsedState CHECK (
        (IsUsed = 0 AND UsedByUserId IS NULL AND UsedAt IS NULL)
        OR
        (IsUsed = 1 AND UsedByUserId IS NOT NULL AND UsedAt IS NOT NULL)
    )
);
GO
```

### 4.5 Alumni Profiles

```sql
-- ========================================================================
-- Table: AlumniProfiles
-- Purpose: Extended profile for Alumni users (1:1 with Users)
-- ========================================================================
CREATE TABLE AlumniProfiles (
    AlumniProfileId INT IDENTITY(1,1) NOT NULL,
    UserId          INT             NOT NULL,
    DepartmentId    INT             NOT NULL,
    GraduationYear  INT             NOT NULL,
    DegreeProgram   NVARCHAR(100)   NOT NULL,
    CurrentCompany  NVARCHAR(150)   NULL,
    JobTitle        NVARCHAR(100)   NULL,
    Industry        NVARCHAR(100)   NULL,
    WorkLocation    NVARCHAR(150)   NULL,
    LinkedInUrl     NVARCHAR(300)   NULL,
    ShortBio        NVARCHAR(1000)  NULL,
    CreatedAt       DATETIME2       NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt       DATETIME2       NULL,

    CONSTRAINT PK_AlumniProfiles        PRIMARY KEY (AlumniProfileId),
    CONSTRAINT UQ_AlumniProfiles_User   UNIQUE (UserId),
    CONSTRAINT FK_Alumni_User           FOREIGN KEY (UserId)
        REFERENCES Users(UserId) ON DELETE CASCADE,
    CONSTRAINT FK_Alumni_Department     FOREIGN KEY (DepartmentId)
        REFERENCES Departments(DepartmentId) ON DELETE NO ACTION,
    CONSTRAINT CK_Alumni_GradYear       CHECK (GraduationYear BETWEEN 1950 AND YEAR(GETDATE())),
    CONSTRAINT CK_Alumni_LinkedIn       CHECK (LinkedInUrl IS NULL OR LinkedInUrl LIKE 'http%linkedin.com%')
);
GO
```

### 4.6 Student Profiles

```sql
-- ========================================================================
-- Table: StudentProfiles
-- Purpose: Extended profile for Student users (1:1 with Users)
-- ========================================================================
CREATE TABLE StudentProfiles (
    StudentProfileId       INT IDENTITY(1,1) NOT NULL,
    UserId                 INT             NOT NULL,
    DepartmentId           INT             NOT NULL,
    EnrollmentYear         INT             NOT NULL,
    ExpectedGraduationYear INT             NOT NULL,
    DegreeProgram          NVARCHAR(100)   NOT NULL,
    CurrentSemester        INT             NULL,
    CGPA                   DECIMAL(3,2)    NULL,
    ShortBio               NVARCHAR(1000)  NULL,
    CreatedAt              DATETIME2       NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt              DATETIME2       NULL,

    CONSTRAINT PK_StudentProfiles       PRIMARY KEY (StudentProfileId),
    CONSTRAINT UQ_StudentProfiles_User  UNIQUE (UserId),
    CONSTRAINT FK_Student_User          FOREIGN KEY (UserId)
        REFERENCES Users(UserId) ON DELETE CASCADE,
    CONSTRAINT FK_Student_Department    FOREIGN KEY (DepartmentId)
        REFERENCES Departments(DepartmentId) ON DELETE NO ACTION,
    CONSTRAINT CK_Student_EnrollYear    CHECK (EnrollmentYear BETWEEN 2000 AND YEAR(GETDATE())),
    CONSTRAINT CK_Student_GradYear      CHECK (ExpectedGraduationYear > EnrollmentYear),
    CONSTRAINT CK_Student_CGPA          CHECK (CGPA IS NULL OR (CGPA BETWEEN 0.00 AND 4.00)),
    CONSTRAINT CK_Student_Semester      CHECK (CurrentSemester IS NULL OR CurrentSemester BETWEEN 1 AND 12)
);
GO
```

### 4.7 Mentorship Requests

```sql
-- ========================================================================
-- Table: MentorshipRequests
-- Purpose: Records mentorship requests from students to alumni
-- ========================================================================
CREATE TABLE MentorshipRequests (
    RequestId       INT IDENTITY(1,1) NOT NULL,
    StudentUserId   INT             NOT NULL,
    AlumniUserId    INT             NOT NULL,
    Message         NVARCHAR(500)   NOT NULL,
    Status          NVARCHAR(20)    NOT NULL DEFAULT 'Pending',
    RequestedAt     DATETIME2       NOT NULL DEFAULT SYSUTCDATETIME(),
    RespondedAt     DATETIME2       NULL,
    ResponseMessage NVARCHAR(500)   NULL,

    CONSTRAINT PK_MentorshipRequests  PRIMARY KEY (RequestId),
    CONSTRAINT FK_Mentorship_Student  FOREIGN KEY (StudentUserId)
        REFERENCES Users(UserId) ON DELETE NO ACTION,
    CONSTRAINT FK_Mentorship_Alumni   FOREIGN KEY (AlumniUserId)
        REFERENCES Users(UserId) ON DELETE NO ACTION,
    CONSTRAINT CK_Mentorship_Status   CHECK (Status IN ('Pending', 'Accepted', 'Rejected', 'Cancelled', 'Expired')),
    CONSTRAINT CK_Mentorship_Users    CHECK (StudentUserId <> AlumniUserId),
    CONSTRAINT CK_Mentorship_MsgLen   CHECK (LEN(Message) BETWEEN 20 AND 500),
    CONSTRAINT CK_Mentorship_Response CHECK (
        (Status = 'Pending' AND RespondedAt IS NULL)
        OR
        (Status IN ('Accepted','Rejected','Cancelled','Expired') AND RespondedAt IS NOT NULL)
    )
);
GO
```

### 4.8 Events

```sql
-- ========================================================================
-- Table: Events
-- Purpose: University events created by admin
-- ========================================================================
CREATE TABLE Events (
    EventId         INT IDENTITY(1,1) NOT NULL,
    Title           NVARCHAR(200)   NOT NULL,
    Description     NVARCHAR(2000)  NOT NULL,
    Category        NVARCHAR(50)    NOT NULL,
    EventDate       DATETIME2       NOT NULL,
    Location        NVARCHAR(300)   NOT NULL,
    Capacity        INT             NOT NULL,
    TicketPrice     DECIMAL(10,2)   NOT NULL DEFAULT 0,
    Status          NVARCHAR(20)    NOT NULL DEFAULT 'Scheduled',
    CreatedByUserId INT             NOT NULL,
    CreatedAt       DATETIME2       NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt       DATETIME2       NULL,

    CONSTRAINT PK_Events            PRIMARY KEY (EventId),
    CONSTRAINT FK_Events_Creator    FOREIGN KEY (CreatedByUserId)
        REFERENCES Users(UserId) ON DELETE NO ACTION,
    CONSTRAINT CK_Events_Category   CHECK (Category IN ('Reunion','Seminar','Workshop','Networking','Conference','Other')),
    CONSTRAINT CK_Events_Status     CHECK (Status IN ('Scheduled','Cancelled','Completed')),
    CONSTRAINT CK_Events_Capacity   CHECK (Capacity >= 1),
    CONSTRAINT CK_Events_Price      CHECK (TicketPrice >= 0)
);
GO
```

### 4.9 Event Registrations

```sql
-- ========================================================================
-- Table: EventRegistrations
-- Purpose: Junction table linking users to events they've registered for
-- ========================================================================
CREATE TABLE EventRegistrations (
    RegistrationId  INT IDENTITY(1,1) NOT NULL,
    EventId         INT             NOT NULL,
    UserId          INT             NOT NULL,
    RegisteredAt    DATETIME2       NOT NULL DEFAULT SYSUTCDATETIME(),
    Attended        BIT             NOT NULL DEFAULT 0,
    CancelledAt     DATETIME2       NULL,

    CONSTRAINT PK_EventRegistrations PRIMARY KEY (RegistrationId),
    CONSTRAINT FK_EventReg_Event     FOREIGN KEY (EventId)
        REFERENCES Events(EventId) ON DELETE CASCADE,
    CONSTRAINT FK_EventReg_User      FOREIGN KEY (UserId)
        REFERENCES Users(UserId) ON DELETE NO ACTION,
    CONSTRAINT UQ_EventReg_EventUser UNIQUE (EventId, UserId)  -- prevents double registration
);
GO
```

### 4.10 Tickets

```sql
-- ========================================================================
-- Table: Tickets
-- Purpose: Tickets issued on successful event registration
-- ========================================================================
CREATE TABLE Tickets (
    TicketId        INT IDENTITY(1,1) NOT NULL,
    RegistrationId  INT             NOT NULL,
    TicketCode      NVARCHAR(50)    NOT NULL,
    Price           DECIMAL(10,2)   NOT NULL,
    PaymentStatus   NVARCHAR(20)    NOT NULL DEFAULT 'Paid',
    IssuedAt        DATETIME2       NOT NULL DEFAULT SYSUTCDATETIME(),
    RefundedAt      DATETIME2       NULL,

    CONSTRAINT PK_Tickets           PRIMARY KEY (TicketId),
    CONSTRAINT UQ_Tickets_Reg       UNIQUE (RegistrationId),
    CONSTRAINT UQ_Tickets_Code      UNIQUE (TicketCode),
    CONSTRAINT FK_Tickets_Reg       FOREIGN KEY (RegistrationId)
        REFERENCES EventRegistrations(RegistrationId) ON DELETE CASCADE,
    CONSTRAINT CK_Tickets_Status    CHECK (PaymentStatus IN ('Paid','Refunded','Pending')),
    CONSTRAINT CK_Tickets_Price     CHECK (Price >= 0)
);
GO
```

### 4.11 Donation Campaigns

```sql
-- ========================================================================
-- Table: DonationCampaigns
-- Purpose: Fundraising campaigns created by admin
-- ========================================================================
CREATE TABLE DonationCampaigns (
    CampaignId      INT IDENTITY(1,1) NOT NULL,
    Title           NVARCHAR(200)   NOT NULL,
    Description     NVARCHAR(2000)  NOT NULL,
    TargetAmount    DECIMAL(14,2)   NOT NULL,
    RaisedAmount    DECIMAL(14,2)   NOT NULL DEFAULT 0,
    StartDate       DATETIME2       NOT NULL DEFAULT SYSUTCDATETIME(),
    EndDate         DATETIME2       NOT NULL,
    Status          NVARCHAR(20)    NOT NULL DEFAULT 'Active',
    CreatedByUserId INT             NOT NULL,
    CreatedAt       DATETIME2       NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt       DATETIME2       NULL,

    CONSTRAINT PK_DonationCampaigns PRIMARY KEY (CampaignId),
    CONSTRAINT FK_Camp_Creator      FOREIGN KEY (CreatedByUserId)
        REFERENCES Users(UserId) ON DELETE NO ACTION,
    CONSTRAINT CK_Camp_Status       CHECK (Status IN ('Active','Closed','Cancelled')),
    CONSTRAINT CK_Camp_Target       CHECK (TargetAmount > 0),
    CONSTRAINT CK_Camp_Raised       CHECK (RaisedAmount >= 0),
    CONSTRAINT CK_Camp_Dates        CHECK (EndDate > StartDate)
);
GO
```

### 4.12 Donations

```sql
-- ========================================================================
-- Table: Donations
-- Purpose: Individual donation transactions (mocked payments)
-- ========================================================================
CREATE TABLE Donations (
    DonationId      INT IDENTITY(1,1) NOT NULL,
    CampaignId      INT             NOT NULL,
    DonorUserId     INT             NOT NULL,
    Amount          DECIMAL(14,2)   NOT NULL,
    IsAnonymous     BIT             NOT NULL DEFAULT 0,
    Status          NVARCHAR(20)    NOT NULL DEFAULT 'Completed',
    DonatedAt       DATETIME2       NOT NULL DEFAULT SYSUTCDATETIME(),

    CONSTRAINT PK_Donations         PRIMARY KEY (DonationId),
    CONSTRAINT FK_Don_Campaign      FOREIGN KEY (CampaignId)
        REFERENCES DonationCampaigns(CampaignId) ON DELETE NO ACTION,
    CONSTRAINT FK_Don_Donor         FOREIGN KEY (DonorUserId)
        REFERENCES Users(UserId) ON DELETE NO ACTION,
    CONSTRAINT CK_Don_Status        CHECK (Status IN ('Completed','Refunded','Failed')),
    CONSTRAINT CK_Don_Amount        CHECK (Amount > 0 AND Amount <= 1000000)
);
GO
```

### 4.13 Constraint Summary

| Constraint Type | Count | Examples |
|---|---|---|
| Primary Keys | 11 | One per table |
| Foreign Keys | 13 | Maintain referential integrity |
| UNIQUE | 7 | Email, ticket codes, invite codes, etc. |
| CHECK | 22+ | Business rules enforced at DB level |
| DEFAULT | 15+ | Timestamps, statuses |
| NOT NULL | Many | Required fields |

---

## 5. Indexes Strategy

Primary keys are automatically indexed (clustered). Foreign keys get non-clustered indexes to speed up joins. Additional indexes are added on columns frequently used in `WHERE`, `JOIN`, and `ORDER BY` clauses.

### 5.1 Index Creation Script

```sql
-- ========================================================================
-- 03_indexes.sql
-- ========================================================================
USE AlumniMS;
GO

-- ----------------- Users -----------------
-- Primary lookup: login by email (handled by UQ_Users_Email which creates a unique index)
CREATE INDEX IX_Users_Role           ON Users(Role);
CREATE INDEX IX_Users_IsActive       ON Users(IsActive) WHERE IsActive = 1;  -- filtered index

-- ----------------- AlumniProfiles -----------------
-- Directory filters
CREATE INDEX IX_Alumni_Department    ON AlumniProfiles(DepartmentId);
CREATE INDEX IX_Alumni_GradYear      ON AlumniProfiles(GraduationYear);
CREATE INDEX IX_Alumni_Industry      ON AlumniProfiles(Industry);
CREATE INDEX IX_Alumni_WorkLocation  ON AlumniProfiles(WorkLocation);
-- Composite for common multi-filter queries
CREATE INDEX IX_Alumni_Dept_Year     ON AlumniProfiles(DepartmentId, GraduationYear);

-- ----------------- StudentProfiles -----------------
CREATE INDEX IX_Student_Department   ON StudentProfiles(DepartmentId);

-- ----------------- MentorshipRequests -----------------
CREATE INDEX IX_Mentorship_Alumni_Status  ON MentorshipRequests(AlumniUserId, Status);
CREATE INDEX IX_Mentorship_Student_Status ON MentorshipRequests(StudentUserId, Status);
CREATE INDEX IX_Mentorship_RequestedAt    ON MentorshipRequests(RequestedAt DESC);

-- ----------------- Events -----------------
CREATE INDEX IX_Events_Date_Status   ON Events(EventDate, Status);
CREATE INDEX IX_Events_Category      ON Events(Category);
CREATE INDEX IX_Events_CreatedBy     ON Events(CreatedByUserId);

-- ----------------- EventRegistrations -----------------
CREATE INDEX IX_EventReg_User        ON EventRegistrations(UserId);
CREATE INDEX IX_EventReg_Event       ON EventRegistrations(EventId);
-- Note: UQ_EventReg_EventUser already covers (EventId, UserId)

-- ----------------- Tickets -----------------
-- UQ_Tickets_Code already creates unique index on TicketCode
CREATE INDEX IX_Tickets_PaymentStatus ON Tickets(PaymentStatus);

-- ----------------- DonationCampaigns -----------------
CREATE INDEX IX_Camp_Status_EndDate  ON DonationCampaigns(Status, EndDate);

-- ----------------- Donations -----------------
CREATE INDEX IX_Don_Campaign         ON Donations(CampaignId);
CREATE INDEX IX_Don_Donor            ON Donations(DonorUserId);
CREATE INDEX IX_Don_DonatedAt        ON Donations(DonatedAt DESC);

-- ----------------- AdminInviteCodes -----------------
-- UQ_InviteCodes_Code already creates unique index on Code
CREATE INDEX IX_InviteCodes_IsUsed   ON AdminInviteCodes(IsUsed) WHERE IsUsed = 0;  -- filtered
GO
```

### 5.2 Index Justification Table

| Index | Query Speeds Up | Expected Query Pattern |
|---|---|---|
| `UQ_Users_Email` | Login authentication | `WHERE Email = @email` |
| `IX_Users_Role` | Filter users by role | `WHERE Role = 'Alumni'` |
| `IX_Alumni_Department` | Directory department filter | `WHERE DepartmentId = @id` |
| `IX_Alumni_GradYear` | Directory year filter | `WHERE GraduationYear BETWEEN @a AND @b` |
| `IX_Alumni_Dept_Year` (composite) | Combined filter queries | `WHERE DepartmentId = @id AND GraduationYear >= @y` |
| `IX_Mentorship_Alumni_Status` | Alumnus inbox: pending requests | `WHERE AlumniUserId = @id AND Status = 'Pending'` |
| `IX_Events_Date_Status` | List upcoming scheduled events | `WHERE EventDate > GETDATE() AND Status = 'Scheduled'` |
| `IX_Don_Campaign` | Campaign total computation | `WHERE CampaignId = @id` |
| `IX_Camp_Status_EndDate` | List active campaigns | `WHERE Status = 'Active' AND EndDate > GETDATE()` |

### 5.3 Indexing Tradeoffs

**Benefits:**
- Faster SELECT queries (O(log n) instead of O(n))
- Efficient JOINs
- Fast sorting when `ORDER BY` matches index order

**Costs:**
- Slower INSERT/UPDATE/DELETE (indexes must be maintained)
- Extra disk space
- Potential lock contention

**Our strategy:**
- Read-heavy workload (directory searches, dashboards) → indexes are worth it
- Write-heavy tables (Donations, EventRegistrations) → only essential indexes
- Use **filtered indexes** (`WHERE` clause in index) to reduce overhead (e.g., `IX_InviteCodes_IsUsed`)

---

## 6. Views

Views simplify complex queries and provide a stable interface to the application.

```sql
-- ========================================================================
-- 04_views.sql
-- ========================================================================
USE AlumniMS;
GO

-- ========================================================================
-- View: vw_AlumniDirectory
-- Purpose: Public-facing alumni info for directory search
-- Usage:   Used by sp_SearchAlumni and frontend alumni listing
-- ========================================================================
CREATE OR ALTER VIEW vw_AlumniDirectory
AS
SELECT
    u.UserId,
    u.FullName,
    u.Email,
    u.Phone,
    ap.AlumniProfileId,
    ap.GraduationYear,
    ap.DegreeProgram,
    ap.DepartmentId,
    d.Name              AS DepartmentName,
    ap.CurrentCompany,
    ap.JobTitle,
    ap.Industry,
    ap.WorkLocation,
    ap.LinkedInUrl,
    ap.ShortBio,
    u.CreatedAt         AS RegisteredAt
FROM Users u
INNER JOIN AlumniProfiles ap ON u.UserId = ap.UserId
INNER JOIN Departments d     ON ap.DepartmentId = d.DepartmentId
WHERE u.Role = 'Alumni' AND u.IsActive = 1;
GO

-- ========================================================================
-- View: vw_StudentDirectory
-- Purpose: Student info for admin oversight
-- ========================================================================
CREATE OR ALTER VIEW vw_StudentDirectory
AS
SELECT
    u.UserId,
    u.FullName,
    u.Email,
    sp.EnrollmentYear,
    sp.ExpectedGraduationYear,
    sp.DegreeProgram,
    sp.CurrentSemester,
    sp.CGPA,
    d.Name              AS DepartmentName,
    u.CreatedAt         AS RegisteredAt
FROM Users u
INNER JOIN StudentProfiles sp ON u.UserId = sp.UserId
INNER JOIN Departments d      ON sp.DepartmentId = d.DepartmentId
WHERE u.Role = 'Student' AND u.IsActive = 1;
GO

-- ========================================================================
-- View: vw_UpcomingEvents
-- Purpose: All scheduled future events with live registration counts
-- ========================================================================
CREATE OR ALTER VIEW vw_UpcomingEvents
AS
SELECT
    e.EventId,
    e.Title,
    e.Description,
    e.Category,
    e.EventDate,
    e.Location,
    e.Capacity,
    e.TicketPrice,
    e.Status,
    COUNT(er.RegistrationId) AS RegisteredCount,
    (e.Capacity - COUNT(er.RegistrationId)) AS SeatsRemaining,
    CASE
        WHEN COUNT(er.RegistrationId) >= e.Capacity THEN 1
        ELSE 0
    END AS IsFull
FROM Events e
LEFT JOIN EventRegistrations er
    ON e.EventId = er.EventId AND er.CancelledAt IS NULL
WHERE e.EventDate > SYSUTCDATETIME() AND e.Status = 'Scheduled'
GROUP BY
    e.EventId, e.Title, e.Description, e.Category,
    e.EventDate, e.Location, e.Capacity, e.TicketPrice, e.Status;
GO

-- ========================================================================
-- View: vw_ActiveCampaigns
-- Purpose: Current donation campaigns with progress metrics
-- ========================================================================
CREATE OR ALTER VIEW vw_ActiveCampaigns
AS
SELECT
    c.CampaignId,
    c.Title,
    c.Description,
    c.TargetAmount,
    c.RaisedAmount,
    CAST((c.RaisedAmount * 100.0 / c.TargetAmount) AS DECIMAL(5,2)) AS ProgressPercent,
    c.StartDate,
    c.EndDate,
    DATEDIFF(DAY, SYSUTCDATETIME(), c.EndDate) AS DaysRemaining,
    (SELECT COUNT(*) FROM Donations
     WHERE CampaignId = c.CampaignId AND Status = 'Completed') AS DonorCount,
    c.CreatedByUserId
FROM DonationCampaigns c
WHERE c.Status = 'Active' AND c.EndDate > SYSUTCDATETIME();
GO

-- ========================================================================
-- View: vw_MentorshipOverview
-- Purpose: Combined student + alumni info for admin oversight of mentorship
-- ========================================================================
CREATE OR ALTER VIEW vw_MentorshipOverview
AS
SELECT
    mr.RequestId,
    mr.Status,
    mr.RequestedAt,
    mr.RespondedAt,
    mr.Message,
    mr.ResponseMessage,
    su.UserId       AS StudentUserId,
    su.FullName     AS StudentName,
    su.Email        AS StudentEmail,
    sd.Name         AS StudentDepartment,
    au.UserId       AS AlumniUserId,
    au.FullName     AS AlumniName,
    au.Email        AS AlumniEmail,
    ad.Name         AS AlumniDepartment,
    ap.CurrentCompany,
    ap.JobTitle
FROM MentorshipRequests mr
INNER JOIN Users           su ON mr.StudentUserId = su.UserId
INNER JOIN StudentProfiles sp ON su.UserId = sp.UserId
INNER JOIN Departments     sd ON sp.DepartmentId = sd.DepartmentId
INNER JOIN Users           au ON mr.AlumniUserId = au.UserId
INNER JOIN AlumniProfiles  ap ON au.UserId = ap.UserId
INNER JOIN Departments     ad ON ap.DepartmentId = ad.DepartmentId;
GO

-- ========================================================================
-- View: vw_UserTickets
-- Purpose: User's tickets with event details
-- ========================================================================
CREATE OR ALTER VIEW vw_UserTickets
AS
SELECT
    t.TicketId,
    t.TicketCode,
    t.Price,
    t.PaymentStatus,
    t.IssuedAt,
    er.UserId,
    e.EventId,
    e.Title           AS EventTitle,
    e.EventDate,
    e.Location,
    e.Category,
    e.Status          AS EventStatus,
    er.Attended,
    er.CancelledAt
FROM Tickets t
INNER JOIN EventRegistrations er ON t.RegistrationId = er.RegistrationId
INNER JOIN Events e              ON er.EventId = e.EventId;
GO
```

---

## 7. Stored Procedures

Stored procedures encapsulate complex logic, improve performance by caching execution plans, and reduce round-trips from the application.

```sql
-- ========================================================================
-- 05_stored_procedures.sql
-- ========================================================================
USE AlumniMS;
GO

-- ========================================================================
-- SP: sp_SearchAlumni
-- Purpose: Alumni directory search with filters and pagination
-- ========================================================================
CREATE OR ALTER PROCEDURE sp_SearchAlumni
    @DepartmentId    INT           = NULL,
    @GradYearFrom    INT           = NULL,
    @GradYearTo      INT           = NULL,
    @Industry        NVARCHAR(100) = NULL,
    @Location        NVARCHAR(150) = NULL,
    @Keyword         NVARCHAR(100) = NULL,
    @PageNumber      INT           = 1,
    @PageSize        INT           = 10
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;

    ;WITH Filtered AS (
        SELECT *
        FROM vw_AlumniDirectory
        WHERE (@DepartmentId IS NULL OR DepartmentId   = @DepartmentId)
          AND (@GradYearFrom IS NULL OR GraduationYear >= @GradYearFrom)
          AND (@GradYearTo   IS NULL OR GraduationYear <= @GradYearTo)
          AND (@Industry     IS NULL OR Industry LIKE '%' + @Industry + '%')
          AND (@Location     IS NULL OR WorkLocation LIKE '%' + @Location + '%')
          AND (@Keyword      IS NULL OR FullName LIKE '%' + @Keyword + '%'
                                     OR CurrentCompany LIKE '%' + @Keyword + '%')
    )
    SELECT *,
           (SELECT COUNT(*) FROM Filtered) AS TotalCount
    FROM Filtered
    ORDER BY GraduationYear DESC, FullName ASC
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END;
GO

-- ========================================================================
-- SP: sp_RegisterForEvent
-- Purpose: Atomically register user for event and issue ticket
-- ========================================================================
CREATE OR ALTER PROCEDURE sp_RegisterForEvent
    @EventId    INT,
    @UserId     INT,
    @TicketCode NVARCHAR(50) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        -- Validate event exists and is open
        DECLARE @Capacity INT, @Registered INT, @Price DECIMAL(10,2),
                @EventDate DATETIME2, @Status NVARCHAR(20);

        SELECT
            @Capacity  = Capacity,
            @Price     = TicketPrice,
            @EventDate = EventDate,
            @Status    = Status
        FROM Events WHERE EventId = @EventId;

        IF @Capacity IS NULL
        BEGIN
            RAISERROR('Event not found.', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END

        IF @Status <> 'Scheduled'
        BEGIN
            RAISERROR('Event is not open for registration.', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END

        IF @EventDate <= SYSUTCDATETIME()
        BEGIN
            RAISERROR('Registration has closed (event is in the past).', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END

        -- Count active registrations
        SELECT @Registered = COUNT(*)
        FROM EventRegistrations
        WHERE EventId = @EventId AND CancelledAt IS NULL;

        IF @Registered >= @Capacity
        BEGIN
            RAISERROR('Event is full.', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END

        -- Check for duplicate
        IF EXISTS (SELECT 1 FROM EventRegistrations
                   WHERE EventId = @EventId AND UserId = @UserId AND CancelledAt IS NULL)
        BEGIN
            RAISERROR('User already registered for this event.', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END

        -- Insert registration
        INSERT INTO EventRegistrations (EventId, UserId)
        VALUES (@EventId, @UserId);

        DECLARE @RegId INT = SCOPE_IDENTITY();

        -- Generate unique ticket code
        SET @TicketCode = 'TKT-' + CONVERT(NVARCHAR(36), NEWID());

        -- Insert ticket
        INSERT INTO Tickets (RegistrationId, TicketCode, Price, PaymentStatus)
        VALUES (@RegId, @TicketCode, @Price, CASE WHEN @Price = 0 THEN 'Paid' ELSE 'Paid' END);
        -- (Payment is mocked — always 'Paid' immediately)

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
GO

-- ========================================================================
-- SP: sp_GetMentorshipStats
-- Purpose: Aggregated mentorship numbers for admin dashboard
-- ========================================================================
CREATE OR ALTER PROCEDURE sp_GetMentorshipStats
    @FromDate DATETIME2 = NULL,
    @ToDate   DATETIME2 = NULL
AS
BEGIN
    SET NOCOUNT ON;

    -- Status breakdown
    SELECT
        Status,
        COUNT(*) AS Total
    FROM MentorshipRequests
    WHERE (@FromDate IS NULL OR RequestedAt >= @FromDate)
      AND (@ToDate   IS NULL OR RequestedAt <= @ToDate)
    GROUP BY Status;

    -- Top 5 alumni mentors
    SELECT TOP 5
        u.UserId,
        u.FullName,
        COUNT(*) AS AcceptedRequests
    FROM MentorshipRequests mr
    INNER JOIN Users u ON mr.AlumniUserId = u.UserId
    WHERE mr.Status = 'Accepted'
      AND (@FromDate IS NULL OR mr.RequestedAt >= @FromDate)
      AND (@ToDate   IS NULL OR mr.RequestedAt <= @ToDate)
    GROUP BY u.UserId, u.FullName
    ORDER BY AcceptedRequests DESC;
END;
GO

-- ========================================================================
-- SP: sp_GetDonationReport
-- Purpose: Donation totals by campaign and top donors
-- ========================================================================
CREATE OR ALTER PROCEDURE sp_GetDonationReport
    @CampaignId INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    -- Per-campaign totals
    SELECT
        c.CampaignId,
        c.Title,
        c.TargetAmount,
        c.RaisedAmount,
        COUNT(d.DonationId) AS DonorCount,
        AVG(d.Amount) AS AverageDonation,
        MAX(d.Amount) AS LargestDonation
    FROM DonationCampaigns c
    LEFT JOIN Donations d ON c.CampaignId = d.CampaignId AND d.Status = 'Completed'
    WHERE (@CampaignId IS NULL OR c.CampaignId = @CampaignId)
    GROUP BY c.CampaignId, c.Title, c.TargetAmount, c.RaisedAmount;

    -- Top 10 donors
    SELECT TOP 10
        u.UserId,
        u.FullName,
        SUM(d.Amount) AS TotalDonated,
        COUNT(*)      AS DonationCount
    FROM Donations d
    INNER JOIN Users u ON d.DonorUserId = u.UserId
    WHERE d.Status = 'Completed'
      AND (@CampaignId IS NULL OR d.CampaignId = @CampaignId)
    GROUP BY u.UserId, u.FullName
    ORDER BY TotalDonated DESC;
END;
GO

-- ========================================================================
-- SP: sp_GetEventReport
-- Purpose: Event participation statistics
-- ========================================================================
CREATE OR ALTER PROCEDURE sp_GetEventReport
    @EventId INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        e.EventId,
        e.Title,
        e.EventDate,
        e.Capacity,
        COUNT(er.RegistrationId) AS TotalRegistered,
        SUM(CASE WHEN er.Attended = 1 THEN 1 ELSE 0 END) AS TotalAttended,
        SUM(CASE WHEN er.CancelledAt IS NOT NULL THEN 1 ELSE 0 END) AS TotalCancelled,
        CASE
            WHEN COUNT(er.RegistrationId) = 0 THEN 0
            ELSE CAST(SUM(CASE WHEN er.Attended = 1 THEN 1 ELSE 0 END) * 100.0
                      / COUNT(er.RegistrationId) AS DECIMAL(5,2))
        END AS AttendanceRate
    FROM Events e
    LEFT JOIN EventRegistrations er ON e.EventId = er.EventId
    WHERE (@EventId IS NULL OR e.EventId = @EventId)
    GROUP BY e.EventId, e.Title, e.EventDate, e.Capacity
    ORDER BY e.EventDate DESC;
END;
GO

-- ========================================================================
-- SP: sp_GetAdminDashboard
-- Purpose: System-wide counts for admin dashboard
-- ========================================================================
CREATE OR ALTER PROCEDURE sp_GetAdminDashboard
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        (SELECT COUNT(*) FROM Users WHERE Role = 'Alumni'  AND IsActive = 1) AS TotalAlumni,
        (SELECT COUNT(*) FROM Users WHERE Role = 'Student' AND IsActive = 1) AS TotalStudents,
        (SELECT COUNT(*) FROM Events WHERE Status = 'Scheduled')              AS UpcomingEvents,
        (SELECT COUNT(*) FROM DonationCampaigns WHERE Status = 'Active')      AS ActiveCampaigns,
        (SELECT ISNULL(SUM(Amount), 0) FROM Donations WHERE Status = 'Completed') AS TotalRaised,
        (SELECT COUNT(*) FROM MentorshipRequests WHERE Status = 'Pending')    AS PendingMentorshipRequests;
END;
GO
```

---

## 8. Triggers

Triggers automate business rules at the database level, ensuring consistency even if the application layer has bugs.

```sql
-- ========================================================================
-- 06_triggers.sql
-- ========================================================================
USE AlumniMS;
GO

-- ========================================================================
-- Trigger: tr_Donations_AfterInsert
-- Purpose: Automatically update Campaign.RaisedAmount when a donation is made
-- Fires: AFTER INSERT on Donations
-- ========================================================================
CREATE OR ALTER TRIGGER tr_Donations_AfterInsert
ON Donations
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE c
    SET c.RaisedAmount = c.RaisedAmount + i.Amount,
        c.UpdatedAt    = SYSUTCDATETIME()
    FROM DonationCampaigns c
    INNER JOIN inserted i ON c.CampaignId = i.CampaignId
    WHERE i.Status = 'Completed';
END;
GO

-- ========================================================================
-- Trigger: tr_Donations_AfterUpdate
-- Purpose: If donation is refunded, decrement the campaign's raised amount
-- Fires: AFTER UPDATE on Donations
-- ========================================================================
CREATE OR ALTER TRIGGER tr_Donations_AfterUpdate
ON Donations
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    -- Only act if Status changed from Completed to Refunded
    IF UPDATE(Status)
    BEGIN
        UPDATE c
        SET c.RaisedAmount = c.RaisedAmount - i.Amount,
            c.UpdatedAt    = SYSUTCDATETIME()
        FROM DonationCampaigns c
        INNER JOIN inserted i ON c.CampaignId = i.CampaignId
        INNER JOIN deleted  d ON i.DonationId = d.DonationId
        WHERE d.Status = 'Completed' AND i.Status = 'Refunded';
    END
END;
GO

-- ========================================================================
-- Trigger: tr_Users_BeforeUpdate_Timestamp
-- Purpose: Automatically update UpdatedAt whenever a user record changes
-- Fires: AFTER UPDATE on Users
-- ========================================================================
CREATE OR ALTER TRIGGER tr_Users_AfterUpdate_Timestamp
ON Users
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE u
    SET u.UpdatedAt = SYSUTCDATETIME()
    FROM Users u
    INNER JOIN inserted i ON u.UserId = i.UserId;
END;
GO

-- ========================================================================
-- Trigger: tr_MentorshipRequests_BeforeInsert
-- Purpose: Prevent duplicate active mentorship requests
-- Fires: INSTEAD OF INSERT on MentorshipRequests
-- ========================================================================
CREATE OR ALTER TRIGGER tr_MentorshipRequests_BeforeInsert
ON MentorshipRequests
INSTEAD OF INSERT
AS
BEGIN
    SET NOCOUNT ON;

    -- Check if any inserted row conflicts with existing active request
    IF EXISTS (
        SELECT 1
        FROM inserted i
        INNER JOIN MentorshipRequests m
            ON i.StudentUserId = m.StudentUserId
           AND i.AlumniUserId  = m.AlumniUserId
        WHERE m.Status IN ('Pending', 'Accepted')
    )
    BEGIN
        RAISERROR('Active mentorship request already exists between this student and alumnus.', 16, 1);
        RETURN;
    END

    -- Verify role rules: student must be Student and alumnus must be Alumni
    IF EXISTS (
        SELECT 1
        FROM inserted i
        INNER JOIN Users su ON i.StudentUserId = su.UserId
        INNER JOIN Users au ON i.AlumniUserId  = au.UserId
        WHERE su.Role <> 'Student' OR au.Role <> 'Alumni'
    )
    BEGIN
        RAISERROR('Mentorship: sender must be Student, receiver must be Alumni.', 16, 1);
        RETURN;
    END

    -- Passed checks: proceed with insert
    INSERT INTO MentorshipRequests (StudentUserId, AlumniUserId, Message, Status, RequestedAt)
    SELECT StudentUserId, AlumniUserId, Message, Status, RequestedAt
    FROM inserted;
END;
GO

-- ========================================================================
-- Trigger: tr_Events_BeforeCancel
-- Purpose: When an event is cancelled, cancel all registrations
-- Fires: AFTER UPDATE on Events (when Status becomes 'Cancelled')
-- ========================================================================
CREATE OR ALTER TRIGGER tr_Events_AfterCancel
ON Events
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    IF UPDATE(Status)
    BEGIN
        UPDATE er
        SET er.CancelledAt = SYSUTCDATETIME()
        FROM EventRegistrations er
        INNER JOIN inserted i ON er.EventId = i.EventId
        INNER JOIN deleted  d ON i.EventId = d.EventId
        WHERE d.Status <> 'Cancelled'
          AND i.Status  = 'Cancelled'
          AND er.CancelledAt IS NULL;

        -- Also refund their tickets
        UPDATE t
        SET t.PaymentStatus = 'Refunded',
            t.RefundedAt    = SYSUTCDATETIME()
        FROM Tickets t
        INNER JOIN EventRegistrations er ON t.RegistrationId = er.RegistrationId
        INNER JOIN inserted i ON er.EventId = i.EventId
        INNER JOIN deleted  d ON i.EventId = d.EventId
        WHERE d.Status <> 'Cancelled'
          AND i.Status  = 'Cancelled'
          AND t.PaymentStatus = 'Paid';
    END
END;
GO
```

### 8.1 Trigger Summary

| Trigger | Fires On | Purpose |
|---|---|---|
| `tr_Donations_AfterInsert` | AFTER INSERT Donations | Update campaign RaisedAmount |
| `tr_Donations_AfterUpdate` | AFTER UPDATE Donations | Subtract from RaisedAmount on refund |
| `tr_Users_AfterUpdate_Timestamp` | AFTER UPDATE Users | Auto-set UpdatedAt |
| `tr_MentorshipRequests_BeforeInsert` | INSTEAD OF INSERT | Prevent duplicates, enforce roles |
| `tr_Events_AfterCancel` | AFTER UPDATE Events | Cascade cancellation to registrations and tickets |

---

## 9. Normalization Analysis

This section proves the database is in **Third Normal Form (3NF)** — the standard for relational databases balancing data integrity with reasonable performance.

### 9.1 First Normal Form (1NF)

**Rule:** Each column contains atomic (indivisible) values; no repeating groups or arrays.

**Verification:**
- ✅ Every column stores a single value — no comma-separated lists.
- ✅ No nested records or JSON blobs.
- ✅ Each row uniquely identified by a primary key.

**Example:** Instead of storing `"Ali, Ahmed, Bilal"` as mentors of a student in one column, we use the `MentorshipRequests` table with a separate row per mentor relationship.

### 9.2 Second Normal Form (2NF)

**Rule:** In 1NF, and every non-key column depends on the **whole** primary key (no partial dependencies).

**Verification:**
- ✅ All our tables use **single-column primary keys** (`INT IDENTITY`), so 2NF is trivially satisfied.
- ✅ No composite primary keys exist, so there's no possibility of partial dependency.

**Note:** Even `EventRegistrations` — which semantically joins Event and User — uses a surrogate `RegistrationId` rather than a composite key `(EventId, UserId)`. The uniqueness is still enforced via `UQ_EventReg_EventUser`.

### 9.3 Third Normal Form (3NF)

**Rule:** In 2NF, and no non-key column depends on another non-key column (no transitive dependencies).

**Verification by Table:**

**Users table:**
- Columns: `FullName`, `Email`, `PasswordHash`, `Phone`, `DateOfBirth`, `Role`, `IsActive`, `FailedAttempts`, `LockedUntil`, `CreatedAt`, `UpdatedAt`
- All depend directly on `UserId`.
- No transitive dependencies.
- ✅ **3NF compliant**

**AlumniProfiles table:**
- Columns depend on `AlumniProfileId` (or equivalently `UserId`).
- `DepartmentId` is stored (not `DepartmentName`) — the name is in `Departments` table.
- Without this split, updating a department name would require updating every alumnus row.
- ✅ **3NF compliant**

**StudentProfiles table:**
- Same logic as AlumniProfiles.
- ✅ **3NF compliant**

**Events table:**
- All columns directly describe the event.
- `CreatedByUserId` is a reference, not duplicated user info.
- ✅ **3NF compliant**

**All other tables:** Verified same way — no transitive dependencies exist.

### 9.4 Intentional Denormalization: `DonationCampaigns.RaisedAmount`

`RaisedAmount` is technically a derived value — it equals `SUM(Donations.Amount WHERE CampaignId = X AND Status = 'Completed')`. Storing it violates strict normalization.

**Why we kept it:**
- Dashboards read this value on every load.
- Computing `SUM()` on every read is expensive at scale.
- The **trigger `tr_Donations_AfterInsert`** keeps it synchronized automatically.

**Trade-off:** Slightly slower INSERTs (trigger overhead) for dramatically faster reads. This is an acceptable and common pattern called **materialized aggregate**.

### 9.5 Splitting Profiles (Design Decision)

We split into `Users` + `AlumniProfiles` + `StudentProfiles` instead of one giant `Users` table with many nullable columns.

**Why:**
- Alumni have `GraduationYear`, `JobTitle`, `Industry`.
- Students have `CGPA`, `CurrentSemester`, `EnrollmentYear`.
- Putting all these in `Users` would result in many NULL values.
- Tall nullable columns waste space and muddle the model.

**Result:** Clean separation, 3NF preserved, no data redundancy.

### 9.6 Normalization Summary Table

| Table | 1NF | 2NF | 3NF | Notes |
|---|---|---|---|---|
| Users | ✅ | ✅ | ✅ | |
| Departments | ✅ | ✅ | ✅ | |
| AdminInviteCodes | ✅ | ✅ | ✅ | |
| AlumniProfiles | ✅ | ✅ | ✅ | DepartmentId, not DepartmentName |
| StudentProfiles | ✅ | ✅ | ✅ | |
| MentorshipRequests | ✅ | ✅ | ✅ | |
| Events | ✅ | ✅ | ✅ | |
| EventRegistrations | ✅ | ✅ | ✅ | |
| Tickets | ✅ | ✅ | ✅ | |
| DonationCampaigns | ✅ | ✅ | ⚠️ | RaisedAmount intentionally denormalized |
| Donations | ✅ | ✅ | ✅ | |

---

## 10. Data Dictionary

Complete column-level documentation.

### 10.1 Table: `Users`

| Column | Type | Null | Default | Description |
|---|---|---|---|---|
| UserId | INT IDENTITY | NO | auto | Primary key |
| FullName | NVARCHAR(150) | NO | — | User's full name |
| Email | NVARCHAR(150) | NO | — | Login email (unique) |
| PasswordHash | NVARCHAR(500) | NO | — | BCrypt hash; never plaintext |
| Phone | NVARCHAR(20) | YES | NULL | Contact phone |
| DateOfBirth | DATE | YES | NULL | User's DOB |
| Role | NVARCHAR(20) | NO | — | 'Student', 'Alumni', or 'Admin' |
| IsActive | BIT | NO | 1 | 0 = deactivated account |
| FailedAttempts | INT | NO | 0 | Login failure counter |
| LockedUntil | DATETIME2 | YES | NULL | Lock expiry (if account locked) |
| CreatedAt | DATETIME2 | NO | SYSUTCDATETIME() | Record creation timestamp |
| UpdatedAt | DATETIME2 | YES | NULL | Last modification timestamp |

### 10.2 Table: `Departments`

| Column | Type | Null | Description |
|---|---|---|---|
| DepartmentId | INT IDENTITY | NO | Primary key |
| Name | NVARCHAR(100) | NO | Department name (unique) |
| Description | NVARCHAR(500) | YES | Optional description |
| CreatedAt | DATETIME2 | NO | Creation timestamp |

### 10.3 Table: `AdminInviteCodes`

| Column | Type | Null | Description |
|---|---|---|---|
| CodeId | INT IDENTITY | NO | Primary key |
| Code | NVARCHAR(50) | NO | The invite code string (unique) |
| IsUsed | BIT | NO | Whether the code has been consumed |
| ExpiryDate | DATETIME2 | NO | Code expiration time |
| CreatedByUserId | INT | YES | Admin who created this code |
| UsedByUserId | INT | YES | User who registered using this code |
| CreatedAt | DATETIME2 | NO | When the code was generated |
| UsedAt | DATETIME2 | YES | When the code was consumed |

### 10.4 Table: `AlumniProfiles`

| Column | Type | Null | Description |
|---|---|---|---|
| AlumniProfileId | INT IDENTITY | NO | Primary key |
| UserId | INT | NO | FK to Users (unique — 1:1 with user) |
| DepartmentId | INT | NO | FK to Departments |
| GraduationYear | INT | NO | Year of graduation (1950–current) |
| DegreeProgram | NVARCHAR(100) | NO | e.g., "BS Computer Science" |
| CurrentCompany | NVARCHAR(150) | YES | Current employer |
| JobTitle | NVARCHAR(100) | YES | Current job title |
| Industry | NVARCHAR(100) | YES | e.g., "Software", "Finance" |
| WorkLocation | NVARCHAR(150) | YES | City, Country |
| LinkedInUrl | NVARCHAR(300) | YES | LinkedIn profile URL |
| ShortBio | NVARCHAR(1000) | YES | Bio paragraph |
| CreatedAt | DATETIME2 | NO | Record creation |
| UpdatedAt | DATETIME2 | YES | Last update |

### 10.5 Table: `StudentProfiles`

| Column | Type | Null | Description |
|---|---|---|---|
| StudentProfileId | INT IDENTITY | NO | Primary key |
| UserId | INT | NO | FK to Users (unique) |
| DepartmentId | INT | NO | FK to Departments |
| EnrollmentYear | INT | NO | Year of joining |
| ExpectedGraduationYear | INT | NO | Expected grad year (> EnrollmentYear) |
| DegreeProgram | NVARCHAR(100) | NO | e.g., "BS Software Engineering" |
| CurrentSemester | INT | YES | 1–12 |
| CGPA | DECIMAL(3,2) | YES | 0.00–4.00 |
| ShortBio | NVARCHAR(1000) | YES | Bio paragraph |
| CreatedAt | DATETIME2 | NO | |
| UpdatedAt | DATETIME2 | YES | |

### 10.6 Table: `MentorshipRequests`

| Column | Type | Null | Description |
|---|---|---|---|
| RequestId | INT IDENTITY | NO | Primary key |
| StudentUserId | INT | NO | FK to Users (student sender) |
| AlumniUserId | INT | NO | FK to Users (alumnus receiver) |
| Message | NVARCHAR(500) | NO | Request message (20–500 chars) |
| Status | NVARCHAR(20) | NO | 'Pending'/'Accepted'/'Rejected'/'Cancelled'/'Expired' |
| RequestedAt | DATETIME2 | NO | When request was sent |
| RespondedAt | DATETIME2 | YES | When responded |
| ResponseMessage | NVARCHAR(500) | YES | Alumnus's response text |

### 10.7 Table: `Events`

| Column | Type | Null | Description |
|---|---|---|---|
| EventId | INT IDENTITY | NO | Primary key |
| Title | NVARCHAR(200) | NO | Event title |
| Description | NVARCHAR(2000) | NO | Full description |
| Category | NVARCHAR(50) | NO | 'Reunion'/'Seminar'/'Workshop'/... |
| EventDate | DATETIME2 | NO | Event date and time |
| Location | NVARCHAR(300) | NO | Venue |
| Capacity | INT | NO | Max attendees (≥ 1) |
| TicketPrice | DECIMAL(10,2) | NO | 0 for free events |
| Status | NVARCHAR(20) | NO | 'Scheduled'/'Cancelled'/'Completed' |
| CreatedByUserId | INT | NO | Admin who created |
| CreatedAt | DATETIME2 | NO | |
| UpdatedAt | DATETIME2 | YES | |

### 10.8 Table: `EventRegistrations`

| Column | Type | Null | Description |
|---|---|---|---|
| RegistrationId | INT IDENTITY | NO | Primary key |
| EventId | INT | NO | FK to Events |
| UserId | INT | NO | FK to Users |
| RegisteredAt | DATETIME2 | NO | Registration timestamp |
| Attended | BIT | NO | Marked by admin post-event |
| CancelledAt | DATETIME2 | YES | Cancellation timestamp |

### 10.9 Table: `Tickets`

| Column | Type | Null | Description |
|---|---|---|---|
| TicketId | INT IDENTITY | NO | Primary key |
| RegistrationId | INT | NO | FK to EventRegistrations (unique) |
| TicketCode | NVARCHAR(50) | NO | Unique ticket identifier (GUID-based) |
| Price | DECIMAL(10,2) | NO | Amount paid |
| PaymentStatus | NVARCHAR(20) | NO | 'Paid'/'Refunded'/'Pending' |
| IssuedAt | DATETIME2 | NO | Ticket issuance |
| RefundedAt | DATETIME2 | YES | If refunded |

### 10.10 Table: `DonationCampaigns`

| Column | Type | Null | Description |
|---|---|---|---|
| CampaignId | INT IDENTITY | NO | Primary key |
| Title | NVARCHAR(200) | NO | Campaign title |
| Description | NVARCHAR(2000) | NO | Full description |
| TargetAmount | DECIMAL(14,2) | NO | Fundraising goal (> 0) |
| RaisedAmount | DECIMAL(14,2) | NO | Auto-updated by trigger |
| StartDate | DATETIME2 | NO | Campaign start |
| EndDate | DATETIME2 | NO | Campaign deadline (> StartDate) |
| Status | NVARCHAR(20) | NO | 'Active'/'Closed'/'Cancelled' |
| CreatedByUserId | INT | NO | Admin who created |
| CreatedAt | DATETIME2 | NO | |
| UpdatedAt | DATETIME2 | YES | |

### 10.11 Table: `Donations`

| Column | Type | Null | Description |
|---|---|---|---|
| DonationId | INT IDENTITY | NO | Primary key |
| CampaignId | INT | NO | FK to DonationCampaigns |
| DonorUserId | INT | NO | FK to Users (alumnus) |
| Amount | DECIMAL(14,2) | NO | 0 < Amount ≤ 1,000,000 |
| IsAnonymous | BIT | NO | Hide donor name on public display |
| Status | NVARCHAR(20) | NO | 'Completed'/'Refunded'/'Failed' |
| DonatedAt | DATETIME2 | NO | Transaction timestamp |

---

## 11. Sample Complex Queries

These demonstrate the kinds of queries the application will run. Useful for performance testing and for showing SQL mastery in the project report.

### 11.1 Find Top Alumni Mentors (with JOIN + GROUP BY)

```sql
-- Find top 10 alumni with most accepted mentorships
SELECT TOP 10
    u.UserId,
    u.FullName,
    ap.CurrentCompany,
    ap.JobTitle,
    d.Name AS Department,
    COUNT(mr.RequestId) AS MentorshipCount
FROM Users u
INNER JOIN AlumniProfiles ap  ON u.UserId = ap.UserId
INNER JOIN Departments d       ON ap.DepartmentId = d.DepartmentId
INNER JOIN MentorshipRequests mr ON u.UserId = mr.AlumniUserId
WHERE mr.Status = 'Accepted'
GROUP BY u.UserId, u.FullName, ap.CurrentCompany, ap.JobTitle, d.Name
ORDER BY MentorshipCount DESC;
```

### 11.2 Events with Low Registration (SUBQUERY)

```sql
-- Find upcoming events with less than 50% capacity filled
SELECT
    e.EventId,
    e.Title,
    e.EventDate,
    e.Capacity,
    (SELECT COUNT(*)
     FROM EventRegistrations
     WHERE EventId = e.EventId AND CancelledAt IS NULL) AS Registered,
    CAST((SELECT COUNT(*) FROM EventRegistrations
          WHERE EventId = e.EventId AND CancelledAt IS NULL) * 100.0
         / e.Capacity AS DECIMAL(5,2)) AS FillPercentage
FROM Events e
WHERE e.EventDate > GETDATE()
  AND e.Status = 'Scheduled'
  AND (SELECT COUNT(*) FROM EventRegistrations
       WHERE EventId = e.EventId AND CancelledAt IS NULL) < (e.Capacity / 2)
ORDER BY e.EventDate;
```

### 11.3 Monthly Donation Trend (WINDOW FUNCTION)

```sql
-- Month-by-month donation totals with running total
SELECT
    YEAR(DonatedAt)  AS Year,
    MONTH(DonatedAt) AS Month,
    SUM(Amount)      AS MonthTotal,
    SUM(SUM(Amount)) OVER (ORDER BY YEAR(DonatedAt), MONTH(DonatedAt)) AS RunningTotal
FROM Donations
WHERE Status = 'Completed'
GROUP BY YEAR(DonatedAt), MONTH(DonatedAt)
ORDER BY Year, Month;
```

### 11.4 Alumni Who Never Received a Mentorship Request (LEFT JOIN + NULL)

```sql
-- Find alumni who have never received any mentorship request
SELECT
    u.UserId,
    u.FullName,
    ap.GraduationYear,
    d.Name AS Department
FROM Users u
INNER JOIN AlumniProfiles ap ON u.UserId = ap.UserId
INNER JOIN Departments d      ON ap.DepartmentId = d.DepartmentId
LEFT JOIN MentorshipRequests mr ON u.UserId = mr.AlumniUserId
WHERE u.Role = 'Alumni'
  AND mr.RequestId IS NULL
ORDER BY ap.GraduationYear DESC;
```

### 11.5 Department-wise Engagement Report (MULTIPLE JOINS + AGGREGATES)

```sql
-- For each department: total alumni, total students, mentorship stats
SELECT
    d.Name AS Department,
    (SELECT COUNT(*) FROM AlumniProfiles WHERE DepartmentId = d.DepartmentId) AS TotalAlumni,
    (SELECT COUNT(*) FROM StudentProfiles WHERE DepartmentId = d.DepartmentId) AS TotalStudents,
    (SELECT COUNT(*)
     FROM MentorshipRequests mr
     INNER JOIN StudentProfiles sp ON mr.StudentUserId = sp.UserId
     WHERE sp.DepartmentId = d.DepartmentId AND mr.Status = 'Accepted') AS AcceptedMentorships,
    (SELECT ISNULL(SUM(don.Amount), 0)
     FROM Donations don
     INNER JOIN AlumniProfiles ap ON don.DonorUserId = ap.UserId
     WHERE ap.DepartmentId = d.DepartmentId AND don.Status = 'Completed') AS TotalDonated
FROM Departments d
ORDER BY TotalAlumni DESC;
```

### 11.6 Top Donors with Rank (RANK WINDOW FUNCTION)

```sql
-- Rank alumni by total amount donated
SELECT
    u.UserId,
    u.FullName,
    SUM(d.Amount)    AS TotalDonated,
    COUNT(*)         AS DonationCount,
    RANK() OVER (ORDER BY SUM(d.Amount) DESC) AS DonorRank
FROM Donations d
INNER JOIN Users u ON d.DonorUserId = u.UserId
WHERE d.Status = 'Completed'
GROUP BY u.UserId, u.FullName;
```

### 11.7 Event Attendance Rate per Category (CASE + GROUP BY)

```sql
-- For each event category, compute average attendance rate
SELECT
    e.Category,
    COUNT(DISTINCT e.EventId) AS EventCount,
    SUM(CASE WHEN er.Attended = 1 THEN 1 ELSE 0 END) AS TotalAttended,
    SUM(CASE WHEN er.CancelledAt IS NULL THEN 1 ELSE 0 END) AS TotalRegistered,
    CAST(
        SUM(CASE WHEN er.Attended = 1 THEN 1 ELSE 0 END) * 100.0
        / NULLIF(SUM(CASE WHEN er.CancelledAt IS NULL THEN 1 ELSE 0 END), 0)
        AS DECIMAL(5,2)
    ) AS AverageAttendanceRate
FROM Events e
LEFT JOIN EventRegistrations er ON e.EventId = er.EventId
WHERE e.Status = 'Completed'
GROUP BY e.Category;
```

### 11.8 Students Matched with Suggested Alumni (Correlated Subquery)

```sql
-- For each student, suggest 5 alumni in the same department
SELECT
    s.UserId  AS StudentId,
    s.FullName AS StudentName,
    a.UserId   AS AlumniId,
    a.FullName AS AlumniName,
    ap.CurrentCompany,
    ap.JobTitle,
    ap.GraduationYear
FROM Users s
INNER JOIN StudentProfiles sp ON s.UserId = sp.UserId
CROSS APPLY (
    SELECT TOP 5 au.UserId, au.FullName,
           alp.CurrentCompany, alp.JobTitle, alp.GraduationYear
    FROM Users au
    INNER JOIN AlumniProfiles alp ON au.UserId = alp.UserId
    WHERE alp.DepartmentId = sp.DepartmentId
      AND au.Role = 'Alumni'
    ORDER BY alp.GraduationYear DESC
) AS a
INNER JOIN AlumniProfiles ap ON a.UserId = ap.UserId
WHERE s.Role = 'Student';
```

### 11.9 Find Duplicate Email Attempts (Potential Abuse)

```sql
-- Find emails used multiple times in failed registrations (using audit log — future extension)
-- For now, simpler: users with most failed login attempts
SELECT
    Email,
    FailedAttempts,
    LockedUntil,
    CASE WHEN LockedUntil > GETDATE() THEN 'Locked' ELSE 'Active' END AS AccountStatus
FROM Users
WHERE FailedAttempts > 3
ORDER BY FailedAttempts DESC;
```

### 11.10 Campaign Performance Summary

```sql
-- Campaign progress with projected completion
SELECT
    c.Title,
    c.TargetAmount,
    c.RaisedAmount,
    CAST(c.RaisedAmount * 100.0 / c.TargetAmount AS DECIMAL(5,2)) AS ProgressPct,
    c.EndDate,
    DATEDIFF(DAY, GETDATE(), c.EndDate) AS DaysRemaining,
    (SELECT COUNT(*) FROM Donations WHERE CampaignId = c.CampaignId) AS DonationCount,
    CASE
        WHEN c.RaisedAmount >= c.TargetAmount THEN 'Goal Reached'
        WHEN c.EndDate < GETDATE() THEN 'Expired'
        WHEN DATEDIFF(DAY, GETDATE(), c.EndDate) < 7 THEN 'Ending Soon'
        ELSE 'On Track'
    END AS Outlook
FROM DonationCampaigns c
WHERE c.Status = 'Active'
ORDER BY ProgressPct DESC;
```

---

## 12. Seed Data

Initial data required for the system to function.

```sql
-- ========================================================================
-- 07_seed_data.sql
-- Purpose: Populate lookup tables and create the first admin
-- ========================================================================
USE AlumniMS;
GO

-- ----------------- Departments -----------------
INSERT INTO Departments (Name, Description) VALUES
('Computer Science',        'Department of Computer Science and IT'),
('Software Engineering',    'Department of Software Engineering'),
('Electrical Engineering',  'Department of Electrical Engineering'),
('Mechanical Engineering',  'Department of Mechanical Engineering'),
('Business Administration', 'School of Business'),
('Economics',               'Department of Economics'),
('Mathematics',             'Department of Mathematics'),
('Physics',                 'Department of Physics');
GO

-- ----------------- First Admin (bootstrap) -----------------
-- Password: Admin@123 (hashed using BCrypt equivalent — regenerate with your backend)
-- IMPORTANT: Replace the hash below with one generated by your actual backend code
--
-- Example C# code to generate:
--   BCrypt.Net.BCrypt.HashPassword("Admin@123");
--
INSERT INTO Users (FullName, Email, PasswordHash, Phone, Role, IsActive)
VALUES (
    'System Administrator',
    'admin@university.edu',
    '$2a$11$REPLACE_THIS_WITH_ACTUAL_BCRYPT_HASH',  -- TODO: Replace
    '+923001234567',
    'Admin',
    1
);
GO

-- ----------------- Initial admin invite codes -----------------
-- Generate 3 codes valid for 90 days for onboarding additional admins
DECLARE @AdminId INT = (SELECT UserId FROM Users WHERE Email = 'admin@university.edu');

INSERT INTO AdminInviteCodes (Code, ExpiryDate, CreatedByUserId) VALUES
('ADMIN-INIT-001', DATEADD(DAY, 90, GETDATE()), @AdminId),
('ADMIN-INIT-002', DATEADD(DAY, 90, GETDATE()), @AdminId),
('ADMIN-INIT-003', DATEADD(DAY, 90, GETDATE()), @AdminId);
GO
```

### 12.1 Sample Demo Data (Optional)

```sql
-- ========================================================================
-- 08_sample_data.sql (OPTIONAL - for demo/testing only)
-- ========================================================================
USE AlumniMS;
GO

-- Sample alumni (passwords all = "Password@123" — replace hash)
-- [Add 10-20 INSERT statements here for demo purposes]

-- Sample students
-- [Add 10-15 INSERT statements here]

-- Sample events
INSERT INTO Events (Title, Description, Category, EventDate, Location, Capacity, TicketPrice, CreatedByUserId)
VALUES
('Annual Alumni Reunion 2026',
 'Join us for our annual reunion celebrating all graduating classes.',
 'Reunion', DATEADD(MONTH, 2, GETDATE()), 'Main Auditorium, University Campus',
 500, 0, 1),

('Tech Industry Panel Discussion',
 'Panel with senior engineers from top tech companies discussing the future of AI.',
 'Seminar', DATEADD(DAY, 20, GETDATE()), 'Lecture Hall A', 150, 500, 1),

('Entrepreneurship Workshop',
 'Hands-on workshop for aspiring entrepreneurs.',
 'Workshop', DATEADD(DAY, 45, GETDATE()), 'Business Building, Room 301',
 50, 1000, 1);
GO

-- Sample donation campaigns
INSERT INTO DonationCampaigns (Title, Description, TargetAmount, EndDate, CreatedByUserId)
VALUES
('New Library Wing', 'Help us build a new wing for the university library.',
 5000000, DATEADD(MONTH, 6, GETDATE()), 1),
('Scholarship Fund 2026', 'Scholarships for deserving students.',
 2000000, DATEADD(MONTH, 12, GETDATE()), 1);
GO
```

---

## 13. Backup & Restore Strategy

### 13.1 Backup Schedule (Recommended)

| Backup Type | Frequency | Retention |
|---|---|---|
| Full backup | Weekly | 4 weeks |
| Differential backup | Daily | 7 days |
| Transaction log backup | Every 4 hours | 24 hours |

> **For academic project:** A single full backup at end-of-semester is sufficient.

### 13.2 Manual Backup Command

```sql
-- Full backup
BACKUP DATABASE AlumniMS
TO DISK = 'C:\Backups\AlumniMS_Full.bak'
WITH FORMAT, INIT, NAME = 'AlumniMS Full Backup',
     COMPRESSION, STATS = 10;
GO
```

### 13.3 Restore Command

```sql
-- Restore from backup
RESTORE DATABASE AlumniMS
FROM DISK = 'C:\Backups\AlumniMS_Full.bak'
WITH REPLACE, RECOVERY, STATS = 10;
GO
```

### 13.4 Scripted Backup (for demo purposes)

```sql
-- Schema-only export for submission
-- Run this in SSMS:
-- Right-click DB → Tasks → Generate Scripts → choose options → save .sql file
```

---

## 14. Execution Order

Run the SQL scripts in this exact order when setting up the database:

```
1. 01_create_database.sql    — Creates AlumniMS database
2. 02_schema.sql             — Creates all tables
3. 03_indexes.sql            — Creates indexes
4. 04_views.sql              — Creates views
5. 05_stored_procedures.sql  — Creates stored procedures
6. 06_triggers.sql           — Creates triggers
7. 07_seed_data.sql          — Inserts departments + first admin + invite codes
8. 08_sample_data.sql        — (Optional) Demo data
```

### 14.1 Quick Setup Command (PowerShell/CMD)

```powershell
# Run all scripts sequentially via sqlcmd
sqlcmd -S localhost -E -i 01_create_database.sql
sqlcmd -S localhost -E -d AlumniMS -i 02_schema.sql
sqlcmd -S localhost -E -d AlumniMS -i 03_indexes.sql
sqlcmd -S localhost -E -d AlumniMS -i 04_views.sql
sqlcmd -S localhost -E -d AlumniMS -i 05_stored_procedures.sql
sqlcmd -S localhost -E -d AlumniMS -i 06_triggers.sql
sqlcmd -S localhost -E -d AlumniMS -i 07_seed_data.sql
sqlcmd -S localhost -E -d AlumniMS -i 08_sample_data.sql
```

---

## 15. Appendix

### 15.1 Database Size Estimation

Estimated storage for 1 year of operation at moderate scale:

| Table | Rows Estimate | Size per Row | Total |
|---|---|---|---|
| Users | 2,000 | 1 KB | 2 MB |
| AlumniProfiles | 1,500 | 2 KB | 3 MB |
| StudentProfiles | 500 | 1.5 KB | 750 KB |
| MentorshipRequests | 5,000 | 1 KB | 5 MB |
| Events | 50 | 3 KB | 150 KB |
| EventRegistrations | 5,000 | 0.5 KB | 2.5 MB |
| Tickets | 5,000 | 0.5 KB | 2.5 MB |
| DonationCampaigns | 20 | 3 KB | 60 KB |
| Donations | 2,000 | 0.5 KB | 1 MB |
| **Total (with indexes ~30% overhead)** | | | **~25 MB** |

SQL Server Express's 10 GB limit is comfortably sufficient.

### 15.2 Key Design Rationale Summary

| Decision | Reasoning |
|---|---|
| Surrogate PKs (IDENTITY) | Stable, performant, insulates from business changes |
| Separate profile tables | Avoids sparse nullable columns; cleaner schema |
| NVARCHAR for text | Unicode support for Pakistani names, future internationalization |
| DECIMAL for money | Never FLOAT — avoids rounding errors |
| DATETIME2 over DATETIME | Better precision, required for modern apps |
| Triggers for aggregates | Keeps `RaisedAmount` consistent without app-layer bugs |
| Check constraints | Last line of defense against bad data |
| Cascade delete on profiles | Deleting user removes orphaned profile |
| No cascade on FKs referencing Users from transactions | Preserves historical data even if user is deactivated |

### 15.3 Future Enhancements (Out of Scope)

Documented here for completeness — NOT to be implemented in this project:
- Multi-language support (localization tables)
- Soft delete on Users (currently only IsActive flag)
- Audit log table for all CRUD operations
- Password reset tokens table
- Email verification tokens table
- Notifications table for in-app messages

---

## 16. Revision History

| Version | Date | Author | Changes |
|---|---|---|---|
| 1.0 | April 2026 | Team (Zuhar, Ramsha, Waleed) | Initial DDD |

---

## 17. Approval

- **Prepared by:** Zuhar Faisal, Ramsha Khalid, M. Waleed
- **Course:** CL2005 — Database Systems
- **Instructor:** Sir Umar Farooq
- **Semester:** Spring 2026
- **Section:** BSE-4A

---

*End of Database Design Document*
