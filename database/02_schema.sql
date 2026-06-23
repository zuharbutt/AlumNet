-- ============================================================
-- 02_schema.sql
-- Purpose: Create all 11 tables with constraints
-- Run AFTER: 01_create_database.sql
-- Execute in order — tables with no FK deps first
-- ============================================================

USE AlumniMS;
GO

-- ============================================================
-- Table 1: Departments (lookup, no FK deps)
-- ============================================================
CREATE TABLE Departments (
    DepartmentId    INT IDENTITY(1,1)   NOT NULL,
    Name            NVARCHAR(100)       NOT NULL,
    Description     NVARCHAR(500)       NULL,
    CreatedAt       DATETIME2           NOT NULL DEFAULT SYSUTCDATETIME(),

    CONSTRAINT PK_Departments       PRIMARY KEY (DepartmentId),
    CONSTRAINT UQ_Departments_Name  UNIQUE (Name)
);
GO

-- ============================================================
-- Table 2: Users (depends on nothing)
-- ============================================================
CREATE TABLE Users (
    UserId          INT IDENTITY(1,1)   NOT NULL,
    FullName        NVARCHAR(150)       NOT NULL,
    Email           NVARCHAR(150)       NOT NULL,
    PasswordHash    NVARCHAR(500)       NOT NULL,
    Phone           NVARCHAR(20)        NULL,
    DateOfBirth     DATE                NULL,
    Role            NVARCHAR(20)        NOT NULL,
    IsActive        BIT                 NOT NULL DEFAULT 1,
    FailedAttempts  INT                 NOT NULL DEFAULT 0,
    LockedUntil     DATETIME2           NULL,
    CreatedAt       DATETIME2           NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt       DATETIME2           NULL,

    CONSTRAINT PK_Users             PRIMARY KEY (UserId),
    CONSTRAINT UQ_Users_Email       UNIQUE (Email),
    CONSTRAINT CK_Users_Role        CHECK (Role IN ('Student', 'Alumni', 'Admin')),
    CONSTRAINT CK_Users_Email       CHECK (Email LIKE '%_@_%._%'),
    CONSTRAINT CK_Users_Attempts    CHECK (FailedAttempts >= 0)
);
GO

-- ============================================================
-- Table 3: AdminInviteCodes (depends on Users)
-- ============================================================
CREATE TABLE AdminInviteCodes (
    CodeId              INT IDENTITY(1,1)   NOT NULL,
    Code                NVARCHAR(50)        NOT NULL,
    IsUsed              BIT                 NOT NULL DEFAULT 0,
    ExpiryDate          DATETIME2           NOT NULL,
    CreatedByUserId     INT                 NULL,
    UsedByUserId        INT                 NULL,
    CreatedAt           DATETIME2           NOT NULL DEFAULT SYSUTCDATETIME(),
    UsedAt              DATETIME2           NULL,

    CONSTRAINT PK_InviteCodes               PRIMARY KEY (CodeId),
    CONSTRAINT UQ_InviteCodes_Code          UNIQUE (Code),
    CONSTRAINT FK_InviteCodes_CreatedBy     FOREIGN KEY (CreatedByUserId)
        REFERENCES Users(UserId) ON DELETE NO ACTION,
    CONSTRAINT FK_InviteCodes_UsedBy        FOREIGN KEY (UsedByUserId)
        REFERENCES Users(UserId) ON DELETE NO ACTION,
    CONSTRAINT CK_InviteCodes_Expiry        CHECK (ExpiryDate > CreatedAt),
    CONSTRAINT CK_InviteCodes_UsedState     CHECK (
        (IsUsed = 0 AND UsedByUserId IS NULL AND UsedAt IS NULL)
        OR
        (IsUsed = 1 AND UsedByUserId IS NOT NULL AND UsedAt IS NOT NULL)
    )
);
GO

-- ============================================================
-- Table 4: AlumniProfiles (depends on Users, Departments)
-- ============================================================
CREATE TABLE AlumniProfiles (
    AlumniProfileId INT IDENTITY(1,1)   NOT NULL,
    UserId          INT                 NOT NULL,
    DepartmentId    INT                 NOT NULL,
    GraduationYear  INT                 NOT NULL,
    DegreeProgram   NVARCHAR(100)       NOT NULL,
    CurrentCompany  NVARCHAR(150)       NULL,
    JobTitle        NVARCHAR(100)       NULL,
    Industry        NVARCHAR(100)       NULL,
    WorkLocation    NVARCHAR(150)       NULL,
    LinkedInUrl     NVARCHAR(300)       NULL,
    ShortBio        NVARCHAR(1000)      NULL,
    CreatedAt       DATETIME2           NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt       DATETIME2           NULL,

    CONSTRAINT PK_AlumniProfiles            PRIMARY KEY (AlumniProfileId),
    CONSTRAINT UQ_AlumniProfiles_User       UNIQUE (UserId),
    CONSTRAINT FK_Alumni_User               FOREIGN KEY (UserId)
        REFERENCES Users(UserId) ON DELETE CASCADE,
    CONSTRAINT FK_Alumni_Department         FOREIGN KEY (DepartmentId)
        REFERENCES Departments(DepartmentId) ON DELETE NO ACTION,
    CONSTRAINT CK_Alumni_GradYear           CHECK (GraduationYear BETWEEN 1950 AND 2100),
    CONSTRAINT CK_Alumni_LinkedIn           CHECK (
        LinkedInUrl IS NULL OR LinkedInUrl LIKE 'http%linkedin.com%'
    )
);
GO

-- ============================================================
-- Table 5: StudentProfiles (depends on Users, Departments)
-- ============================================================
CREATE TABLE StudentProfiles (
    StudentProfileId        INT IDENTITY(1,1)   NOT NULL,
    UserId                  INT                 NOT NULL,
    DepartmentId            INT                 NOT NULL,
    EnrollmentYear          INT                 NOT NULL,
    ExpectedGraduationYear  INT                 NOT NULL,
    DegreeProgram           NVARCHAR(100)       NOT NULL,
    CurrentSemester         INT                 NULL,
    CGPA                    DECIMAL(3,2)        NULL,
    ShortBio                NVARCHAR(1000)      NULL,
    CreatedAt               DATETIME2           NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt               DATETIME2           NULL,

    CONSTRAINT PK_StudentProfiles           PRIMARY KEY (StudentProfileId),
    CONSTRAINT UQ_StudentProfiles_User      UNIQUE (UserId),
    CONSTRAINT FK_Student_User              FOREIGN KEY (UserId)
        REFERENCES Users(UserId) ON DELETE CASCADE,
    CONSTRAINT FK_Student_Department        FOREIGN KEY (DepartmentId)
        REFERENCES Departments(DepartmentId) ON DELETE NO ACTION,
    CONSTRAINT CK_Student_EnrollYear        CHECK (EnrollmentYear BETWEEN 2000 AND 2100),
    CONSTRAINT CK_Student_GradYear          CHECK (ExpectedGraduationYear > EnrollmentYear),
    CONSTRAINT CK_Student_CGPA              CHECK (CGPA IS NULL OR (CGPA BETWEEN 0.00 AND 4.00)),
    CONSTRAINT CK_Student_Semester          CHECK (CurrentSemester IS NULL OR CurrentSemester BETWEEN 1 AND 12)
);
GO

-- ============================================================
-- Table 6: MentorshipRequests (depends on Users)
-- ============================================================
CREATE TABLE MentorshipRequests (
    RequestId           INT IDENTITY(1,1)   NOT NULL,
    StudentUserId       INT                 NOT NULL,
    AlumniUserId        INT                 NOT NULL,
    Message             NVARCHAR(500)       NOT NULL,
    Status              NVARCHAR(20)        NOT NULL DEFAULT 'Pending',
    RequestedAt         DATETIME2           NOT NULL DEFAULT SYSUTCDATETIME(),
    RespondedAt         DATETIME2           NULL,
    ResponseMessage     NVARCHAR(500)       NULL,

    CONSTRAINT PK_MentorshipRequests    PRIMARY KEY (RequestId),
    CONSTRAINT FK_Mentorship_Student    FOREIGN KEY (StudentUserId)
        REFERENCES Users(UserId) ON DELETE NO ACTION,
    CONSTRAINT FK_Mentorship_Alumni     FOREIGN KEY (AlumniUserId)
        REFERENCES Users(UserId) ON DELETE NO ACTION,
    CONSTRAINT CK_Mentorship_Status     CHECK (Status IN ('Pending', 'Accepted', 'Rejected', 'Cancelled', 'Expired')),
    CONSTRAINT CK_Mentorship_Users      CHECK (StudentUserId <> AlumniUserId),
    CONSTRAINT CK_Mentorship_MsgLen     CHECK (LEN(Message) BETWEEN 20 AND 500),
    CONSTRAINT CK_Mentorship_Response   CHECK (
        (Status = 'Pending' AND RespondedAt IS NULL)
        OR
        (Status IN ('Accepted', 'Rejected', 'Cancelled', 'Expired') AND RespondedAt IS NOT NULL)
    )
);
GO

-- ============================================================
-- Table 7: Events (depends on Users)
-- ============================================================
CREATE TABLE Events (
    EventId             INT IDENTITY(1,1)   NOT NULL,
    Title               NVARCHAR(200)       NOT NULL,
    Description         NVARCHAR(2000)      NOT NULL,
    Category            NVARCHAR(50)        NOT NULL,
    EventDate           DATETIME2           NOT NULL,
    Location            NVARCHAR(300)       NOT NULL,
    Capacity            INT                 NOT NULL,
    TicketPrice         DECIMAL(10,2)       NOT NULL DEFAULT 0,
    Status              NVARCHAR(20)        NOT NULL DEFAULT 'Scheduled',
    CreatedByUserId     INT                 NOT NULL,
    CreatedAt           DATETIME2           NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt           DATETIME2           NULL,

    CONSTRAINT PK_Events            PRIMARY KEY (EventId),
    CONSTRAINT FK_Events_Creator    FOREIGN KEY (CreatedByUserId)
        REFERENCES Users(UserId) ON DELETE NO ACTION,
    CONSTRAINT CK_Events_Category   CHECK (Category IN ('Reunion', 'Seminar', 'Workshop', 'Networking', 'Conference', 'Other')),
    CONSTRAINT CK_Events_Status     CHECK (Status IN ('Scheduled', 'Cancelled', 'Completed')),
    CONSTRAINT CK_Events_Capacity   CHECK (Capacity >= 1),
    CONSTRAINT CK_Events_Price      CHECK (TicketPrice >= 0)
);
GO

-- ============================================================
-- Table 8: EventRegistrations (depends on Events, Users)
-- ============================================================
CREATE TABLE EventRegistrations (
    RegistrationId  INT IDENTITY(1,1)   NOT NULL,
    EventId         INT                 NOT NULL,
    UserId          INT                 NOT NULL,
    RegisteredAt    DATETIME2           NOT NULL DEFAULT SYSUTCDATETIME(),
    Attended        BIT                 NOT NULL DEFAULT 0,
    CancelledAt     DATETIME2           NULL,

    CONSTRAINT PK_EventRegistrations    PRIMARY KEY (RegistrationId),
    CONSTRAINT FK_EventReg_Event        FOREIGN KEY (EventId)
        REFERENCES Events(EventId) ON DELETE CASCADE,
    CONSTRAINT FK_EventReg_User         FOREIGN KEY (UserId)
        REFERENCES Users(UserId) ON DELETE NO ACTION,
    CONSTRAINT UQ_EventReg_EventUser    UNIQUE (EventId, UserId)
);
GO

-- ============================================================
-- Table 9: Tickets (depends on EventRegistrations)
-- ============================================================
CREATE TABLE Tickets (
    TicketId        INT IDENTITY(1,1)   NOT NULL,
    RegistrationId  INT                 NOT NULL,
    TicketCode      NVARCHAR(50)        NOT NULL,
    Price           DECIMAL(10,2)       NOT NULL,
    PaymentStatus   NVARCHAR(20)        NOT NULL DEFAULT 'Paid',
    IssuedAt        DATETIME2           NOT NULL DEFAULT SYSUTCDATETIME(),
    RefundedAt      DATETIME2           NULL,

    CONSTRAINT PK_Tickets           PRIMARY KEY (TicketId),
    CONSTRAINT UQ_Tickets_Reg       UNIQUE (RegistrationId),
    CONSTRAINT UQ_Tickets_Code      UNIQUE (TicketCode),
    CONSTRAINT FK_Tickets_Reg       FOREIGN KEY (RegistrationId)
        REFERENCES EventRegistrations(RegistrationId) ON DELETE CASCADE,
    CONSTRAINT CK_Tickets_Status    CHECK (PaymentStatus IN ('Paid', 'Refunded', 'Pending')),
    CONSTRAINT CK_Tickets_Price     CHECK (Price >= 0)
);
GO

-- ============================================================
-- Table 10: DonationCampaigns (depends on Users)
-- ============================================================
CREATE TABLE DonationCampaigns (
    CampaignId      INT IDENTITY(1,1)   NOT NULL,
    Title           NVARCHAR(200)       NOT NULL,
    Description     NVARCHAR(2000)      NOT NULL,
    TargetAmount    DECIMAL(14,2)       NOT NULL,
    RaisedAmount    DECIMAL(14,2)       NOT NULL DEFAULT 0,
    StartDate       DATETIME2           NOT NULL DEFAULT SYSUTCDATETIME(),
    EndDate         DATETIME2           NOT NULL,
    Status          NVARCHAR(20)        NOT NULL DEFAULT 'Active',
    CreatedByUserId INT                 NOT NULL,
    CreatedAt       DATETIME2           NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt       DATETIME2           NULL,

    CONSTRAINT PK_DonationCampaigns PRIMARY KEY (CampaignId),
    CONSTRAINT FK_Camp_Creator      FOREIGN KEY (CreatedByUserId)
        REFERENCES Users(UserId) ON DELETE NO ACTION,
    CONSTRAINT CK_Camp_Status       CHECK (Status IN ('Active', 'Closed', 'Cancelled')),
    CONSTRAINT CK_Camp_Target       CHECK (TargetAmount > 0),
    CONSTRAINT CK_Camp_Raised       CHECK (RaisedAmount >= 0),
    CONSTRAINT CK_Camp_Dates        CHECK (EndDate > StartDate)
);
GO

-- ============================================================
-- Table 11: Donations (depends on DonationCampaigns, Users)
-- ============================================================
CREATE TABLE Donations (
    DonationId      INT IDENTITY(1,1)   NOT NULL,
    CampaignId      INT                 NOT NULL,
    DonorUserId     INT                 NOT NULL,
    Amount          DECIMAL(14,2)       NOT NULL,
    IsAnonymous     BIT                 NOT NULL DEFAULT 0,
    Status          NVARCHAR(20)        NOT NULL DEFAULT 'Completed',
    DonatedAt       DATETIME2           NOT NULL DEFAULT SYSUTCDATETIME(),

    CONSTRAINT PK_Donations     PRIMARY KEY (DonationId),
    CONSTRAINT FK_Don_Campaign  FOREIGN KEY (CampaignId)
        REFERENCES DonationCampaigns(CampaignId) ON DELETE NO ACTION,
    CONSTRAINT FK_Don_Donor     FOREIGN KEY (DonorUserId)
        REFERENCES Users(UserId) ON DELETE NO ACTION,
    CONSTRAINT CK_Don_Status    CHECK (Status IN ('Completed', 'Refunded', 'Failed')),
    CONSTRAINT CK_Don_Amount    CHECK (Amount > 0 AND Amount <= 1000000)
);
GO

PRINT 'All 11 tables created successfully.';
GO
