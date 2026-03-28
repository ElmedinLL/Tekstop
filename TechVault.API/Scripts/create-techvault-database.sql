-- Run this in MySQL Workbench (or mysql CLI) while connected as a user that can create databases (e.g. root).
-- Then update TechVault.API/appsettings.json: set Password= in DefaultConnection to your MySQL password.

CREATE DATABASE IF NOT EXISTS TechVaultDB
  CHARACTER SET utf8mb4
  COLLATE utf8mb4_unicode_ci;
