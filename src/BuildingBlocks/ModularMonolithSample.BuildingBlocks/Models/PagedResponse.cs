namespace ModularMonolithSample.BuildingBlocks.Models;

/// <summary>
/// Paginated response wrapper for list endpoints
/// </summary>
/// <typeparam name="T">The type of items in the collection</typeparam>
public class PagedResponse<T> : ApiResponse<IEnumerable<T>>
{
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalRecords { get; set; }
    public int TotalPages { get; set; }
    public bool HasNextPage { get; set; }
    public bool HasPreviousPage { get; set; }

    public PagedResponse()
    {
    }

    public PagedResponse(IEnumerable<T> data, int pageNumber, int pageSize, int totalRecords)
    {
        Success = true;
        Message = "Data retrieved successfully";
        Data = data;
        PageNumber = pageNumber;
        PageSize = pageSize;
        TotalRecords = totalRecords;
        TotalPages = (int)Math.Ceiling((double)totalRecords / pageSize);
        HasNextPage = pageNumber < TotalPages;
        HasPreviousPage = pageNumber > 1;
    }

    /// <summary>
    /// Creates a successful paged response
    /// </summary>
    public static PagedResponse<T> SuccessResult(
        IEnumerable<T> data, 
        int pageNumber, 
        int pageSize, 
        int totalRecords,
        string message = "Data retrieved successfully")
    {
        return new PagedResponse<T>(data, pageNumber, pageSize, totalRecords)
        {
            Message = message
        };
    }

    /// <summary>
    /// Creates an error paged response
    /// </summary>
    public static new PagedResponse<T> ErrorResult(string message, object? errors = null)
    {
        return new PagedResponse<T>
        {
            Success = false,
            Message = message,
            Errors = errors,
            Data = Enumerable.Empty<T>()
        };
    }
}

/// <summary>
/// Pagination parameters for requests
/// </summary>
public class PaginationParameters
{
    private const int MaxPageSize = 100;
    private int _pageSize = 10;

    public int PageNumber { get; set; } = 1;
    
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value > MaxPageSize ? MaxPageSize : value;
    }

    public string? SortBy { get; set; }
    public string? SortDirection { get; set; } = "asc";
}

/// <summary>
/// Pagination metadata for response headers
/// </summary>
public class PaginationMetadata
{
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalRecords { get; set; }
    public int TotalPages { get; set; }
    public bool HasNextPage { get; set; }
    public bool HasPreviousPage { get; set; }
} 