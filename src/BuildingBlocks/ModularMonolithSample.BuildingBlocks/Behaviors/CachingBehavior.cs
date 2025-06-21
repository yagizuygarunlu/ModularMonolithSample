using System.Text.Json;
using MediatR;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace ModularMonolithSample.BuildingBlocks.Behaviors;

/// <summary>
/// Caches responses for queries that implement ICacheable
/// </summary>
/// <typeparam name="TRequest">The request type</typeparam>
/// <typeparam name="TResponse">The response type</typeparam>
public class CachingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<CachingBehavior<TRequest, TResponse>> _logger;

    public CachingBehavior(IMemoryCache cache, ILogger<CachingBehavior<TRequest, TResponse>> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        // Only cache requests that implement ICacheable
        if (request is not ICacheable cacheableRequest)
        {
            return await next();
        }

        var cacheKey = GenerateCacheKey(request, cacheableRequest.CacheKey);
        
        // Try to get from cache first
        if (_cache.TryGetValue(cacheKey, out TResponse? cachedResponse) && cachedResponse != null)
        {
            _logger.LogDebug("Cache hit for {RequestName} with key {CacheKey}", typeof(TRequest).Name, cacheKey);
            return cachedResponse;
        }

        _logger.LogDebug("Cache miss for {RequestName} with key {CacheKey}", typeof(TRequest).Name, cacheKey);
        
        // Execute the request
        var response = await next();

        // Cache the response
        var cacheOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = cacheableRequest.CacheDuration,
            Priority = CacheItemPriority.Normal
        };

        // Add cache invalidation tags if provided
        if (cacheableRequest.CacheTags?.Any() == true)
        {
            foreach (var tag in cacheableRequest.CacheTags)
            {
                cacheOptions.PostEvictionCallbacks.Add(new PostEvictionCallbackRegistration
                {
                    EvictionCallback = (key, value, reason, state) =>
                    {
                        _logger.LogDebug("Cache entry evicted: {CacheKey}, reason: {Reason}", key, reason);
                    }
                });
            }
        }

        _cache.Set(cacheKey, response, cacheOptions);
        
        _logger.LogDebug("Cached response for {RequestName} with key {CacheKey} for {Duration}", 
            typeof(TRequest).Name, cacheKey, cacheableRequest.CacheDuration);

        return response;
    }

    private static string GenerateCacheKey(TRequest request, string? customCacheKey = null)
    {
        if (!string.IsNullOrEmpty(customCacheKey))
        {
            return $"{typeof(TRequest).Name}:{customCacheKey}";
        }

        // Generate cache key based on request properties
        var requestJson = JsonSerializer.Serialize(request, new JsonSerializerOptions 
        { 
            WriteIndented = false,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        
        var hash = requestJson.GetHashCode();
        return $"{typeof(TRequest).Name}:{hash}";
    }
}

/// <summary>
/// Interface for requests that can be cached
/// </summary>
public interface ICacheable
{
    /// <summary>
    /// Custom cache key (optional). If null, will be generated from request properties.
    /// </summary>
    string? CacheKey { get; }

    /// <summary>
    /// How long to cache the response
    /// </summary>
    TimeSpan CacheDuration { get; }

    /// <summary>
    /// Tags for cache invalidation (optional)
    /// </summary>
    string[]? CacheTags { get; }
}

/// <summary>
/// Attribute to mark queries as cacheable with default settings
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class CacheableAttribute : Attribute
{
    public CacheableAttribute(int durationMinutes = 5)
    {
        DurationMinutes = durationMinutes;
    }

    public int DurationMinutes { get; }
    public string[]? Tags { get; set; }
} 