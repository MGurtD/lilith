using Application.Contracts;

namespace Application.Tests.TestSupport;

/// <summary>
/// Language catalog double returning three fixed entries (ca/es/en).
/// Matches the active-languages contract expected by MenuItemService.
/// </summary>
public sealed class FakeLanguageCatalog : ILanguageCatalog
{
    private static readonly LanguageDto[] Languages =
    [
        new(Guid.NewGuid(), "ca", "Català",  "", true,  1),
        new(Guid.NewGuid(), "es", "Español", "", false, 2),
        new(Guid.NewGuid(), "en", "English", "", false, 3),
    ];

    public static FakeLanguageCatalog Instance { get; } = new();

    public Task<IEnumerable<LanguageDto>> GetAllAsync() =>
        Task.FromResult<IEnumerable<LanguageDto>>(Languages);

    public Task<LanguageDto?> GetByCodeAsync(string code) =>
        Task.FromResult(Languages.FirstOrDefault(l =>
            l.Code.Equals(code, StringComparison.OrdinalIgnoreCase)));

    public Task<LanguageDto?> GetDefaultAsync() =>
        Task.FromResult<LanguageDto?>(Languages[0]);
}
