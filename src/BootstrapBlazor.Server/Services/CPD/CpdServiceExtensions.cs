using BootstrapBlazor.Server.Services.CPD;

namespace BootstrapBlazor.Server.Services.CPD;

/// <summary>
/// CPD Service Extensions for Dependency Injection
/// </summary>
public static class CpdServiceExtensions
{
    /// <summary>
    /// Add CPD services to the service collection
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="baseUrl">Base URL for CPD API</param>
    /// <returns>Service collection</returns>
    public static IServiceCollection AddCpdServices(this IServiceCollection services, string baseUrl = "https://localhost:5001")
    {
        // Configure HttpClient for CPD API
        services.AddHttpClient<ICpdService, CpdService>(client =>
        {
            client.BaseAddress = new Uri(baseUrl);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.Timeout = TimeSpan.FromMinutes(10); // Long timeout for file uploads
        });

        return services;
    }

    /// <summary>
    /// Add CPD services with custom configuration
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="configureOptions">Configuration action</param>
    /// <returns>Service collection</returns>
    public static IServiceCollection AddCpdServices(this IServiceCollection services, Action<CpdServiceOptions> configureOptions)
    {
        var options = new CpdServiceOptions();
        configureOptions(options);

        // Configure HttpClient for CPD API
        services.AddHttpClient<ICpdService, CpdService>(client =>
        {
            client.BaseAddress = new Uri(options.BaseUrl);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.Timeout = options.Timeout;
            
            if (!string.IsNullOrEmpty(options.ApiKey))
            {
                client.DefaultRequestHeaders.Add("X-API-Key", options.ApiKey);
            }
        });

        return services;
    }
}

/// <summary>
/// CPD Service Configuration Options
/// </summary>
public class CpdServiceOptions
{
    /// <summary>
    /// Base URL for CPD API
    /// </summary>
    public string BaseUrl { get; set; } = "https://localhost:5001";

    /// <summary>
    /// API Key for authentication (optional)
    /// </summary>
    public string? ApiKey { get; set; }

    /// <summary>
    /// Request timeout
    /// </summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromMinutes(10);

    /// <summary>
    /// Enable retry policy
    /// </summary>
    public bool EnableRetry { get; set; } = true;

    /// <summary>
    /// Maximum retry attempts
    /// </summary>
    public int MaxRetryAttempts { get; set; } = 3;
}
