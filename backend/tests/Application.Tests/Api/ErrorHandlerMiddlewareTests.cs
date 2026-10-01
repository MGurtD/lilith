using System.Text.Json;
using Api.Middlewares;
using Application.Contracts;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;

namespace Application.Tests.Api;

/// <summary>
/// Exception text can carry EF and PostgreSQL details, so error responses show it only in
/// Development; elsewhere the user sees the localized title and the trace id.
/// </summary>
public class ErrorHandlerMiddlewareTests
{
    private static readonly DbUpdateException DatabaseFailure =
        new("An error occurred while saving the entity changes.",
            new Exception("23505: duplicate key value violates unique constraint \"IX_Customers_Code\""));

    [Fact]
    public async Task Outside_development_errors_carry_only_the_localized_title()
    {
        var response = await Handle(DatabaseFailure, Environments.Production);

        Assert.Equal(409, response.GetProperty("status").GetInt32());
        Assert.Equal("Conflict", response.GetProperty("title").GetString());
        Assert.Equal("Conflict", response.GetProperty("detail").GetString());
        Assert.Equal(["Conflict"], Errors(response));
    }

    [Fact]
    public async Task In_development_errors_carry_the_exception_text()
    {
        var response = await Handle(DatabaseFailure, Environments.Development);

        Assert.Equal(
            [DatabaseFailure.Message, DatabaseFailure.InnerException!.Message],
            Errors(response));
    }

    [Fact]
    public async Task A_refused_delete_carries_its_reason_in_every_environment()
    {
        var response = await Handle(
            new EntityInUseException("ACME", ["DocumentKind.Budgets"]), Environments.Production);

        Assert.Equal(409, response.GetProperty("status").GetInt32());
        Assert.Equal(["In use: ACME, Budgets"], Errors(response));
    }

    [Fact]
    public async Task A_refused_delete_suggests_deactivating_when_the_record_can_be_disabled()
    {
        var response = await Handle(
            new EntityInUseException("ACME", ["DocumentKind.Budgets"], canBeDisabled: true), Environments.Production);

        Assert.Equal(["In use: ACME, Budgets Deactivate it."], Errors(response));
    }

    private static async Task<JsonElement> Handle(Exception exception, string environmentName)
    {
        var localization = Substitute.For<ILocalizationService>();
        localization.GetLocalizedString("ErrorResponse.Conflict").Returns("Conflict");
        localization.GetLocalizedString("DocumentKind.Budgets").Returns("Budgets");
        localization.GetLocalizedString("MasterData.DisableInstead").Returns("Deactivate it.");
        localization.GetLocalizedString("MasterData.InUseNamed", Arg.Any<object[]>())
            .Returns(call => $"In use: {string.Join(", ", call.ArgAt<object[]>(1))}");

        var environment = Substitute.For<IWebHostEnvironment>();
        environment.EnvironmentName.Returns(environmentName);

        var middleware = new ErrorHandlerMiddleware(
            _ => throw exception, NullLoggerFactory.Instance, localization, environment);

        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        await middleware.Invoke(context);

        context.Response.Body.Position = 0;
        return (await JsonDocument.ParseAsync(context.Response.Body)).RootElement;
    }

    private static List<string?> Errors(JsonElement response) =>
        response.GetProperty("errors").EnumerateArray().Select(e => e.GetString()).ToList();
}
