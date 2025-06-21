using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using ModularMonolithSample.BuildingBlocks.Exceptions;
using ModularMonolithSample.BuildingBlocks.Models;

namespace ModularMonolithSample.BuildingBlocks.Middleware;

/// <summary>
/// Global exception handling middleware that catches all unhandled exceptions
/// and converts them to appropriate HTTP responses
/// </summary>
public class GlobalExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;

    public GlobalExceptionHandlingMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "An unhandled exception occurred. TraceId: {TraceId}", context.TraceIdentifier);
            await HandleExceptionAsync(context, exception);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        _logger.LogError(exception, "An unhandled exception occurred: {Message}", exception.Message);

        var response = new ApiResponse<object>
        {
            Success = false,
            Message = GetErrorMessage(exception),
            Errors = GetErrorDetails(exception),
            Timestamp = DateTime.UtcNow,
            TraceId = context.TraceIdentifier
        };

        var statusCode = GetStatusCode(exception);
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(jsonResponse);
    }

    private static string GetTitle(int statusCode) => statusCode switch
    {
        400 => "Bad Request",
        401 => "Unauthorized",
        403 => "Forbidden",
        404 => "Not Found",
        408 => "Request Timeout",
        409 => "Conflict",
        422 => "Unprocessable Entity",
        500 => "Internal Server Error",
        _ => "Error"
    };

    private string GetErrorMessage(Exception exception)
    {
        return exception switch
        {
            BaseException baseException => baseException.Message,
            TaskCanceledException => "The request was cancelled due to timeout.",
            _ => "An internal server error occurred."
        };
    }

    private object? GetErrorDetails(Exception exception)
    {
        return exception switch
        {
            BaseException baseException => baseException.Details,
            TaskCanceledException => null,
            _ => new[] { "An internal server error occurred." }
        };
    }

    private int GetStatusCode(Exception exception)
    {
        return exception switch
        {
            BaseException baseException => (int)baseException.StatusCode,
            TaskCanceledException => 408,
            _ => 500
        };
    }
} 