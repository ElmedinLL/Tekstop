namespace TechVault.API.Repositories.Products;

public sealed record PageRequest(int PageNumber = 1, int PageSize = 20)
{
    public int NormalizedPage => PageNumber < 1 ? 1 : PageNumber;

    public int NormalizedPageSize
    {
        get
        {
            if (PageSize < 1)
            {
                return 20;
            }

            return PageSize > 100 ? 100 : PageSize;
        }
    }

    public int Skip => (NormalizedPage - 1) * NormalizedPageSize;

    public int Take => NormalizedPageSize;
}
