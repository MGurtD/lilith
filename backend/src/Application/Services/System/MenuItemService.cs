using System.Globalization;
using Application.Contracts;
using Domain.Entities.Auth;

namespace Application.Services.System;

public class MenuItemService(
    IUnitOfWork unitOfWork,
    ILocalizationService localization,
    ILanguageCatalog languageCatalog) : IMenuItemService
{
    public async Task<GenericResponse> GetAll(bool hierarchy = false)
    {
        var items = (await unitOfWork.MenuItems.GetAll()).OrderBy(i => i.SortOrder).ToList();
        var translations = await unitOfWork.MenuItemTranslations.FindAsync(t => !t.Disabled);
        var translationsByMenu = translations.ToLookup(t => t.MenuItemId);
        var defaultLanguageCode = (await languageCatalog.GetDefaultAsync())?.Code;
        var cultureCode = CurrentCultureCode();

        var dtos = items
            .Select(item => ToDto(item, translationsByMenu[item.Id], cultureCode, defaultLanguageCode))
            .ToList();

        if (!hierarchy)
            return new GenericResponse(true, dtos);

        var nodes = dtos.ToDictionary(i => i.Id, ToNode);
        var roots = new List<MenuItemNodeDto>();
        foreach (var node in nodes.Values)
        {
            if (node.ParentId.HasValue && nodes.TryGetValue(node.ParentId.Value, out var parent))
                parent.Children.Add(node);
            else
                roots.Add(node);
        }

        return new GenericResponse(true, roots);
    }

    public async Task<GenericResponse> Get(Guid id)
    {
        var item = await unitOfWork.MenuItems.Get(id);
        if (item is null)
            return new GenericResponse(false, localization.GetLocalizedString("MenuItemNotFound", id));

        var translations = await unitOfWork.MenuItemTranslations.FindAsync(t => t.MenuItemId == id && !t.Disabled);
        var defaultLanguageCode = (await languageCatalog.GetDefaultAsync())?.Code;
        return new GenericResponse(true, ToDto(item, translations, CurrentCultureCode(), defaultLanguageCode));
    }

    public async Task<GenericResponse> Create(CreateMenuItemRequest request)
    {
        var normalizedTranslations = await ValidateTranslations(request.Translations);
        if (normalizedTranslations is null)
            return InvalidTranslationsResponse();

        if ((await unitOfWork.MenuItems.FindAsync(m => m.Key == request.Key)).Count > 0)
            return new GenericResponse(false, localization.GetLocalizedString("MenuItemKeyExists", request.Key));

        var hierarchyError = await ValidateParent(request.Id, request.ParentId);
        if (hierarchyError is not null)
            return hierarchyError;

        var item = new MenuItem
        {
            Id = request.Id,
            Key = request.Key,
            Icon = request.Icon,
            Route = request.Route,
            ParentId = request.ParentId,
            SortOrder = request.SortOrder
        };
        var translations = normalizedTranslations.Select(t => new MenuItemTranslation
        {
            MenuItemId = item.Id,
            LanguageCode = t.Key,
            Title = t.Value
        }).ToList();

        await unitOfWork.MenuItems.AddWithoutSave(item);
        await unitOfWork.MenuItemTranslations.AddRangeWithoutSave(translations);
        await unitOfWork.CompleteAsync();

        var defaultLanguageCode = (await languageCatalog.GetDefaultAsync())?.Code;
        return new GenericResponse(true, ToDto(item, translations, CurrentCultureCode(), defaultLanguageCode));
    }

    public async Task<GenericResponse> Update(UpdateMenuItemRequest request)
    {
        var normalizedTranslations = await ValidateTranslations(request.Translations);
        if (normalizedTranslations is null)
            return InvalidTranslationsResponse();

        var current = await unitOfWork.MenuItems.Get(request.Id);
        if (current is null)
            return new GenericResponse(false, localization.GetLocalizedString("MenuItemNotFound", request.Id));

        if (current.Key != request.Key && (await unitOfWork.MenuItems.FindAsync(m => m.Key == request.Key)).Count > 0)
            return new GenericResponse(false, localization.GetLocalizedString("MenuItemKeyExists", request.Key));

        var hierarchyError = await ValidateParent(request.Id, request.ParentId);
        if (hierarchyError is not null)
            return hierarchyError;

        current.Key = request.Key;
        current.Icon = request.Icon;
        current.Route = request.Route;
        current.ParentId = request.ParentId;
        current.SortOrder = request.SortOrder;
        unitOfWork.MenuItems.UpdateWithoutSave(current);

        var existingTranslations = await unitOfWork.MenuItemTranslations.FindAsync(t => t.MenuItemId == request.Id);
        foreach (var translation in normalizedTranslations)
        {
            var existing = existingTranslations.FirstOrDefault(t =>
                t.LanguageCode.Equals(translation.Key, StringComparison.OrdinalIgnoreCase));
            if (existing is null)
            {
                existing = new MenuItemTranslation
                {
                    MenuItemId = current.Id,
                    LanguageCode = translation.Key,
                    Title = translation.Value
                };
                await unitOfWork.MenuItemTranslations.AddWithoutSave(existing);
                existingTranslations.Add(existing);
            }
            else
            {
                existing.LanguageCode = translation.Key;
                existing.Title = translation.Value;
                existing.Disabled = false;
                unitOfWork.MenuItemTranslations.UpdateWithoutSave(existing);
            }
        }

        await unitOfWork.CompleteAsync();

        var defaultLanguageCode = (await languageCatalog.GetDefaultAsync())?.Code;
        return new GenericResponse(true, ToDto(current, existingTranslations, CurrentCultureCode(), defaultLanguageCode));
    }

    public async Task<GenericResponse> GetTranslationMatrix()
    {
        var languages = (await languageCatalog.GetAllAsync())
            .GroupBy(l => l.Code, StringComparer.OrdinalIgnoreCase)
            .Select(group => group.First())
            .OrderBy(l => l.SortOrder)
            .ThenBy(l => l.Name)
            .ToList();
        var items = (await unitOfWork.MenuItems.GetAll()).ToList();
        var translations = await unitOfWork.MenuItemTranslations.FindAsync(t => !t.Disabled);
        var translationsByMenu = translations.ToLookup(t => t.MenuItemId);
        var rows = BuildMatrixRows(items, languages, translationsByMenu);

        return new GenericResponse(true, new MenuItemTranslationMatrixDto(languages, rows));
    }

    public async Task<GenericResponse> UpdateTranslations(UpdateMenuItemTranslationsRequest request)
    {
        if (request.Items is null || request.Items.Count == 0)
            return TranslationBatchError("MenuItemTranslationBatchEmpty");

        var languages = (await languageCatalog.GetAllAsync()).ToList();
        var activeCodes = languages
            .Select(l => l.Code.Trim().ToLowerInvariant())
            .Distinct()
            .ToHashSet();
        var requestedIds = request.Items.Select(i => i.MenuItemId).ToList();
        if (requestedIds.Count != requestedIds.Distinct().Count())
            return TranslationBatchError("MenuItemTranslationBatchDuplicateMenu");

        var menuItems = await unitOfWork.MenuItems.FindAsync(m => requestedIds.Contains(m.Id));
        var menuItemIds = menuItems.Select(m => m.Id).ToHashSet();
        var missingMenuItemId = requestedIds
            .Where(id => !menuItemIds.Contains(id))
            .Select(id => (Guid?)id)
            .FirstOrDefault();
        if (missingMenuItemId.HasValue)
            return new GenericResponse(false, localization.GetLocalizedString("MenuItemNotFound", missingMenuItemId.Value));

        var normalizedCells = new List<(Guid MenuItemId, string LanguageCode, string Title)>();
        var cellKeys = new HashSet<(Guid MenuItemId, string LanguageCode)>();
        foreach (var item in request.Items)
        {
            if (item.Translations is null || item.Translations.Count == 0)
                return TranslationBatchError("MenuItemTranslationBatchEmptyRow");

            foreach (var translation in item.Translations)
            {
                var languageCode = translation.LanguageCode?.Trim().ToLowerInvariant() ?? string.Empty;
                var title = translation.Title?.Trim() ?? string.Empty;
                if (!activeCodes.Contains(languageCode))
                    return new GenericResponse(false, localization.GetLocalizedString("MenuItemTranslationLanguageInvalid", languageCode));
                if (string.IsNullOrWhiteSpace(title) || title.Length > 250)
                    return new GenericResponse(false, localization.GetLocalizedString("MenuItemTranslationTitleInvalid", languageCode));
                if (!cellKeys.Add((item.MenuItemId, languageCode)))
                    return TranslationBatchError("MenuItemTranslationBatchDuplicateCell");

                normalizedCells.Add((item.MenuItemId, languageCode, title));
            }
        }

        var existingTranslations = await unitOfWork.MenuItemTranslations
            .FindAsync(t => requestedIds.Contains(t.MenuItemId));
        foreach (var cell in normalizedCells)
        {
            var existing = existingTranslations.FirstOrDefault(t =>
                t.MenuItemId == cell.MenuItemId
                && t.LanguageCode.Equals(cell.LanguageCode, StringComparison.OrdinalIgnoreCase));
            if (existing is null)
            {
                existing = new MenuItemTranslation
                {
                    MenuItemId = cell.MenuItemId,
                    LanguageCode = cell.LanguageCode,
                    Title = cell.Title
                };
                await unitOfWork.MenuItemTranslations.AddWithoutSave(existing);
                existingTranslations.Add(existing);
            }
            else
            {
                existing.LanguageCode = cell.LanguageCode;
                existing.Title = cell.Title;
                existing.Disabled = false;
                unitOfWork.MenuItemTranslations.UpdateWithoutSave(existing);
            }
        }

        await unitOfWork.CompleteAsync();
        return new GenericResponse(true, new UpdateMenuItemTranslationsResult(request.Items.Count, normalizedCells.Count));
    }

    public async Task<GenericResponse> Delete(Guid id)
    {
        var item = await unitOfWork.MenuItems.Get(id);
        if (item is null)
            return new GenericResponse(false, localization.GetLocalizedString("MenuItemNotFound", id));

        if ((await unitOfWork.MenuItems.FindAsync(m => m.ParentId == id)).Count > 0)
            return new GenericResponse(false, localization.GetLocalizedString("InvalidMenuHierarchy"));

        await unitOfWork.MenuItems.Remove(item);
        return new GenericResponse(true, true);
    }

    private async Task<Dictionary<string, string>?> ValidateTranslations(IReadOnlyList<MenuItemTranslationDto>? translations)
    {
        if (translations is null)
            return null;

        var activeCodes = (await languageCatalog.GetAllAsync())
            .Select(l => l.Code.Trim().ToLowerInvariant())
            .Distinct()
            .ToHashSet();
        var normalized = new Dictionary<string, string>();

        foreach (var translation in translations)
        {
            var code = translation.LanguageCode?.Trim().ToLowerInvariant() ?? string.Empty;
            var title = translation.Title?.Trim() ?? string.Empty;
            if (!activeCodes.Contains(code) || string.IsNullOrWhiteSpace(title) || !normalized.TryAdd(code, title))
                return null;
        }

        return normalized.Count == activeCodes.Count ? normalized : null;
    }

    private async Task<GenericResponse?> ValidateParent(Guid itemId, Guid? parentId)
    {
        if (!parentId.HasValue)
            return null;
        if (parentId.Value == itemId)
            return new GenericResponse(false, localization.GetLocalizedString("InvalidMenuHierarchy"));

        var walker = await unitOfWork.MenuItems.Get(parentId.Value);
        if (walker is null)
            return new GenericResponse(false, localization.GetLocalizedString("MenuItemNotFound", parentId.Value));

        while (walker is not null)
        {
            if (walker.Id == itemId)
                return new GenericResponse(false, localization.GetLocalizedString("InvalidMenuHierarchy"));
            walker = walker.ParentId.HasValue ? await unitOfWork.MenuItems.Get(walker.ParentId.Value) : null;
        }

        return null;
    }

    private GenericResponse InvalidTranslationsResponse() =>
        new(false, localization.GetLocalizedString("MenuItemTranslationsInvalid"));

    private GenericResponse TranslationBatchError(string key) =>
        new(false, localization.GetLocalizedString(key));

    private static List<MenuItemTranslationMatrixRowDto> BuildMatrixRows(
        IReadOnlyList<MenuItem> items,
        IReadOnlyList<LanguageDto> languages,
        ILookup<Guid, MenuItemTranslation> translationsByMenu)
    {
        var itemIds = items.Select(i => i.Id).ToHashSet();
        var childrenByParent = items
            .Where(i => i.ParentId.HasValue && itemIds.Contains(i.ParentId.Value))
            .ToLookup(i => i.ParentId!.Value);
        var roots = items
            .Where(i => !i.ParentId.HasValue || !itemIds.Contains(i.ParentId.Value))
            .OrderBy(i => i.SortOrder)
            .ThenBy(i => i.Key);
        var rows = new List<MenuItemTranslationMatrixRowDto>();
        var visited = new HashSet<Guid>();

        void AddRow(MenuItem item, int depth)
        {
            if (!visited.Add(item.Id))
                return;

            var itemTranslations = translationsByMenu[item.Id].ToList();
            rows.Add(new MenuItemTranslationMatrixRowDto(
                item.Id,
                item.Key,
                item.Route,
                item.ParentId,
                item.SortOrder,
                item.Disabled,
                depth,
                languages.Select(language => new MenuItemTranslationDto(
                    language.Code.ToLowerInvariant(),
                    itemTranslations.FirstOrDefault(t => t.LanguageCode.Equals(language.Code, StringComparison.OrdinalIgnoreCase))?.Title
                        ?? string.Empty)).ToList()));

            foreach (var child in childrenByParent[item.Id].OrderBy(i => i.SortOrder).ThenBy(i => i.Key))
                AddRow(child, depth + 1);
        }

        foreach (var root in roots)
            AddRow(root, 0);
        foreach (var item in items.Where(i => !visited.Contains(i.Id)).OrderBy(i => i.SortOrder).ThenBy(i => i.Key))
            AddRow(item, 0);

        return rows;
    }

    private static string CurrentCultureCode()
    {
        var name = CultureInfo.CurrentUICulture.Name;
        return (string.IsNullOrWhiteSpace(name) ? "ca" : name.Split('-')[0]).ToLowerInvariant();
    }

    private static MenuItemDto ToDto(
        MenuItem item,
        IEnumerable<MenuItemTranslation> translations,
        string cultureCode,
        string? defaultLanguageCode)
    {
        var translationDtos = translations
            .Where(t => !t.Disabled)
            .OrderBy(t => t.LanguageCode)
            .Select(t => new MenuItemTranslationDto(t.LanguageCode, t.Title))
            .ToList();
        var title = translationDtos.FirstOrDefault(t => t.LanguageCode.Equals(cultureCode, StringComparison.OrdinalIgnoreCase))?.Title
            ?? translationDtos.FirstOrDefault(t => t.LanguageCode.Equals(defaultLanguageCode, StringComparison.OrdinalIgnoreCase))?.Title
            ?? translationDtos.FirstOrDefault()?.Title
            ?? item.Key;

        return new MenuItemDto(
            item.Id,
            item.Key,
            title,
            item.Icon,
            item.Route,
            item.SortOrder,
            item.ParentId,
            item.Disabled,
            translationDtos);
    }

    private static MenuItemNodeDto ToNode(MenuItemDto item) => new(
        item.Id,
        item.Key,
        item.Title,
        item.Icon,
        item.Route,
        item.SortOrder,
        item.ParentId,
        item.Disabled,
        item.Translations,
        []);
}
