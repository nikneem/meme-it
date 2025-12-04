namespace HexMaster.MemeIt.Memes.Api.Middleware;

public class ApiKeyMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ApiKeyMiddleware> _logger;
    private const string ApiKeyHeaderName = "X-ApiKey";

    public ApiKeyMiddleware(
        RequestDelegate next,
        IConfiguration configuration,
        ILogger<ApiKeyMiddleware> logger)
    {
        _next = next;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Only validate API key for management endpoints (create, update, delete, upload-token)
        var path = context.Request.Path.Value?.ToLowerInvariant() ?? string.Empty;
        var method = context.Request.Method.ToUpperInvariant();

        var requiresApiKey = (method == "POST" || method == "PUT" || method == "DELETE") &&
                            (path.Contains("/memes/templates") || path.Contains("/memes/upload-token"));

        if (!requiresApiKey)
        {
            await _next(context);
            return;
        }

        // Check for API key in header
        if (!context.Request.Headers.TryGetValue(ApiKeyHeaderName, out var extractedApiKey))
        {
            _logger.LogWarning("API key missing for management operation: {Method} {Path}", method, path);
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("API key is required for this operation");
            return;
        }

        // Validate API key
        var configuredApiKey = _configuration["Management:ApiKey"];
        if (string.IsNullOrWhiteSpace(configuredApiKey))
        {
            _logger.LogError("Management API key is not configured in appsettings");
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsync("Server configuration error");
            return;
        }

        if (!string.Equals(extractedApiKey, configuredApiKey, StringComparison.Ordinal))
        {
            _logger.LogWarning("Invalid API key provided for management operation: {Method} {Path}", method, path);
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsync("Invalid API key");
            return;
        }

        // API key is valid, continue with the request
        await _next(context);
    }
}

public static class ApiKeyMiddlewareExtensions
{
    public static IApplicationBuilder UseApiKeyValidation(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ApiKeyMiddleware>();
    }
}
