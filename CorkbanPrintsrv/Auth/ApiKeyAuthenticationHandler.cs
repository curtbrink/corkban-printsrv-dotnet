using System.Security.Claims;
using System.Text.Encodings.Web;
using CorkbanPrintsrv.Configuration;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace CorkbanPrintsrv.Auth;

public class ApiKeyAuthenticationHandler : AuthenticationHandler<ApiKeyAuthenticationSchemeOptions>
{
    private readonly string _apiKey;

    public ApiKeyAuthenticationHandler(IOptions<PrinterConfiguration> printerConfig,
        IOptionsMonitor<ApiKeyAuthenticationSchemeOptions> options, ILoggerFactory logger,
        UrlEncoder encoder) : base(options, logger, encoder)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(printerConfig.Value.SecretKey);

        _apiKey = printerConfig.Value.SecretKey;
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var headerKey = ApiKeyAuthenticationSchemeOptions.HeaderName;
        var requestApiKey = Request.Headers[headerKey].FirstOrDefault();

        if (string.IsNullOrWhiteSpace(requestApiKey))
            return Task.FromResult(AuthenticateResult.Fail($"Missing {headerKey} header"));

        if (requestApiKey != _apiKey)
            return Task.FromResult(AuthenticateResult.Fail($"Invalid {headerKey} header value"));

        List<Claim> claims = [new("Corkban", "Authenticated")];
        var claimsIdentity = new ClaimsIdentity(claims, Scheme.Name);
        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
        return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(claimsPrincipal, Scheme.Name)));
    }
}