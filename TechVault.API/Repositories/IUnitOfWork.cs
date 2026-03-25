using TechVault.API.Models;

namespace TechVault.API.Repositories;

/// <summary>
/// Coordinates repositories and persists changes in a single transaction via <see cref="SaveChangesAsync"/>.
/// </summary>
public interface IUnitOfWork
{
    IRepository<User> Users { get; }

    IRepository<Address> Addresses { get; }

    IRepository<Category> Categories { get; }

    IRepository<Product> Products { get; }

    IRepository<Tag> Tags { get; }

    IRepository<ProductTag> ProductTags { get; }

    IRepository<CartItem> CartItems { get; }

    IRepository<Order> Orders { get; }

    IRepository<OrderItem> OrderItems { get; }

    IRepository<Review> Reviews { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
