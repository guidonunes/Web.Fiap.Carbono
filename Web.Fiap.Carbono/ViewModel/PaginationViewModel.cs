namespace Web.Fiap.Carbono.ViewModel;

public class PaginationViewModel<T>
{
    public IEnumerable<T> Items { get; set; } = new List<T>();

    public int PageNumber { get; set; }

    public int PageSize { get; set; }

    public int TotalItems { get; set; }

    public int TotalPages { get; set; }
}