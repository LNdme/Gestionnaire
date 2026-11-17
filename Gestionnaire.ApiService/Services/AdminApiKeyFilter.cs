using System.Security.Cryptography;
using System.Text;

namespace Gestionnaire.ApiService.Services;

public sealed class AdminApiKeyFilter(IConfiguration configuration) : IEndpointFilter
{
    public ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var configuredKey = configuration["SiteContent:AdminApiKey"];
        if (string.IsNullOrWhiteSpace(configuredKey))
        {
            return ValueTask.FromResult<object?>(Results.Problem(
                "La clé d'administration de l'API n'est pas configurée.",
                statusCode: StatusCodes.Status503ServiceUnavailable));
        }

        var suppliedKey = context.HttpContext.Request.Headers["X-API-Key"].ToString();
        var suppliedBytes = Encoding.UTF8.GetBytes(suppliedKey);
        var configuredBytes = Encoding.UTF8.GetBytes(configuredKey);
        if (string.IsNullOrEmpty(suppliedKey) ||
            !CryptographicOperations.FixedTimeEquals(suppliedBytes, configuredBytes))
        {
            return ValueTask.FromResult<object?>(Results.Unauthorized());
        }

        return next(context);
    }
}
