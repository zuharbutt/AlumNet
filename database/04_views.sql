-- ============================================================
-- 04_views.sql
-- Purpose: Reusable views that simplify complex joins
-- Run AFTER: 02_schema.sql
-- ============================================================

USE AlumniMS;
GO

-- ============================================================
-- View: vw_AlumniDirectory
-- Public-facing alumni info for directory search.
-- Hides nothing here — contact visibility is enforced by API.
-- ============================================================
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
INNER JOIN Departments    d  ON ap.DepartmentId = d.DepartmentId
WHERE u.Role = 'Alumni' AND u.IsActive = 1;
GO

-- ============================================================
-- View: vw_StudentDirectory
-- Student info for admin oversight.
-- ============================================================
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
INNER JOIN Departments     d  ON sp.DepartmentId = d.DepartmentId
WHERE u.Role = 'Student' AND u.IsActive = 1;
GO

-- ============================================================
-- View: vw_UpcomingEvents
-- Scheduled future events with live registration counts.
-- ============================================================
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
    COUNT(er.RegistrationId)                            AS RegisteredCount,
    (e.Capacity - COUNT(er.RegistrationId))             AS SeatsRemaining,
    CASE
        WHEN COUNT(er.RegistrationId) >= e.Capacity THEN 1
        ELSE 0
    END                                                 AS IsFull
FROM Events e
LEFT JOIN EventRegistrations er
    ON e.EventId = er.EventId AND er.CancelledAt IS NULL
WHERE e.EventDate > SYSUTCDATETIME() AND e.Status = 'Scheduled'
GROUP BY
    e.EventId, e.Title, e.Description, e.Category,
    e.EventDate, e.Location, e.Capacity, e.TicketPrice, e.Status;
GO

-- ============================================================
-- View: vw_ActiveCampaigns
-- Active donation campaigns with progress metrics.
-- ============================================================
CREATE OR ALTER VIEW vw_ActiveCampaigns
AS
SELECT
    c.CampaignId,
    c.Title,
    c.Description,
    c.TargetAmount,
    c.RaisedAmount,
    CAST((c.RaisedAmount * 100.0 / c.TargetAmount) AS DECIMAL(5,2))  AS ProgressPercent,
    c.StartDate,
    c.EndDate,
    DATEDIFF(DAY, SYSUTCDATETIME(), c.EndDate)                        AS DaysRemaining,
    (
        SELECT COUNT(*)
        FROM Donations
        WHERE CampaignId = c.CampaignId AND Status = 'Completed'
    )                                                                  AS DonorCount,
    c.CreatedByUserId
FROM DonationCampaigns c
WHERE c.Status = 'Active' AND c.EndDate > SYSUTCDATETIME();
GO

-- ============================================================
-- View: vw_MentorshipOverview
-- Combined student + alumni info for admin mentorship oversight.
-- ============================================================
CREATE OR ALTER VIEW vw_MentorshipOverview
AS
SELECT
    mr.RequestId,
    mr.Status,
    mr.RequestedAt,
    mr.RespondedAt,
    mr.Message,
    mr.ResponseMessage,
    su.UserId           AS StudentUserId,
    su.FullName         AS StudentName,
    su.Email            AS StudentEmail,
    sd.Name             AS StudentDepartment,
    au.UserId           AS AlumniUserId,
    au.FullName         AS AlumniName,
    au.Email            AS AlumniEmail,
    ad.Name             AS AlumniDepartment,
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

-- ============================================================
-- View: vw_UserTickets
-- User tickets with full event details.
-- ============================================================
CREATE OR ALTER VIEW vw_UserTickets
AS
SELECT
    t.TicketId,
    t.TicketCode,
    t.Price,
    t.PaymentStatus,
    t.IssuedAt,
    t.RefundedAt,
    er.UserId,
    er.Attended,
    er.CancelledAt,
    e.EventId,
    e.Title             AS EventTitle,
    e.EventDate,
    e.Location,
    e.Category,
    e.Status            AS EventStatus
FROM Tickets t
INNER JOIN EventRegistrations er ON t.RegistrationId = er.RegistrationId
INNER JOIN Events             e  ON er.EventId = e.EventId;
GO

PRINT 'All views created successfully.';
GO
