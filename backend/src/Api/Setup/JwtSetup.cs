using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Api.Setup;

public static class JwtSetup
{
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
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(jwt =>
            {
                jwt.SaveToken = true; // After authentication, token will be saved
                jwt.TokenValidationParameters = tokenValidationParameters;
            });
        services.AddSingleton(tokenValidationParameters);

        return services;
    }
}

