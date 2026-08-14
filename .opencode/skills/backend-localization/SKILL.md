---
name: backend-localization
description: Add or fix Lilith backend localization. Use when changing ILocalizationService messages, ca/es/en resource keys, culture resolution, parameterized responses, or lifecycle/status handling in backend services.
compatibility: OpenCode with .NET 10.
---

# Backend Localization

## Source Of Truth

Inspect the live implementations before editing:

- `ILocalizationService` and `LocalizationService` for lookup behavior.
- `CultureMiddleware` for culture precedence.
- `LocalizationSetup` and active JSON resources for resource loading.
- `StatusConstants.cs` for lifecycle and persisted status identifiers.

Never copy the status catalogue into this skill or another reference file.

## Workflow

1. Locate the response path and determine whether text is user-facing or a stable domain value.
2. Reuse an existing resource key only when its meaning and placeholders match.
3. Add a semantic key to Catalan, Spanish, and English resources in the same logical location.
4. Keep named or positional placeholders identical across locales.
5. Inject and use `ILocalizationService` in the application service. Do not localize in repositories.
6. Pass values as formatting arguments rather than interpolating translated fragments manually.
7. For lifecycle/status lookup, use current constants and preserve the persisted value expected by the database.

Culture resolution is query parameter, authenticated locale claim, `Accept-Language`, then the configured default. Missing-key behavior must be verified from the current `LocalizationService`; do not assume it throws.

## Verify

- Check key presence and placeholder parity in all backend locales.
- Run focused tests when available, then `dotnet build` and `dotnet test` from `backend/`.
- If behavior depends on middleware or authentication, describe any runtime check not performed.
