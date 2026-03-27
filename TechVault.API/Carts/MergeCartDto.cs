namespace TechVault.API.Carts;

public sealed class MergeCartDto
{
    public List<MergeCartLineDto> Lines { get; set; } = new();
}

public sealed class MergeCartLineDto
{
    public int ProductId { get; set; }

    public int Quantity { get; set; }
}
