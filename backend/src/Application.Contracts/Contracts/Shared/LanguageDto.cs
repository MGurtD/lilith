namespace Application.Contracts;

public record LanguageDto(
    Guid Id,
    string Code,
    string Name,
    string Icon,
    bool IsDefault,
    int SortOrder
);
