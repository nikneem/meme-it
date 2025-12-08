namespace HexMaster.MemeIt.Memes.Api.Filters;

public class ApiKeyEndpointFilter : IEndpointFilter
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<ApiKeyEndpointFilter> _logger;
    private const string ApiKeyHeaderName = "X-ApiKey";

    public ApiKeyEndpointFilter(IConfiguration configuration, ILoggerFactory loggerFactory)
    {
        _configuration = configuration;
        _logger = loggerFactory.CreateLogger<ApiKeyEndpointFilter>();
    }

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        if (context.HttpContext.Request.Method == "OPTIONS")
        {
            return await next(context);
        }

        if (!context.HttpContext.Request.Headers.TryGetValue(ApiKeyHeaderName, out var extractedApiKey))
        {
            _logger.LogWarning("API key missing for management operation: {Method} {Path}",
                context.HttpContext.Request.Method, context.HttpContext.Request.Path);
            return Results.Unauthorized();
        }

        var configuredApiKey = _configuration["Management:ApiKey"];
        if (string.IsNullOrWhiteSpace(configuredApiKey))
        {
            _logger.LogError("Management API key is not configured in appsettings");
            return Results.Problem("Server configuration error", statusCode: 500);
        }

        if (!string.Equals(extractedApiKey, configuredApiKey, StringComparison.Ordinal))
        {
            _logger.LogWarning("Invalid API key provided for management operation: {Method} {Path}",
                context.HttpContext.Request.Method, context.HttpContext.Request.Path);
            return Results.StatusCode(StatusCodes.Status403Forbidden);
        }

        return await next(context);
    }
}
