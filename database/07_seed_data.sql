-- ============================================================
-- 07_seed_data.sql
-- Purpose: Populate lookup tables and bootstrap initial admins
-- Run AFTER: 06_triggers.sql
-- ============================================================

USE AlumniMS;
GO

SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

-- ─── Departments ─────────────────────────────────────────────
SET IDENTITY_INSERT Departments OFF;

INSERT INTO Departments (Name, Description) VALUES
('Computer Science',        'Department of Computer Science and IT'),
('Software Engineering',    'Department of Software Engineering'),
('Electrical Engineering',  'Department of Electrical Engineering'),
('Mechanical Engineering',  'Department of Mechanical Engineering'),
('Business Administration', 'School of Business and Management'),
('Economics',               'Department of Economics'),
('Mathematics',             'Department of Mathematics'),
('Physics',                 'Department of Physics');
GO

-- ─── Admins (bootstrap) ──────────────────────────────────────
-- Password: Password@123
-- Hash: $2a$11$bW3lCheafBc/UgLuLNk.luxNx6ZBseJvxYs2ztCIAZ.m32weq0vqO
INSERT INTO Users (FullName, Email, PasswordHash, Phone, Role, IsActive)
VALUES 
('Admin One', 'admin1@university.edu', '$2a$11$bW3lCheafBc/UgLuLNk.luxNx6ZBseJvxYs2ztCIAZ.m32weq0vqO', '+923001234561', 'Admin', 1),
('Admin Two', 'admin2@university.edu', '$2a$11$bW3lCheafBc/UgLuLNk.luxNx6ZBseJvxYs2ztCIAZ.m32weq0vqO', '+923001234562', 'Admin', 1);
GO

-- ─── Initial Admin Invite Codes ───────────────────────────────
DECLARE @AdminId INT = (SELECT UserId FROM Users WHERE Email = 'admin1@university.edu');

INSERT INTO AdminInviteCodes (Code, ExpiryDate, CreatedByUserId) VALUES
('ADMIN-INIT-001', DATEADD(DAY, 90, SYSUTCDATETIME()), @AdminId),
('ADMIN-INIT-002', DATEADD(DAY, 90, SYSUTCDATETIME()), @AdminId),
('ADMIN-INIT-003', DATEADD(DAY, 90, SYSUTCDATETIME()), @AdminId);
GO

PRINT 'Seed data inserted: 8 departments, 2 admins, 3 invite codes.';
GO

-- ─── Quick Verification ──────────────────────────────────────
SELECT 'Departments' AS [Table], COUNT(*) AS [Rows] FROM Departments
UNION ALL
SELECT 'Users',                   COUNT(*)           FROM Users
UNION ALL
SELECT 'AdminInviteCodes',        COUNT(*)           FROM AdminInviteCodes;
GO
