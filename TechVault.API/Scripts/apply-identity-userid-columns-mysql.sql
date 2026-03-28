-- One-time fix: DB only had InitialCreate applied; code expects IdentityUserId on users/cartitems/orders.
-- Run: mysql -u root -p TechVaultDB < apply-identity-userid-columns-mysql.sql
-- Clears cartitems (dev cart rows).

SET FOREIGN_KEY_CHECKS = 0;

ALTER TABLE `users` ADD COLUMN `IdentityUserId` VARCHAR(450) NULL;
CREATE UNIQUE INDEX `IX_Users_IdentityUserId` ON `users` (`IdentityUserId`);

DELETE FROM `cartitems`;
ALTER TABLE `cartitems` DROP FOREIGN KEY `FK_CartItems_Users_UserId`;
DROP INDEX `IX_CartItems_UserId_ProductId` ON `cartitems`;
ALTER TABLE `cartitems` DROP COLUMN `UserId`;
ALTER TABLE `cartitems` ADD COLUMN `IdentityUserId` VARCHAR(450) CHARACTER SET utf8mb4 NOT NULL;
CREATE UNIQUE INDEX `IX_CartItems_IdentityUserId_ProductId` ON `cartitems` (`IdentityUserId`, `ProductId`);

ALTER TABLE `orders` DROP FOREIGN KEY `FK_Orders_Users_UserId`;
ALTER TABLE `orders` MODIFY COLUMN `UserId` INT NULL;
ALTER TABLE `orders` ADD COLUMN `IdentityUserId` VARCHAR(450) NULL;
CREATE INDEX `IX_Orders_IdentityUserId` ON `orders` (`IdentityUserId`);
CREATE INDEX `IX_Orders_IdentityUserId_Status` ON `orders` (`IdentityUserId`, `Status`);
ALTER TABLE `orders` ADD CONSTRAINT `FK_Orders_Users_UserId`
  FOREIGN KEY (`UserId`) REFERENCES `users` (`Id`) ON DELETE RESTRICT;

SET FOREIGN_KEY_CHECKS = 1;
