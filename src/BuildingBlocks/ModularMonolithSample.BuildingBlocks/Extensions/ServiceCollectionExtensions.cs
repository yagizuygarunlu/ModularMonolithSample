using Microsoft.Extensions.DependencyInjection;
using ModularMonolithSample.BuildingBlocks.Middleware;
using ModularMonolithSample.BuildingBlocks.Filters;
using ModularMonolithSample.BuildingBlocks.Behaviors;
using MediatR;

namespace ModularMonolithSample.BuildingBlocks.Extensions;

/// <summary>
/// Extension methods for IServiceCollection to register exception handling services
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers global exception handling services
    /// </summary>
    public static IServiceCollection AddGlobalExceptionHandling(this IServiceCollection services)
    {
        // Note: GlobalExceptionHandlingMiddleware is not registered as a service
        // It's registered in the pipeline via app.UseGlobalExceptionHandling()
        return services;
    }

    /// <summary>
    /// Registers API response wrapper services and filters
    /// </summary>
    public static IServiceCollection AddApiResponseWrapper(this IServiceCollection services)
    {
        services.AddScoped<ApiResponseFilter>();
        
        services.Configure<Microsoft.AspNetCore.Mvc.MvcOptions>(options =>
        {
            options.Filters.Add<ApiResponseFilter>();
        });

        return services;
    }

    /// <summary>
    /// Registers all MediatR behaviors for cross-cutting concerns
    /// </summary>
    public static IServiceCollection AddMediatRBehaviors(this IServiceCollection services, Action<BehaviorConfiguration>? configure = null)
    {
        var config = new BehaviorConfiguration();
        configure?.Invoke(config);

        // Always register validation behavior first
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        // Register other behaviors based on configuration
        if (config.EnableLogging)
        {
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        }

        if (config.EnablePerformanceMonitoring)
        {
            services.AddSingleton(config.PerformanceSettings);
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(PerformanceBehavior<,>));
        }

        if (config.EnableCaching)
        {
            services.AddMemoryCache();
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(CachingBehavior<,>));
        }

        if (config.EnableRetry)
        {
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(RetryBehavior<,>));
        }

        if (config.EnableAuditing)
        {
            services.AddHttpContextAccessor();
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(AuditBehavior<,>));
        }

        if (config.EnableTransactions)
        {
            services.AddScoped<ITransactionManager, DefaultTransactionManager>();
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(TransactionBehavior<,>));
        }

        return services;
    }

    /// <summary>
    /// Registers both exception handling and API response wrapper
    /// </summary>
    public static IServiceCollection AddStandardApiServices(this IServiceCollection services)
    {
        services.AddGlobalExceptionHandling();
        services.AddApiResponseWrapper();
        return services;
    }

    /// <summary>
    /// Registers all building blocks services (API services + MediatR behaviors)
    /// </summary>
    public static IServiceCollection AddBuildingBlocks(this IServiceCollection services, Action<BehaviorConfiguration>? configure = null)
    {
        services.AddStandardApiServices();
        services.AddMediatRBehaviors(configure);
        return services;
    }
}

/// <summary>
/// Configuration for MediatR behaviors
/// </summary>
public class BehaviorConfiguration
{
    /// <summary>
    /// Enable request/response logging (default: true)
    /// </summary>
    public bool EnableLogging { get; set; } = true;

    /// <summary>
    /// Enable performance monitoring (default: true)
    /// </summary>
    public bool EnablePerformanceMonitoring { get; set; } = true;

    /// <summary>
    /// Enable caching for queries (default: true)
    /// </summary>
    public bool EnableCaching { get; set; } = true;

    /// <summary>
    /// Enable retry logic (default: false)
    /// </summary>
    public bool EnableRetry { get; set; } = false;

    /// <summary>
    /// Enable audit logging (default: false)
    /// </summary>
    public bool EnableAuditing { get; set; } = false;

    /// <summary>
    /// Enable transaction management (default: false)
    /// </summary>
    public bool EnableTransactions { get; set; } = false;

    /// <summary>
    /// Performance monitoring settings
    /// </summary>
    public PerformanceSettings PerformanceSettings { get; set; } = new();
} 