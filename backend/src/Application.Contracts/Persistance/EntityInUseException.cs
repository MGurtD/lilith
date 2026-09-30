namespace Application.Contracts;

/// <summary>
/// Refusal to delete master data that is still in use. It carries resource keys instead
/// of text, because repositories do not localize: the API error middleware turns it into
/// a 409 response whose title explains where the record is in use.
/// </summary>
public class EntityInUseException(string? entityName, IReadOnlyList<string> documentKindKeys)
    : Exception($"'{entityName}' is in use in: {string.Join(", ", documentKindKeys)}")
{
    /// <summary>Name of the record the user tried to delete, when it has one.</summary>
    public string? EntityName { get; } = entityName;

    /// <summary>Resource keys of the kinds of records that refer to it, in display order.</summary>
    public IReadOnlyList<string> DocumentKindKeys { get; } = documentKindKeys;
}
