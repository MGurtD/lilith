using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using Application.Contracts;
using Application.Services.System;
using Domain.Entities;
using Domain.Entities.Auth;
using Xunit;

namespace Application.Tests.MenuItems;

public class MenuItemServiceTests
{
    [Fact]
    public async Task GetAll_uses_the_current_request_culture()
    {
        var item = new MenuItem { Key = "users", SortOrder = 1 };
        var menuItems = new InMemoryRepository<MenuItem>([item]);
        var translations = new InMemoryRepository<MenuItemTranslation>(
        [
            new() { MenuItemId = item.Id, LanguageCode = "ca", Title = "Usuaris" },
            new() { MenuItemId = item.Id, LanguageCode = "es", Title = "Usuarios" },
            new() { MenuItemId = item.Id, LanguageCode = "en", Title = "Users" }
        ]);
        var (unitOfWork, _) = CreateUnitOfWork(menuItems, translations);
        var service = new MenuItemService(unitOfWork, new FakeLocalizationService(), new FakeLanguageCatalog());
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
        var service = new MenuItemService(unitOfWork, new FakeLocalizationService(), new FakeLanguageCatalog());
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
        var menuItems = new InMemoryRepository<MenuItem>();
        var translations = new InMemoryRepository<MenuItemTranslation>();
        var (unitOfWork, proxy) = CreateUnitOfWork(menuItems, translations);
        var service = new MenuItemService(unitOfWork, new FakeLocalizationService(), new FakeLanguageCatalog());
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
        Assert.Equal(1, proxy.CompleteCalls);
    }

    [Fact]
    public async Task GetTranslationMatrix_returns_active_languages_and_empty_missing_cells()
    {
        var parent = new MenuItem { Key = "system", SortOrder = 1 };
        var child = new MenuItem { Key = "users", ParentId = parent.Id, SortOrder = 1 };
        var menuItems = new InMemoryRepository<MenuItem>([child, parent]);
        var translations = new InMemoryRepository<MenuItemTranslation>(
        [
            new() { MenuItemId = parent.Id, LanguageCode = "ca", Title = "Sistema" },
            new() { MenuItemId = child.Id, LanguageCode = "ca", Title = "Usuaris" }
        ]);
        var (unitOfWork, _) = CreateUnitOfWork(menuItems, translations);
        var service = new MenuItemService(unitOfWork, new FakeLocalizationService(), new FakeLanguageCatalog());

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
        var first = new MenuItem { Key = "users" };
        var second = new MenuItem { Key = "profiles" };
        var menuItems = new InMemoryRepository<MenuItem>([first, second]);
        var translations = new InMemoryRepository<MenuItemTranslation>(
        [
            new() { MenuItemId = first.Id, LanguageCode = "ca", Title = "Usuaris" },
            new() { MenuItemId = first.Id, LanguageCode = "es", Title = "Usuaris" },
            new() { MenuItemId = second.Id, LanguageCode = "ca", Title = "Perfils" }
        ]);
        var (unitOfWork, proxy) = CreateUnitOfWork(menuItems, translations);
        var service = new MenuItemService(unitOfWork, new FakeLocalizationService(), new FakeLanguageCatalog());
        var request = new UpdateMenuItemTranslationsRequest(
        [
            new(first.Id, [new("es", "Usuarios")]),
            new(second.Id, [new("en", "Profiles")])
        ]);

        var response = await service.UpdateTranslations(request);

        Assert.True(response.Result);
        Assert.Equal("Usuaris", translations.Items.Single(t => t.MenuItemId == first.Id && t.LanguageCode == "ca").Title);
        Assert.Equal("Usuarios", translations.Items.Single(t => t.MenuItemId == first.Id && t.LanguageCode == "es").Title);
        Assert.Equal("Profiles", translations.Items.Single(t => t.MenuItemId == second.Id && t.LanguageCode == "en").Title);
        var result = Assert.IsType<UpdateMenuItemTranslationsResult>(response.Content);
        Assert.Equal(2, result.UpdatedMenuItems);
        Assert.Equal(2, result.UpdatedTranslations);
        Assert.Equal(1, proxy.CompleteCalls);
    }

    [Fact]
    public async Task UpdateTranslations_rejects_the_entire_batch_before_mutating_when_a_cell_is_invalid()
    {
        var item = new MenuItem { Key = "users" };
        var menuItems = new InMemoryRepository<MenuItem>([item]);
        var translation = new MenuItemTranslation { MenuItemId = item.Id, LanguageCode = "es", Title = "Usuaris" };
        var translations = new InMemoryRepository<MenuItemTranslation>([translation]);
        var (unitOfWork, proxy) = CreateUnitOfWork(menuItems, translations);
        var service = new MenuItemService(unitOfWork, new FakeLocalizationService(), new FakeLanguageCatalog());
        var request = new UpdateMenuItemTranslationsRequest(
        [
            new(item.Id, [new("es", "Usuarios"), new("fr", "Utilisateurs")])
        ]);

        var response = await service.UpdateTranslations(request);

        Assert.False(response.Result);
        Assert.Equal("Usuaris", translation.Title);
        Assert.Equal(0, proxy.CompleteCalls);
    }

    [Fact]
    public async Task UpdateTranslations_rejects_duplicate_cells()
    {
        var item = new MenuItem { Key = "users" };
        var (unitOfWork, proxy) = CreateUnitOfWork(
            new InMemoryRepository<MenuItem>([item]),
            new InMemoryRepository<MenuItemTranslation>());
        var service = new MenuItemService(unitOfWork, new FakeLocalizationService(), new FakeLanguageCatalog());
        var request = new UpdateMenuItemTranslationsRequest(
        [
            new(item.Id, [new("es", "Usuarios"), new("ES", "Usuarios")])
        ]);

        var response = await service.UpdateTranslations(request);

        Assert.False(response.Result);
        Assert.Contains("MenuItemTranslationBatchDuplicateCell", response.Errors);
        Assert.Equal(0, proxy.CompleteCalls);
    }

    private static CreateMenuItemRequest NewRequest(IReadOnlyList<MenuItemTranslationDto> translations) =>
        new(Guid.NewGuid(), "users", "pi pi-users", "/users", 1, null, translations);

    private static (IUnitOfWork UnitOfWork, UnitOfWorkProxy Proxy) CreateUnitOfWork(
        IRepository<MenuItem, Guid> menuItems,
        IRepository<MenuItemTranslation, Guid> translations)
    {
        var unitOfWork = DispatchProxy.Create<IUnitOfWork, UnitOfWorkProxy>();
        var proxy = (UnitOfWorkProxy)(object)unitOfWork;
        proxy.Repositories[nameof(IUnitOfWork.MenuItems)] = menuItems;
        proxy.Repositories[nameof(IUnitOfWork.MenuItemTranslations)] = translations;
        return (unitOfWork, proxy);
    }

    public class UnitOfWorkProxy : DispatchProxy
    {
        public Dictionary<string, object> Repositories { get; } = new();
        public int CompleteCalls { get; private set; }

        protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
        {
            if (targetMethod?.Name == nameof(IUnitOfWork.CompleteAsync))
            {
                CompleteCalls++;
                return Task.FromResult(1);
            }
            if (targetMethod?.Name == nameof(IDisposable.Dispose))
                return null;
            if (targetMethod?.Name.StartsWith("get_", StringComparison.Ordinal) == true
                && Repositories.TryGetValue(targetMethod.Name[4..], out var repository))
                return repository;

            throw new NotImplementedException(targetMethod?.Name);
        }
    }

    private sealed class InMemoryRepository<TEntity>(IEnumerable<TEntity>? seed = null) : IRepository<TEntity, Guid>
        where TEntity : Entity
    {
        public List<TEntity> Items { get; } = seed?.ToList() ?? [];

        public Task<TEntity?> Get(Guid id) => Task.FromResult(Items.FirstOrDefault(e => e.Id == id));
        public Task<IEnumerable<TEntity>> GetAll() => Task.FromResult<IEnumerable<TEntity>>(Items.ToList());
        public Task<List<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate) =>
            Task.FromResult(Items.AsQueryable().Where(predicate).ToList());
        public Task<List<TEntity>> FindAsyncWithQueryParams(Expression<Func<TEntity, bool>> predicate, Func<IQueryable<TEntity>, IQueryable<TEntity>>? includeFunc)
        {
            var query = Items.AsQueryable().Where(predicate);
            return Task.FromResult((includeFunc?.Invoke(query) ?? query).ToList());
        }
        public IEnumerable<TEntity> Find(Expression<Func<TEntity, bool>> predicate) => Items.AsQueryable().Where(predicate);
        public Task<bool> Exists(Guid id) => Task.FromResult(Items.Any(e => e.Id == id));
        public Task Add(TEntity entity) { Items.Add(entity); return Task.CompletedTask; }
        public Task AddWithoutSave(TEntity entity) { Items.Add(entity); return Task.CompletedTask; }
        public Task AddRange(IEnumerable<TEntity> entities) { Items.AddRange(entities); return Task.CompletedTask; }
        public Task AddRangeWithoutSave(IEnumerable<TEntity> entities) { Items.AddRange(entities); return Task.CompletedTask; }
        public Task Update(TEntity entity) { Replace(entity); return Task.CompletedTask; }
        public bool UpdateWithoutSave(TEntity entity) { Replace(entity); return true; }
        public Task Remove(TEntity entity) { Items.Remove(entity); return Task.CompletedTask; }
        public Task RemoveRange(IEnumerable<TEntity> entities) { Items.RemoveAll(entities.Contains); return Task.CompletedTask; }
        public Task SaveChanges() => Task.CompletedTask;

        private void Replace(TEntity entity)
        {
            var index = Items.FindIndex(e => e.Id == entity.Id);
            if (index >= 0) Items[index] = entity;
        }
    }

    private sealed class FakeLanguageCatalog : ILanguageCatalog
    {
        private static readonly LanguageDto[] Languages =
        [
            new(Guid.NewGuid(), "ca", "Català", "", true, 1),
            new(Guid.NewGuid(), "es", "Español", "", false, 2),
            new(Guid.NewGuid(), "en", "English", "", false, 3)
        ];

        public Task<IEnumerable<LanguageDto>> GetAllAsync() => Task.FromResult<IEnumerable<LanguageDto>>(Languages);
        public Task<LanguageDto?> GetByCodeAsync(string code) =>
            Task.FromResult(Languages.FirstOrDefault(l => l.Code.Equals(code, StringComparison.OrdinalIgnoreCase)));
        public Task<LanguageDto?> GetDefaultAsync() => Task.FromResult<LanguageDto?>(Languages[0]);
    }

    private sealed class FakeLocalizationService : ILocalizationService
    {
        public string GetLocalizedString(string key, params object[] arguments) => key;
        public string GetLocalizedStringForCulture(string key, string culture, params object[] arguments) => key;
        public Dictionary<string, string> GetAllTranslations() => [];
        public Dictionary<string, string> GetAllTranslationsForCulture(string culture) => [];
        public string[] GetSupportedCultures() => ["ca", "es", "en"];
    }
}
