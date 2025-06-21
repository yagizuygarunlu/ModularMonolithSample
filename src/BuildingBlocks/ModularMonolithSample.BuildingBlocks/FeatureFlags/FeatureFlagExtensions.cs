using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace ModularMonolithSample.BuildingBlocks.FeatureFlags;

/// <summary>
/// Modern feature flags system for .NET 9
/// </summary>
public static class FeatureFlagExtensions
{
    public static IServiceCollection AddFeatureFlags(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<FeatureFlagConfiguration>(configuration.GetSection("FeatureFlags"));
        services.AddSingleton<IFeatureFlagService, FeatureFlagService>();
        services.AddScoped<FeatureFlagMiddleware>();
        
        return services;
    }

    public static WebApplication UseFeatureFlags(this WebApplication app)
    {
        app.UseMiddleware<FeatureFlagMiddleware>();
        return app;
    }
}

/// <summary>
/// Feature flag configuration
/// </summary>
public class FeatureFlagConfiguration
{
    public Dictionary<string, FeatureFlagSettings> Flags { get; set; } = new();
    public bool EnableDevModeOverrides { get; set; } = true;
    public TimeSpan CacheExpiration { get; set; } = TimeSpan.FromMinutes(5);
}

/// <summary>
/// Individual feature flag settings
/// </summary>
public class FeatureFlagSettings
{
    public bool Enabled { get; set; }
    public string? Description { get; set; }
    public Dictionary<string, object>? Conditions { get; set; }
    public DateTime? ExpirationDate { get; set; }
    public string[]? AllowedEnvironments { get; set; }
    public string[]? AllowedRoles { get; set; }
    public double? RolloutPercentage { get; set; }
}

/// <summary>
/// Feature flag service interface
/// </summary>
public interface IFeatureFlagService
{
    Task<bool> IsEnabledAsync(string featureName, FeatureFlagContext? context = null);
    Task<T?> GetFeatureValueAsync<T>(string featureName, T? defaultValue = default, FeatureFlagContext? context = null);
    Task<Dictionary<string, bool>> GetAllFlagsAsync(FeatureFlagContext? context = null);
    void InvalidateCache();
}

/// <summary>
/// Context for feature flag evaluation
/// </summary>
public class FeatureFlagContext
{
    public string? UserId { get; set; }
    public string? Environment { get; set; }
    public string[]? UserRoles { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public Dictionary<string, object>? CustomProperties { get; set; }
}

/// <summary>
/// Feature flag service implementation
/// </summary>
public class FeatureFlagService : IFeatureFlagService
{
    private readonly FeatureFlagConfiguration _configuration;
    private readonly ILogger<FeatureFlagService> _logger;
    private readonly Dictionary<string, (bool Value, DateTime CachedAt)> _cache = new();
    private readonly SemaphoreSlim _cacheLock = new(1, 1);

    public FeatureFlagService(
        Microsoft.Extensions.Options.IOptions<FeatureFlagConfiguration> configuration,
        ILogger<FeatureFlagService> logger)
    {
        _configuration = configuration.Value;
        _logger = logger;
    }

    public async Task<bool> IsEnabledAsync(string featureName, FeatureFlagContext? context = null)
    {
        try
        {
            // Check cache first
            if (TryGetFromCache(featureName, out var cachedValue))
            {
                return cachedValue;
            }

            // Evaluate feature flag
            var isEnabled = EvaluateFeatureFlag(featureName, context);
            
            // Cache the result
            await SetCacheAsync(featureName, isEnabled);

            _logger.LogDebug("Feature flag '{FeatureName}' evaluated to {IsEnabled}", featureName, isEnabled);
            
            return isEnabled;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error evaluating feature flag '{FeatureName}'. Defaulting to false.", featureName);
            return false;
        }
    }

    public async Task<T?> GetFeatureValueAsync<T>(string featureName, T? defaultValue = default, FeatureFlagContext? context = null)
    {
        var isEnabled = await IsEnabledAsync(featureName, context);
        
        if (!isEnabled)
            return defaultValue;

        if (_configuration.Flags.TryGetValue(featureName, out var settings) && 
            settings.Conditions?.TryGetValue("value", out var value) == true)
        {
            try
            {
                if (value is JsonElement jsonElement)
                {
                    return JsonSerializer.Deserialize<T>(jsonElement.GetRawText());
                }
                
                return (T?)Convert.ChangeType(value, typeof(T));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to convert feature flag value for '{FeatureName}'", featureName);
            }
        }

        return defaultValue;
    }

    public async Task<Dictionary<string, bool>> GetAllFlagsAsync(FeatureFlagContext? context = null)
    {
        var results = new Dictionary<string, bool>();
        
        foreach (var flagName in _configuration.Flags.Keys)
        {
            results[flagName] = await IsEnabledAsync(flagName, context);
        }

        return results;
    }

    public void InvalidateCache()
    {
        _cache.Clear();
        _logger.LogInformation("Feature flag cache invalidated");
    }

    private bool EvaluateFeatureFlag(string featureName, FeatureFlagContext? context)
    {
        if (!_configuration.Flags.TryGetValue(featureName, out var settings))
        {
            _logger.LogWarning("Feature flag '{FeatureName}' not found in configuration", featureName);
            return false;
        }

        // Check if feature is globally disabled
        if (!settings.Enabled)
            return false;

        // Check expiration date
        if (settings.ExpirationDate.HasValue && DateTime.UtcNow > settings.ExpirationDate.Value)
        {
            _logger.LogInformation("Feature flag '{FeatureName}' has expired", featureName);
            return false;
        }

        // Check environment restrictions
        if (settings.AllowedEnvironments?.Length > 0)
        {
            var currentEnvironment = context?.Environment ?? Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
            if (!settings.AllowedEnvironments.Contains(currentEnvironment, StringComparer.OrdinalIgnoreCase))
            {
                return false;
            }
        }

        // Check role restrictions
        if (settings.AllowedRoles?.Length > 0 && context?.UserRoles != null)
        {
            if (!settings.AllowedRoles.Any(role => context.UserRoles.Contains(role, StringComparer.OrdinalIgnoreCase)))
            {
                return false;
            }
        }

        // Check rollout percentage
        if (settings.RolloutPercentage.HasValue)
        {
            var hash = GetUserHash(context?.UserId ?? context?.IpAddress ?? "anonymous");
            var userPercentile = hash % 100;
            if (userPercentile >= settings.RolloutPercentage.Value)
            {
                return false;
            }
        }

        return true;
    }

    private bool TryGetFromCache(string featureName, out bool value)
    {
        value = false;
        
        if (_cache.TryGetValue(featureName, out var cached))
        {
            if (DateTime.UtcNow - cached.CachedAt < _configuration.CacheExpiration)
            {
                value = cached.Value;
                return true;
            }
            
            _cache.Remove(featureName);
        }

        return false;
    }

    private Task SetCacheAsync(string featureName, bool value)
    {
        _cacheLock.Wait();
        try
        {
            _cache[featureName] = (value, DateTime.UtcNow);
        }
        finally
        {
            _cacheLock.Release();
        }
        
        return Task.CompletedTask;
    }

    private static int GetUserHash(string input)
    {
        return Math.Abs(input.GetHashCode());
    }
}

/// <summary>
/// Middleware to inject feature flag context
/// </summary>
public class FeatureFlagMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<FeatureFlagMiddleware> _logger;

    public FeatureFlagMiddleware(RequestDelegate next, ILogger<FeatureFlagMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IFeatureFlagService featureFlagService)
    {
        // Create feature flag context from HTTP context
        var flagContext = new FeatureFlagContext
        {
            UserId = context.User?.Identity?.Name,
            Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"),
            UserRoles = context.User?.Claims?
                .Where(c => c.Type == "role")
                .Select(c => c.Value)
                .ToArray(),
            IpAddress = context.Connection.RemoteIpAddress?.ToString(),
            UserAgent = context.Request.Headers.UserAgent.ToString()
        };

        // Store context in HttpContext for later use
        context.Items["FeatureFlagContext"] = flagContext;

        await _next(context);
    }
}

/// <summary>
/// Feature flag attribute for controller actions
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class FeatureFlagAttribute : Attribute
{
    public string FeatureName { get; }
    public bool RequireAll { get; set; } = false;

    public FeatureFlagAttribute(string featureName)
    {
        FeatureName = featureName;
    }

    public FeatureFlagAttribute(params string[] featureNames)
    {
        FeatureName = string.Join(",", featureNames);
    }
}

/// <summary>
/// Known feature flags for the system
/// </summary>
public static class FeatureFlags
{
    // Event Management Features
    public const string EnableEventCreation = "enable-event-creation";
    public const string EnableEventCancellation = "enable-event-cancellation";
    public const string EnableAdvancedEventAnalytics = "enable-advanced-event-analytics";
    
    // Attendee Management Features
    public const string EnableBulkAttendeeRegistration = "enable-bulk-attendee-registration";
    public const string EnableAttendeeWaitlist = "enable-attendee-waitlist";
    public const string EnableSocialLogin = "enable-social-login";
    
    // Ticket Management Features
    public const string EnableDigitalTickets = "enable-digital-tickets";
    public const string EnableTicketTransfer = "enable-ticket-transfer";
    public const string EnableQrCodeGeneration = "enable-qr-code-generation";
    
    // Feedback Features
    public const string EnableRealTimeFeedback = "enable-real-time-feedback";
    public const string EnableFeedbackAnalytics = "enable-feedback-analytics";
    public const string EnableAnonymousFeedback = "enable-anonymous-feedback";
    
    // Infrastructure Features
    public const string EnableAdvancedCaching = "enable-advanced-caching";
    public const string EnableDetailedLogging = "enable-detailed-logging";
    public const string EnablePerformanceMonitoring = "enable-performance-monitoring";
    public const string EnableHealthChecks = "enable-health-checks";
} 