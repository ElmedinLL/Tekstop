-- Run in MySQL Workbench against TechVaultDB if an Identity migration failed halfway
-- (e.g. only AspNetRoles existed). Then run: dotnet ef database update --context AuthDbContext

USE TechVaultDB;

SET FOREIGN_KEY_CHECKS = 0;
DROP TABLE IF EXISTS `RefreshTokens`;
DROP TABLE IF EXISTS `AspNetUserTokens`;
DROP TABLE IF EXISTS `AspNetUserRoles`;
DROP TABLE IF EXISTS `AspNetUserLogins`;
DROP TABLE IF EXISTS `AspNetUserClaims`;
DROP TABLE IF EXISTS `AspNetRoleClaims`;
DROP TABLE IF EXISTS `AspNetUsers`;
DROP TABLE IF EXISTS `AspNetRoles`;
SET FOREIGN_KEY_CHECKS = 1;
