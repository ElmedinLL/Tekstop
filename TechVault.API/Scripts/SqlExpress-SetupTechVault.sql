/*
  TechVault local SQL Server Express — one-time setup (run as sysadmin)

  In SSMS: Connect to your instance (e.g. XHEVAT\SQLEXPRESS) using Windows Authentication
  with an account that is sysadmin (or has dbcreator + rights to create users).

  This script:
    1. Creates database TechVaultDB if missing
    2. Ensures a Windows login exists for your dev user
    3. Maps that login into TechVaultDB as db_owner so EF migrations and the API work

  Replace BOTH occurrences of XHEVAT\elmedin below if your Windows username path differs
  (Object Explorer → Security → Logins shows the exact string).
*/

USE [master];
GO

IF NOT EXISTS (SELECT 1 FROM sys.databases WHERE name = N'TechVaultDB')
BEGIN
    CREATE DATABASE [TechVaultDB];
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.server_principals WHERE name = N'XHEVAT\elmedin')
BEGIN
    CREATE LOGIN [XHEVAT\elmedin] FROM WINDOWS;
END
GO

USE [TechVaultDB];
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.database_principals WHERE name = N'XHEVAT\elmedin')
BEGIN
    CREATE USER [XHEVAT\elmedin] FOR LOGIN [XHEVAT\elmedin];
END
GO

ALTER ROLE [db_owner] ADD MEMBER [XHEVAT\elmedin];
GO
