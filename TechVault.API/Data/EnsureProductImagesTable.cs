using Microsoft.EntityFrameworkCore;

namespace TechVault.API.Data;

/// <summary>
/// MySQL DBs created from partial migrations may omit <c>ProductImages</c>; the app expects it for product queries.
/// </summary>
internal static class EnsureProductImagesTable
{
    public static async Task ExecuteAsync(ApplicationDbContext db, CancellationToken cancellationToken = default)
    {
        const string sql =
            """
            CREATE TABLE IF NOT EXISTS `ProductImages` (
              `Id` int NOT NULL AUTO_INCREMENT,
              `ProductId` int NOT NULL,
              `Url` varchar(2048) NOT NULL,
              `SortOrder` int NOT NULL,
              PRIMARY KEY (`Id`),
              KEY `IX_ProductImages_ProductId` (`ProductId`),
              CONSTRAINT `FK_ProductImages_Products_ProductId` FOREIGN KEY (`ProductId`) REFERENCES `Products` (`Id`) ON DELETE CASCADE
            ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
            """;

        await db.Database.ExecuteSqlRawAsync(sql, cancellationToken);
    }
}
