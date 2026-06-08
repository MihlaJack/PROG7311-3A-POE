-- Initialize GLMS Database
-- This script runs when SQL Server container starts

IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'GLMS')
BEGIN
    CREATE DATABASE GLMS;
END
GO

USE GLMS;
GO

-- Ensure proper collation
ALTER DATABASE GLMS COLLATE Latin1_General_CI_AS;
GO