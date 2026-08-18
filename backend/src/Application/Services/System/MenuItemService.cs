using System.Globalization;
using System.Text.Json;
using Application.Contracts;
using Domain.Entities.Auth;

namespace Application.Services.System;

public class MenuItemService(
    IUnitOfWork unitOfWork,
    ILocalizationService localization,
    ILanguageCatalog languageCatalog) : IMenuItemService
{
    private const int TransferVersion = 1;
    private const long MaxImportFileSize = 5 * 1024 * 1024;

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

    public async Task<GenericResponse> Export()
    {
        var menuItems = (await unitOfWork.MenuItems.GetAll()).ToList();
        var duplicateKey = menuItems
            .GroupBy(item => item.Key, StringComparer.Ordinal)
            .FirstOrDefault(group => group.Count() > 1)?.Key;
        if (duplicateKey is not null)
            return TransferError("MenuItemTransferDuplicateKey", duplicateKey);

        var itemsById = menuItems.ToDictionary(item => item.Id);
        var activeCodes = await GetActiveLanguageCodes();
        var translations = await unitOfWork.MenuItemTranslations.FindAsync(t => !t.Disabled);
        var translationsByMenu = translations.ToLookup(t => t.MenuItemId);
        var transferItems = new List<MenuItemTransferItem>(menuItems.Count);

        foreach (var item in menuItems.OrderBy(item => item.SortOrder).ThenBy(item => item.Key))
        {
            string? parentKey = null;
            if (item.ParentId.HasValue)
            {
                if (!itemsById.TryGetValue(item.ParentId.Value, out var parent))
                    return TransferError("MenuItemTransferParentInvalid", item.Key);
                parentKey = parent.Key;
            }

            var itemTranslations = translationsByMenu[item.Id].ToList();
            transferItems.Add(new MenuItemTransferItem(
                item.Key,
                parentKey,
                item.Icon,
                item.Route,
                item.SortOrder,
                item.Disabled,
                activeCodes
                    .OrderBy(code => code)
                    .Select(code => new MenuItemTranslationDto(
                        code,
                        itemTranslations.FirstOrDefault(translation =>
                            translation.LanguageCode.Equals(code, StringComparison.OrdinalIgnoreCase))?.Title
                            ?? string.Empty))
                    .ToList()));
        }

        var document = new MenuItemTransferDocument(TransferVersion, transferItems);
        var validation = ValidateTransferDocument(document, activeCodes);
        return validation.Error ?? new GenericResponse(true, ToTransferDocument(validation.Items!));
    }

    public async Task<GenericResponse> Import(Stream? content)
    {
        if (content is null || !content.CanRead || (content.CanSeek && content.Length == 0))
            return TransferError("MenuItemTransferFileInvalid");
        if (content.CanSeek && content.Length > MaxImportFileSize)
            return TransferError("MenuItemTransferFileTooLarge");

        MenuItemTransferDocument? document;
        try
        {
            document = await JsonSerializer.DeserializeAsync<MenuItemTransferDocument>(
                content,
                new JsonSerializerOptions(JsonSerializerDefaults.Web));
        }
        catch (JsonException)
        {
            return TransferError("MenuItemTransferFileInvalid");
        }
        catch (NotSupportedException)
        {
            return TransferError("MenuItemTransferFileInvalid");
        }

        if (document is null)
            return TransferError("MenuItemTransferFileInvalid");

        var activeCodes = await GetActiveLanguageCodes();
        var validation = ValidateTransferDocument(document, activeCodes);
        if (validation.Error is not null)
            return validation.Error;

        var importedItems = validation.Items!;
        var existingItems = (await unitOfWork.MenuItems.GetAll()).ToList();
        var duplicateExistingKey = existingItems
            .GroupBy(item => item.Key, StringComparer.Ordinal)
            .FirstOrDefault(group => group.Count() > 1)?.Key;
        if (duplicateExistingKey is not null)
            return TransferError("MenuItemTransferDuplicateKey", duplicateExistingKey);

        var existingByKey = existingItems.ToDictionary(item => item.Key, StringComparer.Ordinal);
        var entitiesByKey = importedItems.ToDictionary(
            item => item.Key,
            item => existingByKey.GetValueOrDefault(item.Key) ?? new MenuItem(),
            StringComparer.Ordinal);
        var affectedIds = entitiesByKey.Values.Select(item => item.Id).ToHashSet();
        var existingTranslations = await unitOfWork.MenuItemTranslations
            .FindAsync(translation => affectedIds.Contains(translation.MenuItemId));
        var createdItems = 0;
        var updatedItems = 0;
        var updatedTranslations = 0;

        // Validation is complete. Stage the entire import and save it as one EF transaction.
        foreach (var imported in importedItems)
        {
            var entity = entitiesByKey[imported.Key];
            var exists = existingByKey.ContainsKey(imported.Key);
            entity.Key = imported.Key;
            entity.Icon = imported.Icon;
            entity.Route = imported.Route;
            entity.SortOrder = imported.SortOrder;
            entity.Disabled = imported.Disabled;
            entity.ParentId = imported.ParentKey is null ? null : entitiesByKey[imported.ParentKey].Id;

            if (exists)
            {
                unitOfWork.MenuItems.UpdateWithoutSave(entity);
                updatedItems++;
            }
            else
            {
                await unitOfWork.MenuItems.AddWithoutSave(entity);
                createdItems++;
            }

            foreach (var importedTranslation in imported.Translations)
            {
                var translation = existingTranslations.FirstOrDefault(existing =>
                    existing.MenuItemId == entity.Id
                    && existing.LanguageCode.Equals(importedTranslation.Key, StringComparison.OrdinalIgnoreCase));
                if (translation is null)
                {
                    translation = new MenuItemTranslation
                    {
                        MenuItemId = entity.Id,
                        LanguageCode = importedTranslation.Key,
                        Title = importedTranslation.Value
                    };
                    await unitOfWork.MenuItemTranslations.AddWithoutSave(translation);
                    existingTranslations.Add(translation);
                }
                else
                {
                    translation.LanguageCode = importedTranslation.Key;
                    translation.Title = importedTranslation.Value;
                    translation.Disabled = false;
                    unitOfWork.MenuItemTranslations.UpdateWithoutSave(translation);
                }
                updatedTranslations++;
            }
        }

        await unitOfWork.CompleteAsync();
        return new GenericResponse(
            true,
            new MenuItemImportResult(createdItems, updatedItems, updatedTranslations));
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

    private async Task<HashSet<string>> GetActiveLanguageCodes() =>
        (await languageCatalog.GetAllAsync())
            .Select(language => language.Code.Trim().ToLowerInvariant())
            .Where(code => code.Length > 0)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

    private (List<NormalizedTransferItem>? Items, GenericResponse? Error) ValidateTransferDocument(
        MenuItemTransferDocument document,
        HashSet<string> activeCodes)
    {
        if (document.Version != TransferVersion)
            return (null, TransferError("MenuItemTransferVersionInvalid", document.Version));
        if (document.Items is null || document.Items.Count == 0)
            return (null, TransferError("MenuItemTransferItemsEmpty"));

        var normalizedItems = new List<NormalizedTransferItem>(document.Items.Count);
        var keys = new HashSet<string>(StringComparer.Ordinal);
        foreach (var item in document.Items)
        {
            if (item is null)
                return (null, TransferError("MenuItemTransferFileInvalid"));

            var key = item.Key?.Trim() ?? string.Empty;
            var parentKey = string.IsNullOrWhiteSpace(item.ParentKey) ? null : item.ParentKey.Trim();
            var icon = string.IsNullOrWhiteSpace(item.Icon) ? null : item.Icon.Trim();
            var route = string.IsNullOrWhiteSpace(item.Route) ? null : item.Route.Trim();
            if (key.Length == 0 || key.Length > 250)
                return (null, TransferError("MenuItemTransferKeyInvalid", key));
            if (!keys.Add(key))
                return (null, TransferError("MenuItemTransferDuplicateKey", key));
            if (icon?.Length > 100 || route?.Length > 500 || item.SortOrder < 0)
                return (null, TransferError("MenuItemTransferFieldInvalid", key));
            if (item.Translations is null)
                return (null, TransferError("MenuItemTransferTranslationsInvalid", key));

            var normalizedTranslations = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var translation in item.Translations)
            {
                var code = translation.LanguageCode?.Trim().ToLowerInvariant() ?? string.Empty;
                var title = translation.Title?.Trim() ?? string.Empty;
                if (!activeCodes.Contains(code)
                    || title.Length == 0
                    || title.Length > 250
                    || !normalizedTranslations.TryAdd(code, title))
                    return (null, TransferError("MenuItemTransferTranslationsInvalid", key));
            }
            if (normalizedTranslations.Count != activeCodes.Count)
                return (null, TransferError("MenuItemTransferTranslationsInvalid", key));

            normalizedItems.Add(new NormalizedTransferItem(
                key,
                parentKey,
                icon,
                route,
                item.SortOrder,
                item.Disabled,
                normalizedTranslations));
        }

        var itemsByKey = normalizedItems.ToDictionary(item => item.Key, StringComparer.Ordinal);
        foreach (var item in normalizedItems)
        {
            if (item.ParentKey is not null
                && (item.ParentKey == item.Key || !itemsByKey.ContainsKey(item.ParentKey)))
                return (null, TransferError("MenuItemTransferParentInvalid", item.Key));
        }

        var visited = new HashSet<string>(StringComparer.Ordinal);
        var visiting = new HashSet<string>(StringComparer.Ordinal);
        bool HasCycle(NormalizedTransferItem item)
        {
            if (visited.Contains(item.Key)) return false;
            if (!visiting.Add(item.Key)) return true;
            if (item.ParentKey is not null && HasCycle(itemsByKey[item.ParentKey])) return true;
            visiting.Remove(item.Key);
            visited.Add(item.Key);
            return false;
        }

        if (normalizedItems.Any(HasCycle))
            return (null, TransferError("MenuItemTransferHierarchyInvalid"));

        return (normalizedItems, null);
    }

    private static MenuItemTransferDocument ToTransferDocument(IEnumerable<NormalizedTransferItem> items) =>
        new(
            TransferVersion,
            items.Select(item => new MenuItemTransferItem(
                item.Key,
                item.ParentKey,
                item.Icon,
                item.Route,
                item.SortOrder,
                item.Disabled,
                item.Translations
                    .OrderBy(translation => translation.Key)
                    .Select(translation => new MenuItemTranslationDto(translation.Key, translation.Value))
                    .ToList()))
                .ToList());

    private GenericResponse TransferError(string key, params object[] arguments) =>
        new(false, localization.GetLocalizedString(key, arguments));

    private sealed record NormalizedTransferItem(
        string Key,
        string? ParentKey,
        string? Icon,
        string? Route,
        int SortOrder,
        bool Disabled,
        Dictionary<string, string> Translations);

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
