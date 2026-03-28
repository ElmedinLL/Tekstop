-- ProductImages was never applied via EF migrations; CreateProduct loads images and fails without this table.
-- Run once against TechVaultDB (adjust `Products` casing if your schema uses `products` only).

CREATE TABLE IF NOT EXISTS `ProductImages` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `ProductId` int NOT NULL,
  `Url` varchar(2048) NOT NULL,
  `SortOrder` int NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_ProductImages_ProductId` (`ProductId`),
  CONSTRAINT `FK_ProductImages_Products_ProductId` FOREIGN KEY (`ProductId`) REFERENCES `Products` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
