using TechVault.API.Data;
using TechVault.API.Models;

namespace TechVault.API.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;

    private IRepository<User>? _users;
    private IRepository<Address>? _addresses;
    private IRepository<Category>? _categories;
    private IRepository<Product>? _products;
    private IRepository<Tag>? _tags;
    private IRepository<ProductTag>? _productTags;
    private IRepository<CartItem>? _cartItems;
    private IRepository<Order>? _orders;
    private IRepository<OrderItem>? _orderItems;
    private IRepository<Review>? _reviews;

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
    }

    public IRepository<User> Users => _users ??= new BaseRepository<User>(_context);

    public IRepository<Address> Addresses => _addresses ??= new BaseRepository<Address>(_context);

    public IRepository<Category> Categories => _categories ??= new BaseRepository<Category>(_context);

    public IRepository<Product> Products => _products ??= new BaseRepository<Product>(_context);

    public IRepository<Tag> Tags => _tags ??= new BaseRepository<Tag>(_context);

    public IRepository<ProductTag> ProductTags => _productTags ??= new BaseRepository<ProductTag>(_context);

    public IRepository<CartItem> CartItems => _cartItems ??= new BaseRepository<CartItem>(_context);

    public IRepository<Order> Orders => _orders ??= new BaseRepository<Order>(_context);

    public IRepository<OrderItem> OrderItems => _orderItems ??= new BaseRepository<OrderItem>(_context);

    public IRepository<Review> Reviews => _reviews ??= new BaseRepository<Review>(_context);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => _context.SaveChangesAsync(cancellationToken);
}
