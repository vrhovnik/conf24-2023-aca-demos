namespace ITS.Core;

public class PaginatedList<T> : List<T>
{
    public int PageIndex { get; }
    public int TotalPages { get; }
    public int TotalItems { get; }
    public string Query { get; }
    public string DataBag { get; set; }

    public PaginatedList() : this(new List<T>(), 0, 0, 1, string.Empty, string.Empty)
    {
    }

    public PaginatedList(IEnumerable<T> items, int count, int pageIndex, int pageSize, string query = "",string dataBag="")
    {
        PageIndex = pageIndex;
        DataBag = dataBag;
        TotalPages = (int)Math.Ceiling(count / (double)pageSize);
        TotalItems = count;
        Query = query;
        AddRange(items);
    }

    public bool HasPreviousPage => PageIndex > 1;

    public bool HasNextPage => PageIndex < TotalPages;

    public static PaginatedList<T> Create(
        IQueryable<T> source, int pageIndex, int pageSize, string query = "")
    {
        var count = source.Count();
        var items = source.Skip((pageIndex - 1) * pageSize).Take(pageSize);
        return new PaginatedList<T>(items, count, pageIndex, pageSize, query);
    }

    public static PaginatedList<T> Create(
        List<T> source, int pageIndex, int pageSize, string query = "")
    {
        var count = source.Count;
        var items = source.Skip((pageIndex - 1) * pageSize).Take(pageSize);
        return new PaginatedList<T>(items, count, pageIndex, pageSize, query);
    }
}