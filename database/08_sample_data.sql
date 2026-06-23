-- ============================================================
-- 08_sample_data.sql
-- Purpose: Populate requested alumni and student users, along
--          with sample campaigns, donations, events, registrations,
--          tickets, and mentorship requests.
-- Run AFTER: 07_seed_data.sql
-- ============================================================

USE AlumniMS;
GO

SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

-- ─── Password Hash ──────────────────────────────────────────
-- Password: Password@123
-- Hash: $2a$11$bW3lCheafBc/UgLuLNk.luxNx6ZBseJvxYs2ztCIAZ.m32weq0vqO
DECLARE @PwHash NVARCHAR(500) = '$2a$11$bW3lCheafBc/UgLuLNk.luxNx6ZBseJvxYs2ztCIAZ.m32weq0vqO';

-- ─── Alumni Users ────────────────────────────────────────────
INSERT INTO Users (FullName, Email, PasswordHash, Phone, DateOfBirth, Role, IsActive) VALUES
('Zuhar',  'zuhar@alumni.edu',  @PwHash, '+923011111101', '1998-05-15', 'Alumni', 1),
('Ramsha', 'ramsha@alumni.edu', @PwHash, '+923011111102', '1999-08-20', 'Alumni', 1);

-- ─── Student Users ───────────────────────────────────────────
INSERT INTO Users (FullName, Email, PasswordHash, Phone, DateOfBirth, Role, IsActive) VALUES
('Student One', 'std1@student.edu', @PwHash, '+923011111103', '2004-01-10', 'Student', 1),
('Student Two', 'std2@student.edu', @PwHash, '+923011111104', '2005-03-25', 'Student', 1);
GO

-- ─── Alumni Profiles ─────────────────────────────────────────
DECLARE @ZuharId INT = (SELECT UserId FROM Users WHERE Email = 'zuhar@alumni.edu');
DECLARE @RamshaId INT = (SELECT UserId FROM Users WHERE Email = 'ramsha@alumni.edu');

INSERT INTO AlumniProfiles (UserId, DepartmentId, GraduationYear, DegreeProgram, CurrentCompany, JobTitle, Industry, WorkLocation, ShortBio) VALUES
(@ZuharId, 1, 2020, 'BS Computer Science', 'TechCorp', 'Software Engineer', 'Information Technology', 'Lahore, Pakistan', 'Passionate developer and mentor.'),
(@RamshaId, 2, 2021, 'BS Software Engineering', 'SoftSolutions', 'QA Engineer', 'Information Technology', 'Karachi, Pakistan', 'Quality assurance specialist.');

-- ─── Student Profiles ────────────────────────────────────────
DECLARE @Std1Id INT = (SELECT UserId FROM Users WHERE Email = 'std1@student.edu');
DECLARE @Std2Id INT = (SELECT UserId FROM Users WHERE Email = 'std2@student.edu');

INSERT INTO StudentProfiles (UserId, DepartmentId, EnrollmentYear, ExpectedGraduationYear, DegreeProgram, CurrentSemester, CGPA, ShortBio) VALUES
(@Std1Id, 1, 2023, 2027, 'BS Computer Science', 5, 3.80, 'Interested in AI and Web development.'),
(@Std2Id, 2, 2024, 2028, 'BS Software Engineering', 3, 3.50, 'Aspiring product manager.');
GO

-- ─── Donation Campaigns ──────────────────────────────────────
DECLARE @AdminId INT = (SELECT UserId FROM Users WHERE Email = 'admin1@university.edu');

INSERT INTO DonationCampaigns (Title, Description, TargetAmount, RaisedAmount, StartDate, EndDate, Status, CreatedByUserId) VALUES
('Annual Scholarship Fund 2026', 'Fund to support underprivileged students in their higher education journey.', 50000.00, 0.00, SYSUTCDATETIME(), DATEADD(DAY, 60, SYSUTCDATETIME()), 'Active', @AdminId),
('New Campus Library Wing', 'Raising funds to construct a modern library wing with digital resources.', 200000.00, 0.00, SYSUTCDATETIME(), DATEADD(DAY, 90, SYSUTCDATETIME()), 'Active', @AdminId);
GO

-- ─── Donations (trigger tr_Donations_AfterInsert updates campaign RaisedAmount) ─
DECLARE @ZuharId INT = (SELECT UserId FROM Users WHERE Email = 'zuhar@alumni.edu');
DECLARE @RamshaId INT = (SELECT UserId FROM Users WHERE Email = 'ramsha@alumni.edu');
DECLARE @Campaign1Id INT = (SELECT CampaignId FROM DonationCampaigns WHERE Title = 'Annual Scholarship Fund 2026');
DECLARE @Campaign2Id INT = (SELECT CampaignId FROM DonationCampaigns WHERE Title = 'New Campus Library Wing');

INSERT INTO Donations (CampaignId, DonorUserId, Amount, IsAnonymous, Status) VALUES
(@Campaign1Id, @ZuharId, 2500.00, 0, 'Completed'),
(@Campaign2Id, @RamshaId, 5000.00, 1, 'Completed');
GO

-- ─── Events ──────────────────────────────────────────────────
DECLARE @AdminId INT = (SELECT UserId FROM Users WHERE Email = 'admin1@university.edu');

INSERT INTO Events (Title, Description, Category, EventDate, Location, Capacity, TicketPrice, Status, CreatedByUserId) VALUES
('CS Alumni Homecoming 2026', 'A networking and reunion dinner event for all Computer Science department alumni.', 'Reunion', DATEADD(DAY, 10, SYSUTCDATETIME()), 'Main Auditorium, Campus A', 100, 500.00, 'Scheduled', @AdminId),
('AI & Software Engineering Workshop', 'A hands-on workshop focused on LLM integration and modern software architectures.', 'Workshop', DATEADD(DAY, 20, SYSUTCDATETIME()), 'Seminar Hall 3, Block B', 50, 0.00, 'Scheduled', @AdminId);
GO

-- ─── Event Registrations & Tickets ───────────────────────────
DECLARE @ZuharId INT = (SELECT UserId FROM Users WHERE Email = 'zuhar@alumni.edu');
DECLARE @Std1Id INT = (SELECT UserId FROM Users WHERE Email = 'std1@student.edu');
DECLARE @Event1Id INT = (SELECT EventId FROM Events WHERE Title = 'CS Alumni Homecoming 2026');
DECLARE @Event2Id INT = (SELECT EventId FROM Events WHERE Title = 'AI & Software Engineering Workshop');

-- Zuhar registers for Homecoming Event (Paid)
INSERT INTO EventRegistrations (EventId, UserId, Attended) VALUES (@Event1Id, @ZuharId, 0);
DECLARE @Reg1Id INT = SCOPE_IDENTITY();
INSERT INTO Tickets (RegistrationId, TicketCode, Price, PaymentStatus) VALUES (@Reg1Id, 'TKT-HOME-ZUH-01', 500.00, 'Paid');

-- Student One registers for Workshop Event (Free)
INSERT INTO EventRegistrations (EventId, UserId, Attended) VALUES (@Event2Id, @Std1Id, 0);
DECLARE @Reg2Id INT = SCOPE_IDENTITY();
INSERT INTO Tickets (RegistrationId, TicketCode, Price, PaymentStatus) VALUES (@Reg2Id, 'TKT-AIWS-STD-01', 0.00, 'Paid');
GO

-- ─── Mentorship Requests ─────────────────────────────────────
DECLARE @Std1Id INT = (SELECT UserId FROM Users WHERE Email = 'std1@student.edu');
DECLARE @Std2Id INT = (SELECT UserId FROM Users WHERE Email = 'std2@student.edu');
DECLARE @ZuharId INT = (SELECT UserId FROM Users WHERE Email = 'zuhar@alumni.edu');
DECLARE @RamshaId INT = (SELECT UserId FROM Users WHERE Email = 'ramsha@alumni.edu');

INSERT INTO MentorshipRequests (StudentUserId, AlumniUserId, Message, Status) VALUES
(@Std1Id, @ZuharId, 'Hello Zuhar, I am very interested in Web Development and AI. I would love to get your mentorship and guidance.', 'Pending'),
(@Std2Id, @RamshaId, 'Hi Ramsha, I am looking to specialize in Quality Assurance. Your guidance on QA practices and tools would be highly valuable.', 'Pending');
GO

PRINT 'Sample data inserted: 2 alumni, 2 students, 2 campaigns, 2 donations, 2 events, 2 registrations/tickets, 2 mentorship requests.';
GO

-- ─── Quick Verification ──────────────────────────────────────
SELECT 'Total Users' AS [Entity], COUNT(*) AS [Count] FROM Users
UNION ALL
SELECT 'Alumni',                                       COUNT(*) FROM AlumniProfiles
UNION ALL
SELECT 'Students',                                     COUNT(*) FROM StudentProfiles
UNION ALL
SELECT 'Campaigns',                                    COUNT(*) FROM DonationCampaigns
UNION ALL
SELECT 'Donations',                                    COUNT(*) FROM Donations
UNION ALL
SELECT 'Events',                                       COUNT(*) FROM Events
UNION ALL
SELECT 'Tickets',                                      COUNT(*) FROM Tickets
UNION ALL
SELECT 'Mentorship Requests',                          COUNT(*) FROM MentorshipRequests;
GO
