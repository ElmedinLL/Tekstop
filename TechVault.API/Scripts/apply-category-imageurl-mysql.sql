-- Categories.ImageUrl (see migration AddCategoryImageUrl). Run once on TechVaultDB.

ALTER TABLE `categories` ADD COLUMN `ImageUrl` VARCHAR(2048) NULL;
