-- ============================================================
-- 01_create_database.sql
-- Purpose: Drop (if exists) and create the AlumniMS database
-- Run as: sysadmin or db_creator on the SQL Server instance
-- ============================================================

USE master;
GO

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

PRINT 'Database AlumniMS created successfully.';
GO
