using Microsoft.AspNetCore.Builder;
using ModularMonolithSample.BuildingBlocks.Middleware;

namespace ModularMonolithSample.BuildingBlocks.Extensions;

/// <summary>
/// Extension methods for IApplicationBuilder to configure exception handling middleware
/// </summary>
public static class ApplicationBuilderExtensions
{
    /// <summary>
    /// Adds global exception handling middleware to the pipeline
    /// </summary>
    public static IApplicationBuilder UseGlobalExceptionHandling(this IApplicationBuilder app)
    {
        return app.UseMiddleware<GlobalExceptionHandlingMiddleware>();
    }
} 