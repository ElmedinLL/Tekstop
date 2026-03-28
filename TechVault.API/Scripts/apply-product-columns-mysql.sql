-- Products table was never migrated past InitialCreate; code expects soft-delete + JSON + review stats.
-- Run once against TechVaultDB.

ALTER TABLE `products` ADD COLUMN `DeletedAtUtc` datetime(6) NULL;
ALTER TABLE `products` ADD COLUMN `IsDeleted` tinyint(1) NOT NULL DEFAULT 0;
CREATE INDEX `IX_Products_IsDeleted` ON `products` (`IsDeleted`);

ALTER TABLE `products` ADD COLUMN `ImagesJson` longtext NULL;
ALTER TABLE `products` ADD COLUMN `SpecsJson` longtext NULL;

ALTER TABLE `products` ADD COLUMN `AverageRating` decimal(4,2) NULL;
ALTER TABLE `products` ADD COLUMN `ReviewCount` int NOT NULL DEFAULT 0;
