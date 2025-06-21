using Microsoft.AspNetCore.Mvc;
using ModularMonolithSample.BuildingBlocks.Controllers;
using ModularMonolithSample.BuildingBlocks.Models;

namespace ModularMonolithSample.BuildingBlocks.Examples;

/// <summary>
/// Example controller demonstrating various response wrapper usages
/// </summary>
public class ExampleController : BaseApiController
{
    // Example 1: Simple success response with data
    [HttpGet("simple")]
    public ActionResult<ApiResponse<string>> GetSimpleData()
    {
        return Success("Hello World!", "Data retrieved successfully");
    }

    // Example 2: Using Result pattern
    [HttpGet("with-result")]
    public ActionResult<ApiResponse<string>> GetDataWithResult()
    {
        var result = ProcessSomeOperation();
        return FromResult(result);
    }

    // Example 3: Paginated response
    [HttpGet("paginated")]
    public ActionResult<PagedResponse<string>> GetPaginatedData(int page = 1, int size = 10)
    {
        var items = GenerateItems().Skip((page - 1) * size).Take(size);
        var totalCount = GenerateItems().Count();
        
        return PagedSuccess(items, page, size, totalCount);
    }

    // Example 4: Created response
    [HttpPost("create")]
    public ActionResult<ApiResponse<Guid>> CreateResource(ExampleCreateRequest request)
    {
        var id = Guid.NewGuid();
        return Created(id, "Resource created successfully");
    }

    // Example 5: Error response
    [HttpGet("error")]
    public ActionResult<ApiResponse<string>> GetError()
    {
        return BadRequest<string>("Something went wrong", new { Field = "Value", Error = "Invalid" });
    }

    // Example 6: Not found response
    [HttpGet("not-found/{id}")]
    public ActionResult<ApiResponse<string>> GetNotFound(Guid id)
    {
        return NotFound<string>($"Resource with ID {id} was not found");
    }

    // Helper methods
    private Result<string> ProcessSomeOperation()
    {
        // Simulate some business logic
        if (DateTime.Now.Millisecond % 2 == 0)
        {
            return Result<string>.Success("Operation completed successfully");
        }
        
        return Result<string>.Failure("Operation failed due to some reason");
    }

    private List<string> GenerateItems()
    {
        return Enumerable.Range(1, 100).Select(i => $"Item {i}").ToList();
    }
}

/// <summary>
/// Example request model
/// </summary>
public class ExampleCreateRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

/// <summary>
/// Example of manual response creation (without BaseApiController)
/// </summary>
public class ManualResponseExamples
{
    public static ApiResponse<T> CreateSuccessResponse<T>(T data, string message = "Success")
    {
        return ApiResponse<T>.SuccessResult(data, message);
    }

    public static ApiResponse<T> CreateErrorResponse<T>(string message, object? errors = null)
    {
        return ApiResponse<T>.ErrorResult(message, errors);
    }

    public static PagedResponse<T> CreatePagedResponse<T>(
        IEnumerable<T> data, 
        int pageNumber, 
        int pageSize, 
        int totalRecords)
    {
        return PagedResponse<T>.SuccessResult(data, pageNumber, pageSize, totalRecords);
    }
} 