namespace ModularMonolithSample.BuildingBlocks.Models;

/// <summary>
/// Standardized error response model
/// </summary>
public class ErrorResponse
{
    public string Title { get; set; } = string.Empty;
    public int Status { get; set; }
    public string Detail { get; set; } = string.Empty;
    public string Instance { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string ErrorCode { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string TraceId { get; set; } = string.Empty;
    public object? Errors { get; set; }
}

/// <summary>
/// Problem details response following RFC 7807
/// </summary>
public class ProblemDetails
{
    public string Type { get; set; } = "about:blank";
    public string Title { get; set; } = string.Empty;
    public int Status { get; set; }
    public string Detail { get; set; } = string.Empty;
    public string Instance { get; set; } = string.Empty;
    public Dictionary<string, object>? Extensions { get; set; }
} 