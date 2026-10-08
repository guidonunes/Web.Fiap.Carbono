namespace Web.Fiap.Carbono.Data.MongoDb.Repositories;

public sealed record MongoPagedResult<T>(
    IReadOnlyList<T> Items,
    long TotalItems,
    int PageNumber,
    int PageSize
)
{
    public int TotalPages =>
        TotalItems == 0
            ? 0
            : (int)((TotalItems + PageSize - 1) / PageSize);
}