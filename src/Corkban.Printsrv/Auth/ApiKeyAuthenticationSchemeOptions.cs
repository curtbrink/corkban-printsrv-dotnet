using Microsoft.AspNetCore.Authentication;

namespace CorkbanPrintsrv.Auth;

public class ApiKeyAuthenticationSchemeOptions : AuthenticationSchemeOptions
{
    public const string DefaultScheme = "APIKeyAuthentication";
    public const string HeaderName = "X-Corkban-Key";
}