-- Prefer Scripts/SqlExpress-SetupTechVault.sql — it creates the database AND grants your
-- Windows login access (fixes error 4060 "Cannot open database TechVaultDB").
--
-- This file only creates the database if you already have CREATE DATABASE rights.

IF NOT EXISTS (SELECT 1 FROM sys.databases WHERE name = N'TechVaultDB')
BEGIN
    CREATE DATABASE TechVaultDB;
END
GO
