using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Api.Setup;

public static class JwtSetup
{
    private const string PolicySchemeName = "JwtOrApiKey";

    public static IServiceCollection AddJwtSetup(this IServiceCollection services, bool isDevelopment, string jwtSecret)
    {
        // JWT Service    
        var signKey = Encoding.ASCII.GetBytes(jwtSecret);
        var tokenValidationParameters = new TokenValidationParameters()
        {
            IssuerSigningKey = new SymmetricSecurityKey(signKey),
            ValidateIssuer = !isDevelopment,
            ValidateIssuerSigningKey = true,
            ValidateAudience = !isDevelopment,
            RequireExpirationTime = !isDevelopment,
            ValidateLifetime = true,
        };

        services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = PolicySchemeName;
                options.DefaultScheme = PolicySchemeName;
                options.DefaultChallengeScheme = PolicySchemeName;
            })
            // PolicyScheme: route to ApiKey handler if X-Api-Key header present, otherwise JWT
            .AddPolicyScheme(PolicySchemeName, PolicySchemeName, policyOptions =>
            {
                policyOptions.ForwardDefaultSelector = context =>
                    context.Request.Headers.ContainsKey("X-Api-Key")
                        ? ApiKeyAuthenticationHandler.SchemeName
                        : JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(jwt =>
            {
                jwt.SaveToken = true;
                jwt.TokenValidationParameters = tokenValidationParameters;
            })
            .AddScheme<AuthenticationSchemeOptions, ApiKeyAuthenticationHandler>(
                ApiKeyAuthenticationHandler.SchemeName, _ => { });

        services.AddSingleton(tokenValidationParameters);

        return services;
    }
}

