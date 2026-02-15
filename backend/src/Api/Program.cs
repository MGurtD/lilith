using Api;
using Api.Middlewares;
using Api.Setup;
using Serilog;
using System.Text.Json.Serialization;
using Application.Contracts; // ILanguageCatalog
using Application.Services.System; // LanguageCatalog

// Configure Serilog bootstrap logger for early startup logging
SerilogSetup.ConfigureBootstrapLogger();

try
{
    Log.Information("Starting Lilith Backend API");
    
    var builder = WebApplication.CreateBuilder(args);
    {
        // Add Serilog (configuration read from appsettings.json)
        builder.Host.AddSerilogSetup();

        // Configuration and settings
        builder.Services.Configure<AppSettings>(builder.Configuration);
        var appSettings = builder.Configuration.Get<AppSettings>()!;
        appSettings.Validate();

        // Set server request body size limit
        builder.WebHost.ConfigureKestrel(options =>
        {
            options.Limits.MaxRequestBodySize = appSettings.FileManagment.LimitSize * 1024L * 1024L;
        });

        builder.Services
            .AddDatabaseServices(appSettings.ConnectionStrings.Default)
            .AddApplicationServices()
            .AddJwtSetup(builder.Environment.IsDevelopment(), appSettings.JwtConfig.Secret)
            .AddSwaggerSetup()
            .AddLocalizationSetup(); // Add localization services

        // Language catalog singleton
        builder.Services.AddSingleton<ILanguageCatalog, LanguageCatalog>();

        builder.Services.AddControllers()
                        .AddJsonOptions(x => x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);
    }

    var app = builder.Build();
    {
        // Serilog HTTP request logging
        app.UseSerilogRequestLogging();
        
        // Correlation ID middleware for request tracking
        app.UseMiddleware<CorrelationMiddleware>();
        
        app.UseSwagger();
        app.UseSwaggerUI();

        app.UseHttpsRedirection();

        // Ensure authentication happens before culture resolution so we can read the user's locale claim
        app.UseAuthentication();
        app.UseLocalizationSetup();
        app.UseAuthorization();

        // Enhanced error handling middleware (after authentication/authorization)
        app.UseMiddleware<ErrorHandlerMiddleware>();

        // global cors policy
        app.UseCors(x => x
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());

        app.MapControllers();
        app.Run();
    }
}
catch (Exception exception)
{
    Log.Fatal(exception, "Application terminated unexpectedly");
    throw;
}
finally
{
    Log.CloseAndFlush();
}