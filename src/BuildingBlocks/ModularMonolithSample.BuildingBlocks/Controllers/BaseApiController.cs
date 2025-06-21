using Microsoft.AspNetCore.Mvc;
using ModularMonolithSample.BuildingBlocks.Models;

namespace ModularMonolithSample.BuildingBlocks.Controllers;

/// <summary>
/// Base API controller that provides helper methods for consistent responses
/// </summary>
[ApiController]
[Route("api/[controller]")]
public abstract class BaseApiController : ControllerBase
{
    /// <summary>
    /// Returns a successful response with data
    /// </summary>
    protected ActionResult<ApiResponse<T>> Success<T>(T data, string message = "Success")
    {
        var response = ApiResponse<T>.SuccessResult(data, message);
        response.TraceId = HttpContext.TraceIdentifier;
        return Ok(response);
    }

    /// <summary>
    /// Returns a successful response without data
    /// </summary>
    protected ActionResult<ApiResponse> Success(string message = "Success")
    {
        var response = ApiResponse.SuccessResult(message);
        response.TraceId = HttpContext.TraceIdentifier;
        return Ok(response);
    }

    /// <summary>
    /// Returns a created response with data
    /// </summary>
    protected ActionResult<ApiResponse<T>> Created<T>(T data, string message = "Resource created successfully")
    {
        var response = ApiResponse<T>.SuccessResult(data, message);
        response.TraceId = HttpContext.TraceIdentifier;
        return StatusCode(201, response);
    }

    /// <summary>
    /// Returns a created response with location
    /// </summary>
    protected ActionResult<ApiResponse<T>> Created<T>(string actionName, object routeValues, T data, string message = "Resource created successfully")
    {
        var response = ApiResponse<T>.SuccessResult(data, message);
        response.TraceId = HttpContext.TraceIdentifier;
        return CreatedAtAction(actionName, routeValues, response);
    }

    /// <summary>
    /// Returns a paginated response
    /// </summary>
    protected ActionResult<PagedResponse<T>> PagedSuccess<T>(
        IEnumerable<T> data, 
        int pageNumber, 
        int pageSize, 
        int totalRecords,
        string message = "Data retrieved successfully")
    {
        var response = PagedResponse<T>.SuccessResult(data, pageNumber, pageSize, totalRecords, message);
        response.TraceId = HttpContext.TraceIdentifier;
        return Ok(response);
    }

    /// <summary>
    /// Returns a bad request response
    /// </summary>
    protected ActionResult<ApiResponse<T>> BadRequest<T>(string message, object? errors = null)
    {
        var response = ApiResponse<T>.ErrorResult(message, errors);
        response.TraceId = HttpContext.TraceIdentifier;
        return BadRequest(response);
    }

    /// <summary>
    /// Returns a not found response
    /// </summary>
    protected ActionResult<ApiResponse<T>> NotFound<T>(string message)
    {
        var response = ApiResponse<T>.ErrorResult(message);
        response.TraceId = HttpContext.TraceIdentifier;
        return NotFound(response);
    }

    /// <summary>
    /// Returns a response based on Result pattern
    /// </summary>
    protected ActionResult<ApiResponse<T>> FromResult<T>(Result<T> result, string? successMessage = null)
    {
        var response = result.ToApiResponse(successMessage);
        response.TraceId = HttpContext.TraceIdentifier;
        
        if (result.IsSuccess)
        {
            return Ok(response);
        }

        return BadRequest(response);
    }

    /// <summary>
    /// Returns a response based on non-generic Result pattern
    /// </summary>
    protected ActionResult<ApiResponse> FromResult(Result result, string? successMessage = null)
    {
        var response = result.ToApiResponse(successMessage);
        response.TraceId = HttpContext.TraceIdentifier;
        
        if (result.IsSuccess)
        {
            return Ok(response);
        }

        return BadRequest(response);
    }
} 