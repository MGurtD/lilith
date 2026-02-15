using Application.Services.System;
using Microsoft.Extensions.Localization;
using System.Globalization;

namespace Api.Setup
{
    public static class LocalizationSetup
    {
        public static IServiceCollection AddLocalizationSetup(this IServiceCollection services)
        {
            // Configure localization options
            services.Configure<LocalizationOptions>(options =>
            {
                options.ResourcesPath = "Resources";
            });

            // Register custom JSON localizer factory
            services.AddSingleton<IStringLocalizerFactory, JsonStringLocalizerFactory>();
            
            // Register localization service
            services.AddSingleton<Application.Contracts.ILocalizationService, LocalizationService>();

            // Configure supported cultures
            services.Configure<RequestLocalizationOptions>(options =>
            {
                var supportedCultures = new[]
                {
                    new CultureInfo("ca"),    // Catalan (default)
                    new CultureInfo("es"),    // Spanish
                    new CultureInfo("en")     // English
                };

                options.DefaultRequestCulture = new Microsoft.AspNetCore.Localization.RequestCulture("ca");
                options.SupportedCultures = supportedCultures;
                options.SupportedUICultures = supportedCultures;
            });

            return services;
        }

        public static IApplicationBuilder UseLocalizationSetup(this IApplicationBuilder app)
        {
            // Use built-in request localization first
            app.UseRequestLocalization();
            
            // Then enforce our custom culture resolution (query > claim > header > default)
            app.UseMiddleware<Middlewares.CultureMiddleware>();

            return app;
        }
    }
}