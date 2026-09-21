using System.Globalization;
using System.Text;
using System.Text.Json;
using Application.Contracts;
using Application.Services.System;
using Application.Tests.TestSupport;
using Domain.Entities;
using Domain.Entities.Auth;
using NSubstitute;
using Xunit;

namespace Application.Tests.Services.System;

/// <summary>
/// Unit tests for <see cref="MenuItemService"/> using in-memory state only where
/// query and mutation behavior is part of the assertion.
/// </summary>
public class MenuItemServiceTests
{
    [Fact]
    public async Task GetAll_uses_the_current_request_culture()
    {
        var item = new MenuItem { Key = "users", SortOrder = 1 };
        var menuItems    = new InMemoryRepository<MenuItem>([item]);
        var translations = new InMemoryRepository<MenuItemTranslation>(
        [
            new() { MenuItemId = item.Id, LanguageCode = "ca", Title = "Usuaris" },
            new() { MenuItemId = item.Id, LanguageCode = "es", Title = "Usuarios" },
            new() { MenuItemId = item.Id, LanguageCode = "en", Title = "Users" }
        ]);
        var (unitOfWork, completeSpy) = CreateUnitOfWork(menuItems, translations);
        var service = new MenuItemService(unitOfWork, NullLocalizationService.Instance, FakeLanguageCatalog.Instance);
        var previousCulture = CultureInfo.CurrentUICulture;

        try
        {
            CultureInfo.CurrentUICulture = new CultureInfo("es");
            var response = await service.GetAll();

            var result = Assert.IsType<List<MenuItemDto>>(response.Content);
            Assert.Equal("Usuarios", Assert.Single(result).Title);
        }
        finally
        {
            CultureInfo.CurrentUICulture = previousCulture;
        }
    }

    [Fact]
    public async Task Create_rejects_a_missing_active_language()
    {
        var (unitOfWork, _) = CreateUnitOfWork(
            new InMemoryRepository<MenuItem>(),
            new InMemoryRepository<MenuItemTranslation>());
        var service = new MenuItemService(unitOfWork, NullLocalizationService.Instance, FakeLanguageCatalog.Instance);
        var request = NewRequest(
        [
            new("ca", "Usuaris"),
            new("es", "Usuarios")
        ]);

        var response = await service.Create(request);

        Assert.False(response.Result);
        Assert.Contains("MenuItemTranslationsInvalid", response.Errors);
    }

    [Fact]
    public async Task Create_persists_menu_and_all_active_translations_atomically()
    {
        var menuItems    = new InMemoryRepository<MenuItem>();
        var translations = new InMemoryRepository<MenuItemTranslation>();
        var (unitOfWork, completeSpy) = CreateUnitOfWork(menuItems, translations);
        var service = new MenuItemService(unitOfWork, NullLocalizationService.Instance, FakeLanguageCatalog.Instance);
        var request = NewRequest(
        [
            new("ca", "Usuaris"),
            new("es", "Usuarios"),
            new("en", "Users")
        ]);

        var response = await service.Create(request);

        Assert.True(response.Result);
        Assert.Single(menuItems.Items);
        Assert.Equal(3, translations.Items.Count);
        Assert.Equal(1, completeSpy.CompleteCount);
    }

    [Fact]
    public async Task GetTranslationMatrix_returns_active_languages_and_empty_missing_cells()
    {
        var parent = new MenuItem { Key = "system", SortOrder = 1 };
        var child  = new MenuItem { Key = "users", ParentId = parent.Id, SortOrder = 1 };
        var menuItems    = new InMemoryRepository<MenuItem>([child, parent]);
        var translations = new InMemoryRepository<MenuItemTranslation>(
        [
            new() { MenuItemId = parent.Id, LanguageCode = "ca", Title = "Sistema" },
            new() { MenuItemId = child.Id,  LanguageCode = "ca", Title = "Usuaris" }
        ]);
        var (unitOfWork, _) = CreateUnitOfWork(menuItems, translations);
        var service = new MenuItemService(unitOfWork, NullLocalizationService.Instance, FakeLanguageCatalog.Instance);

        var response = await service.GetTranslationMatrix();

        var matrix = Assert.IsType<MenuItemTranslationMatrixDto>(response.Content);
        Assert.Equal(["ca", "es", "en"], matrix.Languages.Select(l => l.Code));
        Assert.Equal([parent.Id, child.Id], matrix.Items.Select(i => i.Id));
        Assert.Equal(1, matrix.Items[1].Depth);
        Assert.Equal(string.Empty, matrix.Items[1].Translations.Single(t => t.LanguageCode == "es").Title);
    }

    [Fact]
    public async Task UpdateTranslations_updates_only_requested_cells_and_creates_missing_translations()
    {
        var first  = new MenuItem { Key = "users" };
        var second = new MenuItem { Key = "profiles" };
        var menuItems    = new InMemoryRepository<MenuItem>([first, second]);
        var translations = new InMemoryRepository<MenuItemTranslation>(
        [
            new() { MenuItemId = first.Id,  LanguageCode = "ca", Title = "Usuaris" },
            new() { MenuItemId = first.Id,  LanguageCode = "es", Title = "Usuaris" },
            new() { MenuItemId = second.Id, LanguageCode = "ca", Title = "Perfils" }
        ]);
        var (unitOfWork, completeSpy) = CreateUnitOfWork(menuItems, translations);
        var service = new MenuItemService(unitOfWork, NullLocalizationService.Instance, FakeLanguageCatalog.Instance);
        var request = new UpdateMenuItemTranslationsRequest(
        [
            new(first.Id,  [new("es", "Usuarios")]),
            new(second.Id, [new("en", "Profiles")])
        ]);

        var response = await service.UpdateTranslations(request);

        Assert.True(response.Result);
        Assert.Equal("Usuaris",  translations.Items.Single(t => t.MenuItemId == first.Id  && t.LanguageCode == "ca").Title);
        Assert.Equal("Usuarios", translations.Items.Single(t => t.MenuItemId == first.Id  && t.LanguageCode == "es").Title);
        Assert.Equal("Profiles", translations.Items.Single(t => t.MenuItemId == second.Id && t.LanguageCode == "en").Title);
        var result = Assert.IsType<UpdateMenuItemTranslationsResult>(response.Content);
        Assert.Equal(2, result.UpdatedMenuItems);
        Assert.Equal(2, result.UpdatedTranslations);
        Assert.Equal(1, completeSpy.CompleteCount);
    }

    [Fact]
    public async Task UpdateTranslations_rejects_the_entire_batch_before_mutating_when_a_cell_is_invalid()
    {
        var item = new MenuItem { Key = "users" };
        var menuItems    = new InMemoryRepository<MenuItem>([item]);
        var translation  = new MenuItemTranslation { MenuItemId = item.Id, LanguageCode = "es", Title = "Usuaris" };
        var translations = new InMemoryRepository<MenuItemTranslation>([translation]);
        var (unitOfWork, completeSpy) = CreateUnitOfWork(menuItems, translations);
        var service = new MenuItemService(unitOfWork, NullLocalizationService.Instance, FakeLanguageCatalog.Instance);
        var request = new UpdateMenuItemTranslationsRequest(
        [
            new(item.Id, [new("es", "Usuarios"), new("fr", "Utilisateurs")])
        ]);

        var response = await service.UpdateTranslations(request);

        Assert.False(response.Result);
        Assert.Equal("Usuaris", translation.Title);
        Assert.Equal(0, completeSpy.CompleteCount);
    }

    [Fact]
    public async Task UpdateTranslations_rejects_duplicate_cells()
    {
        var item = new MenuItem { Key = "users" };
        var (unitOfWork, completeSpy) = CreateUnitOfWork(
            new InMemoryRepository<MenuItem>([item]),
            new InMemoryRepository<MenuItemTranslation>());
        var service = new MenuItemService(unitOfWork, NullLocalizationService.Instance, FakeLanguageCatalog.Instance);
        var request = new UpdateMenuItemTranslationsRequest(
        [
            new(item.Id, [new("es", "Usuarios"), new("ES", "Usuarios")])
        ]);

        var response = await service.UpdateTranslations(request);

        Assert.False(response.Result);
        Assert.Contains("MenuItemTranslationBatchDuplicateCell", response.Errors);
        Assert.Equal(0, completeSpy.CompleteCount);
    }

    [Fact]
    public async Task Export_uses_keys_for_hierarchy_and_includes_all_transferable_fields()
    {
        var parent = new MenuItem { Key = "system", Icon = "pi pi-cog", SortOrder = 1, Disabled = true };
        var child  = new MenuItem { Key = "users",  ParentId = parent.Id, Route = "/users", SortOrder = 2 };
        var menuItems    = new InMemoryRepository<MenuItem>([child, parent]);
        var translations = new InMemoryRepository<MenuItemTranslation>(
            TranslationsFor(parent, "Sistema", "Sistema", "System")
                .Concat(TranslationsFor(child, "Usuaris", "Usuarios", "Users")));
        var (unitOfWork, _) = CreateUnitOfWork(menuItems, translations);
        var service = new MenuItemService(unitOfWork, NullLocalizationService.Instance, FakeLanguageCatalog.Instance);

        var response = await service.Export();

        Assert.True(response.Result);
        var document = Assert.IsType<MenuItemTransferDocument>(response.Content);
        Assert.Equal(1, document.Version);
        var exportedParent = Assert.Single(document.Items!, item => item.Key == "system");
        var exportedChild  = Assert.Single(document.Items!, item => item.Key == "users");
        Assert.Null(exportedParent.ParentKey);
        Assert.True(exportedParent.Disabled);
        Assert.Equal("system", exportedChild.ParentKey);
        Assert.Equal("/users", exportedChild.Route);
        Assert.Equal(["ca", "en", "es"], exportedChild.Translations!.Select(t => t.LanguageCode));
    }

    [Fact]
    public async Task Import_creates_and_updates_by_key_preserving_absent_items_and_existing_ids()
    {
        var existingId = Guid.NewGuid();
        var existing   = new MenuItem { Id = existingId, Key = "system", Icon = "old", SortOrder = 9 };
        var absentFromImport = new MenuItem { Key = "local_only", Route = "/local" };
        var menuItems    = new InMemoryRepository<MenuItem>([existing, absentFromImport]);
        var translations = new InMemoryRepository<MenuItemTranslation>(
            TranslationsFor(existing, "Sistema vell", "Sistema viejo", "Old system"));
        var (unitOfWork, completeSpy) = CreateUnitOfWork(menuItems, translations);
        var service  = new MenuItemService(unitOfWork, NullLocalizationService.Instance, FakeLanguageCatalog.Instance);
        var document = new MenuItemTransferDocument(1,
        [
            TransferItem("system", null, "Sistema",  "Sistema",  "System",  icon: "pi pi-cog", disabled: true),
            TransferItem("users",  "system", "Usuaris", "Usuarios", "Users", route: "/users", sortOrder: 2)
        ]);

        var response = await service.Import(JsonStream(document));

        Assert.True(response.Result);
        Assert.Equal(1, completeSpy.CompleteCount);
        Assert.Equal(3, menuItems.Items.Count);
        Assert.Equal(existingId, menuItems.Items.Single(i => i.Key == "system").Id);
        Assert.Equal("pi pi-cog", existing.Icon);
        Assert.True(existing.Disabled);
        Assert.Contains(absentFromImport, menuItems.Items);
        var users = menuItems.Items.Single(i => i.Key == "users");
        Assert.Equal(existingId, users.ParentId);
        Assert.Equal("/users", users.Route);
        Assert.Equal("System", translations.Items.Single(t => t.MenuItemId == existingId && t.LanguageCode == "en").Title);
        Assert.Equal(3, translations.Items.Count(t => t.MenuItemId == users.Id));
        var result = Assert.IsType<MenuItemImportResult>(response.Content);
        Assert.Equal(1, result.CreatedItems);
        Assert.Equal(1, result.UpdatedItems);
        Assert.Equal(6, result.UpdatedTranslations);
    }

    [Fact]
    public async Task Import_rejects_invalid_json_without_mutating_data()
    {
        var existing = new MenuItem { Key = "system", Icon = "original" };
        var menuItems = new InMemoryRepository<MenuItem>([existing]);
        var (unitOfWork, completeSpy) = CreateUnitOfWork(menuItems, new InMemoryRepository<MenuItemTranslation>());
        var service = new MenuItemService(unitOfWork, NullLocalizationService.Instance, FakeLanguageCatalog.Instance);

        var response = await service.Import(new MemoryStream(Encoding.UTF8.GetBytes("{ invalid")));

        Assert.False(response.Result);
        Assert.Contains("MenuItemTransferFileInvalid", response.Errors);
        Assert.Equal("original", existing.Icon);
        Assert.Equal(0, completeSpy.CompleteCount);
    }

    [Fact]
    public async Task Import_rejects_language_mismatch_before_staging_changes()
    {
        var existing  = new MenuItem { Key = "system", Icon = "original" };
        var menuItems = new InMemoryRepository<MenuItem>([existing]);
        var (unitOfWork, completeSpy) = CreateUnitOfWork(menuItems, new InMemoryRepository<MenuItemTranslation>());
        var service  = new MenuItemService(unitOfWork, NullLocalizationService.Instance, FakeLanguageCatalog.Instance);
        var document = new MenuItemTransferDocument(1,
        [
            new MenuItemTransferItem(
                "system", null, "changed", null, 0, false,
                [new("ca", "Sistema"), new("es", "Sistema")])
        ]);

        var response = await service.Import(JsonStream(document));

        Assert.False(response.Result);
        Assert.Contains("MenuItemTransferTranslationsInvalid", response.Errors);
        Assert.Equal("original", existing.Icon);
        Assert.Equal(0, completeSpy.CompleteCount);
    }

    [Fact]
    public async Task Import_rejects_cyclic_hierarchy_before_staging_changes()
    {
        var menuItems = new InMemoryRepository<MenuItem>();
        var (unitOfWork, completeSpy) = CreateUnitOfWork(menuItems, new InMemoryRepository<MenuItemTranslation>());
        var service  = new MenuItemService(unitOfWork, NullLocalizationService.Instance, FakeLanguageCatalog.Instance);
        var document = new MenuItemTransferDocument(1,
        [
            TransferItem("first",  "second", "Primer", "Primero", "First"),
            TransferItem("second", "first",  "Segon",  "Segundo", "Second")
        ]);

        var response = await service.Import(JsonStream(document));

        Assert.False(response.Result);
        Assert.Contains("MenuItemTransferHierarchyInvalid", response.Errors);
        Assert.Empty(menuItems.Items);
        Assert.Equal(0, completeSpy.CompleteCount);
    }

    [Fact]
    public async Task Export_rejects_menu_items_without_every_active_translation()
    {
        var item      = new MenuItem { Key = "system" };
        var menuItems = new InMemoryRepository<MenuItem>([item]);
        var translations = new InMemoryRepository<MenuItemTranslation>(
        [
            new() { MenuItemId = item.Id, LanguageCode = "ca", Title = "Sistema" }
        ]);
        var (unitOfWork, completeSpy) = CreateUnitOfWork(menuItems, translations);
        var service = new MenuItemService(unitOfWork, NullLocalizationService.Instance, FakeLanguageCatalog.Instance);

        var response = await service.Export();

        Assert.False(response.Result);
        Assert.Contains("MenuItemTransferTranslationsInvalid", response.Errors);
        Assert.Equal(0, completeSpy.CompleteCount);
    }

    // -------- helpers --------

    private static CreateMenuItemRequest NewRequest(IReadOnlyList<MenuItemTranslationDto> translations) =>
        new(Guid.NewGuid(), "users", "pi pi-users", "/users", 1, null, translations);

    private static MenuItemTransferItem TransferItem(
        string key,
        string? parentKey,
        string catalan,
        string spanish,
        string english,
        string? icon = null,
        string? route = null,
        int sortOrder = 0,
        bool disabled = false) =>
        new(key, parentKey, icon, route, sortOrder, disabled,
            [new("ca", catalan), new("es", spanish), new("en", english)]);

    private static IEnumerable<MenuItemTranslation> TranslationsFor(
        MenuItem item, string catalan, string spanish, string english) =>
    [
        new() { MenuItemId = item.Id, LanguageCode = "ca", Title = catalan },
        new() { MenuItemId = item.Id, LanguageCode = "es", Title = spanish },
        new() { MenuItemId = item.Id, LanguageCode = "en", Title = english },
    ];

    private static MemoryStream JsonStream(MenuItemTransferDocument document) =>
        new(JsonSerializer.SerializeToUtf8Bytes(document, new JsonSerializerOptions(JsonSerializerDefaults.Web)));

    /// <summary>
    /// Builds an NSubstitute IUnitOfWork with only MenuItems and MenuItemTranslations
    /// wired to real in-memory repositories. Returns a spy to track CompleteAsync calls.
    /// </summary>
    private static (IUnitOfWork UnitOfWork, CompleteAsyncSpy Spy) CreateUnitOfWork(
        InMemoryRepository<MenuItem> menuItems,
        InMemoryRepository<MenuItemTranslation> translations)
    {
        var spy = new CompleteAsyncSpy();
        var uow = Substitute.For<IUnitOfWork>();

        uow.MenuItems.Returns(menuItems);
        uow.MenuItemTranslations.Returns(translations);
        uow.CompleteAsync().Returns(_ => { spy.Record(); return Task.FromResult(1); });

        return (uow, spy);
    }

    /// <summary>Lightweight counter for CompleteAsync calls (replaces DispatchProxy boilerplate).</summary>
    private sealed class CompleteAsyncSpy
    {
        public int CompleteCount { get; private set; }
        public void Record() => CompleteCount++;
    }
}
