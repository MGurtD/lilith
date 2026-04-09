using Application.Contracts;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;

namespace Api.Setup;

public class ApiKeyAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    IServiceProvider serviceProvider)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    public const string SchemeName = "ApiKey";
    private const string HeaderName = "X-Api-Key";

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(HeaderName, out var rawKey) || string.IsNullOrWhiteSpace(rawKey))
            return AuthenticateResult.NoResult();

        var fullKey = rawKey.ToString().Trim();

        // Expected format: rs_<hexprefix>_<base64secret>
        var parts = fullKey.Split('_');

        // Resolve scoped services from a new scope
        await using var scope = serviceProvider.CreateAsyncScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var localization = scope.ServiceProvider.GetRequiredService<ILocalizationService>();

        if (parts.Length < 3)
            return AuthenticateResult.Fail(localization.GetLocalizedString("ApiKey.InvalidFormat"));

        var keyPrefix = parts[1];
        var keyHash = HashKey(fullKey);

        var apiKey = unitOfWork.ApiKeys
            .Find(k => k.KeyPrefix == keyPrefix && !k.Disabled)
            .FirstOrDefault();

        if (apiKey is null)
            return AuthenticateResult.Fail(localization.GetLocalizedString("ApiKey.NotFoundOrDisabled"));

        if (apiKey.ExpiresOn.HasValue && apiKey.ExpiresOn.Value < DateTime.UtcNow)
            return AuthenticateResult.Fail(localization.GetLocalizedString("ApiKey.Expired"));

        if (!string.Equals(apiKey.KeyHash, keyHash, StringComparison.OrdinalIgnoreCase))
            return AuthenticateResult.Fail(localization.GetLocalizedString("ApiKey.NotFoundOrDisabled"));

        // Update LastUsedOn (fire-and-forget, non-critical)
        apiKey.LastUsedOn = DateTime.UtcNow;
        await unitOfWork.ApiKeys.Update(apiKey);

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, apiKey.Name),
            new(ClaimTypes.NameIdentifier, apiKey.Id.ToString()),
            new("apikey_prefix", apiKey.KeyPrefix),
        };

        if (!string.IsNullOrWhiteSpace(apiKey.Scopes))
        {
            foreach (var scope2 in apiKey.Scopes.Split([',', ' '], StringSplitOptions.RemoveEmptyEntries))
                claims.Add(new Claim("scope", scope2.Trim()));
        }

        var identity = new ClaimsIdentity(claims, SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, SchemeName);

        return AuthenticateResult.Success(ticket);
    }

    private static string HashKey(string apiKey)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(apiKey));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
