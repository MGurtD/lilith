using Application.Contracts;
using Microsoft.Extensions.Hosting;
using System.Text.Json;

namespace Application.Services.System
{
    public class LanguageCatalog(IHostEnvironment env) : ILanguageCatalog
    {
        private readonly Lazy<Task<IReadOnlyList<LanguageDto>>> _languagesLazy = new(() => LoadAsync(Path.Combine(env.ContentRootPath, RelativePath)));
        private const string RelativePath = "Resources/Languages/languages.json";

        private static async Task<IReadOnlyList<LanguageDto>> LoadAsync(string path)
        {
            if (!File.Exists(path)) return [];
            await using var stream = File.OpenRead(path);
            var items = await JsonSerializer.DeserializeAsync<List<LanguageFileItem>>(stream, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();
            // Basic sanitation & ensure single default
            var list = items
                .Select(i => new LanguageDto(i.Id, i.Code.ToLowerInvariant(), i.Name, i.Icon ?? string.Empty, i.IsDefault, i.SortOrder))
                .OrderBy(l => l.SortOrder).ThenBy(l => l.Name)
                .ToList();
            if (list.Count(l => l.IsDefault) != 1 && list.Count > 0)
            {
                // Force first as default if invalid config
                for (int i = 0; i < list.Count; i++)
                    list[i] = list[i] with { IsDefault = i == 0 };
            }
            return list;
        }

        private async Task<IReadOnlyList<LanguageDto>> GetAllInternalAsync() => await _languagesLazy.Value;

        public async Task<IEnumerable<LanguageDto>> GetAllAsync() => await GetAllInternalAsync();

        public async Task<LanguageDto?> GetByCodeAsync(string code)
        {
            var all = await GetAllInternalAsync();
            return all.FirstOrDefault(l => l.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
        }

        public async Task<LanguageDto?> GetDefaultAsync()
        {
            var all = await GetAllInternalAsync();
            return all.FirstOrDefault(l => l.IsDefault) ?? all.FirstOrDefault();
        }

        private record LanguageFileItem(Guid Id, string Code, string Name, string? Icon, bool IsDefault, int SortOrder);
    }
}





