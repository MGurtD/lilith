namespace Application.Contracts;

using Application.Contracts;

public interface ILanguageCatalog
{
    Task<IEnumerable<LanguageDto>> GetAllAsync();
    Task<LanguageDto?> GetByCodeAsync(string code);
    Task<LanguageDto?> GetDefaultAsync();
}
