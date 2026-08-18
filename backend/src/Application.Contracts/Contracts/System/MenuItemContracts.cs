namespace Application.Contracts;

public record MenuItemTranslationDto(string LanguageCode, string Title);

public record MenuItemDto(
    Guid Id,
    string Key,
    string Title,
    string? Icon,
    string? Route,
    int SortOrder,
    Guid? ParentId,
    bool Disabled,
    IReadOnlyList<MenuItemTranslationDto> Translations);

public record MenuItemNodeDto(
    Guid Id,
    string Key,
    string Title,
    string? Icon,
    string? Route,
    int SortOrder,
    Guid? ParentId,
    bool Disabled,
    IReadOnlyList<MenuItemTranslationDto> Translations,
    List<MenuItemNodeDto> Children);

public record CreateMenuItemRequest(
    Guid Id,
    string Key,
    string? Icon,
    string? Route,
    int SortOrder,
    Guid? ParentId,
    IReadOnlyList<MenuItemTranslationDto> Translations);

public record UpdateMenuItemRequest(
    Guid Id,
    string Key,
    string? Icon,
    string? Route,
    int SortOrder,
    Guid? ParentId,
    IReadOnlyList<MenuItemTranslationDto> Translations);

public record MenuItemTranslationMatrixDto(
    IReadOnlyList<LanguageDto> Languages,
    IReadOnlyList<MenuItemTranslationMatrixRowDto> Items);

public record MenuItemTranslationMatrixRowDto(
    Guid Id,
    string Key,
    string? Route,
    Guid? ParentId,
    int SortOrder,
    bool Disabled,
    int Depth,
    IReadOnlyList<MenuItemTranslationDto> Translations);

public record UpdateMenuItemTranslationsRequest(
    IReadOnlyList<UpdateMenuItemTranslationRowRequest> Items);

public record UpdateMenuItemTranslationRowRequest(
    Guid MenuItemId,
    IReadOnlyList<MenuItemTranslationDto> Translations);

public record UpdateMenuItemTranslationsResult(int UpdatedMenuItems, int UpdatedTranslations);

public record MenuItemTransferDocument(
    int Version,
    IReadOnlyList<MenuItemTransferItem>? Items);

public record MenuItemTransferItem(
    string? Key,
    string? ParentKey,
    string? Icon,
    string? Route,
    int SortOrder,
    bool Disabled,
    IReadOnlyList<MenuItemTranslationDto>? Translations);

public record MenuItemImportResult(int CreatedItems, int UpdatedItems, int UpdatedTranslations);
