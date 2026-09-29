namespace Application.Contracts;

public sealed record BrandingUpdateRequest(
    string? BrandName,
    string? PrimaryColor);

public sealed record BrandingWatermarkRequest(bool Enabled);
