# Application tests

The directory structure mirrors the application services under test:

```text
Services/       Test classes grouped by business module
TestData/       Builders that create valid domain objects by default
TestSupport/    Small deterministic doubles shared by multiple test classes
Utils/          Tests for application utilities
```

## Test doubles

- Use NSubstitute for application ports, service dependencies, and `IUnitOfWork`.
- Configure only the repository properties and methods exercised by the test.
- Do not implement `IUnitOfWork` in a test. Its broad interface creates unrelated boilerplate and makes tests break when a repository is added.
- Use `InMemoryRepository<TEntity>` only when a test needs realistic query or mutation state. For interaction-only tests, substitute the repository directly.
- Keep specialized stateful behavior in a named context, such as `BrandingTestContext`, rather than in the test class.
- Use `NullLocalizationService` when assertions target resource keys. Use `KeyedLocalizationService` only when translated output is part of the behavior.

## Test data

Builders in `TestData/` must produce valid entities by default. Each test should modify only the fields relevant to its scenario.

## Verification

Run from `backend/`:

```powershell
dotnet test tests/Application.Tests/Application.Tests.csproj
```
