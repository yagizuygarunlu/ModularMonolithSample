using System.Security.Claims;
using System.Text.Json;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace ModularMonolithSample.BuildingBlocks.Behaviors;

/// <summary>
/// Audits requests that implement IAuditable by logging user actions and changes
/// </summary>
/// <typeparam name="TRequest">The request type</typeparam>
/// <typeparam name="TResponse">The response type</typeparam>
public class AuditBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<AuditBehavior<TRequest, TResponse>> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuditBehavior(ILogger<AuditBehavior<TRequest, TResponse>> logger, IHttpContextAccessor httpContextAccessor)
    {
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        // Only audit requests that implement IAuditable
        if (request is not IAuditable auditableRequest)
        {
            return await next();
        }

        var auditInfo = await CreateAuditInfo(request, auditableRequest);
        
        // Log audit information before execution
        _logger.LogInformation(
            "Audit: {Action} initiated by {UserId} ({UserName}) at {Timestamp}. Request: {RequestData}",
            auditInfo.Action,
            auditInfo.UserId,
            auditInfo.UserName,
            auditInfo.Timestamp,
            auditInfo.RequestData);

        TResponse response;
        var executionSuccessful = false;
        Exception? exception = null;

        try
        {
            response = await next();
            executionSuccessful = true;
            
            // Log successful audit
            _logger.LogInformation(
                "Audit: {Action} completed successfully by {UserId} at {Timestamp}",
                auditInfo.Action,
                auditInfo.UserId,
                DateTime.UtcNow);

            return response;
        }
        catch (Exception ex)
        {
            exception = ex;
            
            // Log failed audit
            _logger.LogWarning(ex,
                "Audit: {Action} failed for {UserId} at {Timestamp}. Error: {ErrorMessage}",
                auditInfo.Action,
                auditInfo.UserId,
                DateTime.UtcNow,
                ex.Message);
            
            throw;
        }
        finally
        {
            // Could save audit log to database here
            await SaveAuditLog(auditInfo, executionSuccessful, exception);
        }
    }

    private async Task<AuditInfo> CreateAuditInfo(TRequest request, IAuditable auditableRequest)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        var user = httpContext?.User;

        var auditInfo = new AuditInfo
        {
            Action = auditableRequest.Action ?? typeof(TRequest).Name,
            Entity = auditableRequest.Entity,
            EntityId = auditableRequest.EntityId,
            UserId = GetUserId(user),
            UserName = GetUserName(user),
            IpAddress = GetClientIpAddress(httpContext),
            UserAgent = httpContext?.Request.Headers["User-Agent"].ToString(),
            Timestamp = DateTime.UtcNow,
            RequestData = auditableRequest.IncludeRequestData 
                ? JsonSerializer.Serialize(request, new JsonSerializerOptions { WriteIndented = false })
                : null
        };

        return await Task.FromResult(auditInfo);
    }

    private static string? GetUserId(ClaimsPrincipal? user)
    {
        return user?.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
               user?.FindFirst("sub")?.Value ??
               user?.FindFirst("id")?.Value;
    }

    private static string? GetUserName(ClaimsPrincipal? user)
    {
        return user?.FindFirst(ClaimTypes.Name)?.Value ??
               user?.FindFirst("name")?.Value ??
               user?.FindFirst(ClaimTypes.Email)?.Value;
    }

    private static string? GetClientIpAddress(HttpContext? context)
    {
        if (context == null) return null;

        // Check for forwarded IP first (in case of proxy/load balancer)
        var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            return forwardedFor.Split(',')[0].Trim();
        }

        var realIp = context.Request.Headers["X-Real-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(realIp))
        {
            return realIp;
        }

        return context.Connection.RemoteIpAddress?.ToString();
    }

    private async Task SaveAuditLog(AuditInfo auditInfo, bool successful, Exception? exception)
    {
        // Placeholder for saving audit log to database
        // In a real application, you would:
        // 1. Save to audit log table
        // 2. Send to external audit service
        // 3. Write to audit files
        // 4. Send to message queue for processing
        
        _logger.LogTrace(
            "Audit log saved: {Action} by {UserId} - Success: {Successful}",
            auditInfo.Action,
            auditInfo.UserId,
            successful);

        await Task.CompletedTask;
    }
}

/// <summary>
/// Interface for requests that should be audited
/// </summary>
public interface IAuditable
{
    /// <summary>
    /// The action being performed (optional, defaults to request type name)
    /// </summary>
    string? Action { get; }

    /// <summary>
    /// The entity being operated on (e.g., "User", "Order", "Product")
    /// </summary>
    string? Entity { get; }

    /// <summary>
    /// The ID of the entity being operated on (optional)
    /// </summary>
    string? EntityId { get; }

    /// <summary>
    /// Whether to include full request data in audit log (default: false for privacy)
    /// </summary>
    bool IncludeRequestData { get; }
}

/// <summary>
/// Audit information captured for each auditable request
/// </summary>
public class AuditInfo
{
    public string Action { get; set; } = string.Empty;
    public string? Entity { get; set; }
    public string? EntityId { get; set; }
    public string? UserId { get; set; }
    public string? UserName { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public DateTime Timestamp { get; set; }
    public string? RequestData { get; set; }
}

/// <summary>
/// Attribute to mark requests as auditable with default settings
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class AuditableAttribute : Attribute
{
    public AuditableAttribute(string? entity = null, bool includeRequestData = false)
    {
        Entity = entity;
        IncludeRequestData = includeRequestData;
    }

    public string? Entity { get; }
    public bool IncludeRequestData { get; }
    public string? Action { get; set; }
} 