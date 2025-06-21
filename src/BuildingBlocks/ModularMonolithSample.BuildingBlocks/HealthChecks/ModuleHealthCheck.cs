using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Diagnostics;

namespace ModularMonolithSample.BuildingBlocks.HealthChecks;

/// <summary>
/// Modern health check implementation for modular monolith modules
/// </summary>
public class ModuleHealthCheck : IHealthCheck
{
    private readonly string _moduleName;
    private readonly IServiceProvider _serviceProvider;

    public ModuleHealthCheck(string moduleName, IServiceProvider serviceProvider)
    {
        _moduleName = moduleName;
        _serviceProvider = serviceProvider;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var stopwatch = Stopwatch.StartNew();
            
            // Check module-specific health indicators
            var healthData = new Dictionary<string, object>
            {
                ["module"] = _moduleName,
                ["timestamp"] = DateTime.UtcNow,
                ["environment"] = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown"
            };

            // Check memory usage
            var memoryUsage = GC.GetTotalMemory(false);
            healthData["memoryUsageBytes"] = memoryUsage;
            healthData["memoryUsageMB"] = Math.Round(memoryUsage / 1024.0 / 1024.0, 2);

            // Check if critical services are available
            await CheckModuleServices(healthData);

            stopwatch.Stop();
            healthData["checkDurationMs"] = stopwatch.ElapsedMilliseconds;

            // Determine health status based on metrics
            var status = DetermineHealthStatus(healthData);

            return new HealthCheckResult(
                status,
                description: $"{_moduleName} module health check",
                data: healthData
            );
        }
        catch (Exception ex)
        {
            return new HealthCheckResult(
                HealthStatus.Unhealthy,
                description: $"{_moduleName} module health check failed",
                exception: ex,
                data: new Dictionary<string, object>
                {
                    ["module"] = _moduleName,
                    ["error"] = ex.Message,
                    ["timestamp"] = DateTime.UtcNow
                }
            );
        }
    }

    private Task CheckModuleServices(Dictionary<string, object> healthData)
    {
        // Check if module services are properly registered
        var serviceTypes = GetModuleServiceTypes();
        var servicesStatus = new Dictionary<string, bool>();

        foreach (var serviceType in serviceTypes)
        {
            try
            {
                var service = _serviceProvider.GetService(serviceType);
                servicesStatus[serviceType.Name] = service != null;
            }
            catch
            {
                servicesStatus[serviceType.Name] = false;
            }
        }

        healthData["services"] = servicesStatus;
        healthData["allServicesHealthy"] = servicesStatus.Values.All(x => x);
        
        return Task.CompletedTask;
    }

    private Type[] GetModuleServiceTypes()
    {
        // Return empty array for now to avoid namespace dependencies
        // In a real implementation, these would be injected or resolved differently
        return [];
    }

    private static HealthStatus DetermineHealthStatus(Dictionary<string, object> healthData)
    {
        // Check memory usage threshold (500 MB warning, 1 GB critical)
        if (healthData["memoryUsageMB"] is double memoryMB)
        {
            if (memoryMB > 1024)
                return HealthStatus.Unhealthy;
            if (memoryMB > 512)
                return HealthStatus.Degraded;
        }

        // Check response time threshold
        if (healthData["checkDurationMs"] is long durationMs && durationMs > 5000)
            return HealthStatus.Degraded;

        // Check if all services are healthy
        if (healthData["allServicesHealthy"] is bool allHealthy && !allHealthy)
            return HealthStatus.Degraded;

        return HealthStatus.Healthy;
    }
}

/// <summary>
/// Database health check for Entity Framework contexts
/// </summary>
public class DatabaseHealthCheck : IHealthCheck
{
    private readonly string _connectionString;
    private readonly string _contextName;

    public DatabaseHealthCheck(string connectionString, string contextName)
    {
        _connectionString = connectionString;
        _contextName = contextName;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var stopwatch = Stopwatch.StartNew();
            
            // Simple connection test (replace with actual DB context check)
            await Task.Delay(10, cancellationToken); // Simulate DB check
            
            stopwatch.Stop();
            
            var healthData = new Dictionary<string, object>
            {
                ["context"] = _contextName,
                ["connectionTime"] = stopwatch.ElapsedMilliseconds,
                ["timestamp"] = DateTime.UtcNow
            };

            var status = stopwatch.ElapsedMilliseconds switch
            {
                > 5000 => HealthStatus.Unhealthy,
                > 1000 => HealthStatus.Degraded,
                _ => HealthStatus.Healthy
            };

            return new HealthCheckResult(
                status,
                description: $"{_contextName} database health check",
                data: healthData
            );
        }
        catch (Exception ex)
        {
            return new HealthCheckResult(
                HealthStatus.Unhealthy,
                description: $"{_contextName} database connection failed",
                exception: ex
            );
        }
    }
} 