-- ============================================================
-- 03_indexes.sql
-- Purpose: Non-clustered indexes for performance
-- Run AFTER: 02_schema.sql
-- Note: PKs are already clustered indexes. UQ constraints
--       already create unique non-clustered indexes.
-- ============================================================

USE AlumniMS;
GO

SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

-- ─── Users ───────────────────────────────────────────────────
-- UQ_Users_Email already indexes email for login lookups.
CREATE INDEX IX_Users_Role
    ON Users(Role);

CREATE INDEX IX_Users_IsActive
    ON Users(IsActive)
    WHERE IsActive = 1;      -- filtered: only active accounts

-- ─── AlumniProfiles ──────────────────────────────────────────
CREATE INDEX IX_Alumni_Department
    ON AlumniProfiles(DepartmentId);

CREATE INDEX IX_Alumni_GradYear
    ON AlumniProfiles(GraduationYear);

CREATE INDEX IX_Alumni_Industry
    ON AlumniProfiles(Industry);

CREATE INDEX IX_Alumni_WorkLocation
    ON AlumniProfiles(WorkLocation);

-- Composite: most common combined filter in directory search
CREATE INDEX IX_Alumni_Dept_Year
    ON AlumniProfiles(DepartmentId, GraduationYear);

-- ─── StudentProfiles ─────────────────────────────────────────
CREATE INDEX IX_Student_Department
    ON StudentProfiles(DepartmentId);

-- ─── MentorshipRequests ──────────────────────────────────────
CREATE INDEX IX_Mentorship_Alumni_Status
    ON MentorshipRequests(AlumniUserId, Status);

CREATE INDEX IX_Mentorship_Student_Status
    ON MentorshipRequests(StudentUserId, Status);

CREATE INDEX IX_Mentorship_RequestedAt
    ON MentorshipRequests(RequestedAt DESC);

-- ─── Events ──────────────────────────────────────────────────
CREATE INDEX IX_Events_Date_Status
    ON Events(EventDate, Status);

CREATE INDEX IX_Events_Category
    ON Events(Category);

CREATE INDEX IX_Events_CreatedBy
    ON Events(CreatedByUserId);

-- ─── EventRegistrations ──────────────────────────────────────
-- UQ_EventReg_EventUser already covers (EventId, UserId).
CREATE INDEX IX_EventReg_User
    ON EventRegistrations(UserId);

CREATE INDEX IX_EventReg_Event
    ON EventRegistrations(EventId);

-- ─── Tickets ─────────────────────────────────────────────────
-- UQ_Tickets_Code already indexes TicketCode.
CREATE INDEX IX_Tickets_PaymentStatus
    ON Tickets(PaymentStatus);

-- ─── DonationCampaigns ───────────────────────────────────────
CREATE INDEX IX_Camp_Status_EndDate
    ON DonationCampaigns(Status, EndDate);

-- ─── Donations ───────────────────────────────────────────────
CREATE INDEX IX_Don_Campaign
    ON Donations(CampaignId);

CREATE INDEX IX_Don_Donor
    ON Donations(DonorUserId);

CREATE INDEX IX_Don_DonatedAt
    ON Donations(DonatedAt DESC);

-- ─── AdminInviteCodes ────────────────────────────────────────
-- UQ_InviteCodes_Code already indexes Code.
CREATE INDEX IX_InviteCodes_IsUsed
    ON AdminInviteCodes(IsUsed)
    WHERE IsUsed = 0;         -- filtered: only unused codes

GO

PRINT 'All indexes created successfully.';
GO
