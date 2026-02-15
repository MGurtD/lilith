namespace Domain.Entities.Shared;

public record AvailableStatusTransitionDto(
    Guid StatusToId,
    string StatusToName,
    string StatusToDescription,
    string? StatusToColor,
    string TransitionName
);
