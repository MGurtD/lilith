using Application.Contracts;
using Application.Contracts.Persistance.Repositories.Purchase;
using Application.Services.Production;
using Application.Services.System;
using Domain.Entities;
using Domain.Entities.Auth;
using Domain.Entities.Production;
using Domain.Entities.Purchase;
using Domain.Entities.Sales;
using Domain.Entities.Shared;
using Domain.Entities.Transport;
using Domain.Entities.Warehouse;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using System.Linq.Expressions;
using Xunit;

namespace Application.Tests.Services.Production;

public class BrandingServiceTests
{
    private static readonly byte[] PngHeader =
    [137, 80, 78, 71, 13, 10, 26, 10];

    [Fact]
    public async Task GetCurrent_returns_branding_from_the_single_enabled_enterprise()
    {
        var root = CreateTempDirectory();
        try
        {
            var enterprise = NewEnterprise();
            enterprise.BrandName = "  Acme  ";
            enterprise.PrimaryColor = "  InDiGo  ";
            var sut = BuildSut(new FakeUnitOfWork(enterprise), root);

            var response = await sut.GetCurrent();

            Assert.Equal("Acme", response.BrandName);
            Assert.Equal(BrandingPalette.Indigo, response.PrimaryColor);
            Assert.Null(response.MainLogoVersion);
            Assert.Null(response.SidebarLogoVersion);
        }
        finally
        {
            DeleteDirectory(root);
        }
    }

    [Fact]
    public async Task UpdateCurrent_normalizes_allowed_palette_keys_and_validates_brand_name()
    {
        var root = CreateTempDirectory();
        try
        {
            var enterprise = NewEnterprise();
            var sut = BuildSut(new FakeUnitOfWork(enterprise), root);

            var invalid = await sut.UpdateCurrent(new BrandingUpdateRequest("A".PadRight(61, 'x'), "blue"));
            var valid = await sut.UpdateCurrent(new BrandingUpdateRequest("  Acme  ", "  TeAL  "));

            Assert.False(invalid.Result);
            Assert.True(valid.Result);
            Assert.Equal("Acme", enterprise.BrandName);
            Assert.Equal(BrandingPalette.Teal, enterprise.PrimaryColor);
        }
        finally
        {
            DeleteDirectory(root);
        }
    }

    [Theory]
    [InlineData("BLACK", BrandingPalette.Black)]
    [InlineData("BLUE", BrandingPalette.Blue)]
    [InlineData("indigo", BrandingPalette.Indigo)]
    [InlineData("EMERALD", BrandingPalette.Emerald)]
    [InlineData("teal", BrandingPalette.Teal)]
    [InlineData("VIOLET", BrandingPalette.Violet)]
    [InlineData("orange", BrandingPalette.Orange)]
    [InlineData("ROSE", BrandingPalette.Rose)]
    public async Task UpdateCurrent_accepts_and_normalizes_each_allowed_palette_key(
        string primaryColor,
        string expectedPalette)
    {
        var root = CreateTempDirectory();
        try
        {
            var enterprise = NewEnterprise();
            var sut = BuildSut(new FakeUnitOfWork(enterprise), root);

            var response = await sut.UpdateCurrent(new BrandingUpdateRequest("Acme", primaryColor));

            Assert.True(response.Result);
            Assert.Equal(expectedPalette, enterprise.PrimaryColor);
        }
        finally
        {
            DeleteDirectory(root);
        }
    }

    [Theory]
    [InlineData("#12ABEF")]
    [InlineData("magenta")]
    public async Task UpdateCurrent_rejects_non_palette_values(string primaryColor)
    {
        var root = CreateTempDirectory();
        try
        {
            var enterprise = NewEnterprise();
            var sut = BuildSut(new FakeUnitOfWork(enterprise), root);

            var response = await sut.UpdateCurrent(new BrandingUpdateRequest("Acme", primaryColor));

            Assert.False(response.Result);
            Assert.Contains("BrandingPaletteInvalid", response.Errors);
            Assert.Null(enterprise.PrimaryColor);
        }
        finally
        {
            DeleteDirectory(root);
        }
    }

    [Theory]
    [InlineData(null)]
    [InlineData("#12ABEF")]
    [InlineData("unknown")]
    public async Task GetCurrent_falls_back_to_default_for_legacy_or_unknown_palette_values(string? primaryColor)
    {
        var root = CreateTempDirectory();
        try
        {
            var enterprise = NewEnterprise();
            enterprise.PrimaryColor = primaryColor;
            var sut = BuildSut(new FakeUnitOfWork(enterprise), root);

            var response = await sut.GetCurrent();

            Assert.Equal(BrandingPalette.Default, response.PrimaryColor);
        }
        finally
        {
            DeleteDirectory(root);
        }
    }

    [Fact]
    public async Task Current_logo_operations_resolve_the_enabled_enterprise()
    {
        var root = CreateTempDirectory();
        try
        {
            var enterprise = NewEnterprise();
            var uow = new FakeUnitOfWork(enterprise);
            var sut = BuildSut(uow, root);
            using var stream = new MemoryStream(PngHeader);
            var file = NewFormFile(stream, "logo.png", "image/png");

            var upload = await sut.UploadCurrentLogo(BrandingLogoSlot.Main, file);
            var remove = await sut.RemoveCurrentLogo(BrandingLogoSlot.Main);

            Assert.True(upload.Result);
            Assert.True(remove.Result);
            Assert.Null(enterprise.LogoMainFileId);
            Assert.Empty(uow.FilesStore.Store);
        }
        finally
        {
            DeleteDirectory(root);
        }
    }

    [Fact]
    public async Task UploadLogo_uses_distinct_entities_for_both_slots_and_replacements()
    {
        var root = CreateTempDirectory();
        try
        {
            var enterprise = NewEnterprise();
            var uow = new FakeUnitOfWork(enterprise);
            var sut = BuildSut(uow, root);

            await sut.UploadLogo(enterprise.Id, BrandingLogoSlot.Main, NewFormFile(new MemoryStream(PngHeader), "main.png", "image/png"));
            await sut.UploadLogo(enterprise.Id, BrandingLogoSlot.Sidebar, NewFormFile(new MemoryStream(PngHeader), "sidebar.png", "image/png"));
            await sut.UploadLogo(enterprise.Id, BrandingLogoSlot.Main, NewFormFile(new MemoryStream(PngHeader), "main-replacement.png", "image/png"));
            await sut.UploadLogo(enterprise.Id, BrandingLogoSlot.Sidebar, NewFormFile(new MemoryStream(PngHeader), "sidebar-replacement.png", "image/png"));

            Assert.Equal(2, uow.FilesStore.Store.Count);
            Assert.Contains(uow.FilesStore.Store, file => file.Entity == "EnterpriseBranding:main");
            Assert.Contains(uow.FilesStore.Store, file => file.Entity == "EnterpriseBranding:sidebar");
            Assert.Equal(enterprise.LogoMainFileId, uow.FilesStore.Store.Single(file => file.Entity == "EnterpriseBranding:main").Id);
            Assert.Equal(enterprise.LogoSidebarFileId, uow.FilesStore.Store.Single(file => file.Entity == "EnterpriseBranding:sidebar").Id);

            var response = await sut.GetCurrent();
            Assert.Equal(enterprise.LogoMainFileId?.ToString("N"), response.MainLogoVersion);
            Assert.Equal(enterprise.LogoSidebarFileId?.ToString("N"), response.SidebarLogoVersion);
            Assert.NotEqual(response.MainLogoVersion, response.SidebarLogoVersion);
        }
        finally
        {
            DeleteDirectory(root);
        }
    }

    [Fact]
    public async Task Legacy_branding_file_is_valid_only_for_the_main_slot()
    {
        var root = CreateTempDirectory();
        try
        {
            var enterprise = NewEnterprise();
            var path = Path.Combine(root, "EnterpriseBranding", "legacy.png");
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            await System.IO.File.WriteAllBytesAsync(path, PngHeader);
            var legacyFile = NewBrandingFile(enterprise.Id, path, "EnterpriseBranding");
            enterprise.LogoMainFileId = legacyFile.Id;
            enterprise.LogoSidebarFileId = legacyFile.Id;
            var uow = new FakeUnitOfWork(enterprise);
            uow.FilesStore.Store.Add(legacyFile);

            var response = await BuildSut(uow, root).GetCurrent();

            Assert.True(response.HasMainLogo);
            Assert.False(response.HasSidebarLogo);
            Assert.Equal(legacyFile.Id.ToString("N"), response.MainLogoVersion);
            Assert.Null(response.SidebarLogoVersion);
        }
        finally
        {
            DeleteDirectory(root);
        }
    }

    [Fact]
    public async Task GetCurrent_logo_tokens_change_when_a_slot_file_is_replaced()
    {
        var root = CreateTempDirectory();
        try
        {
            var enterprise = NewEnterprise();
            var uow = new FakeUnitOfWork(enterprise);
            var sut = BuildSut(uow, root);

            await sut.UploadLogo(enterprise.Id, BrandingLogoSlot.Main,
                NewFormFile(new MemoryStream(PngHeader), "main.png", "image/png"));
            var first = await sut.GetCurrent();

            await sut.UploadLogo(enterprise.Id, BrandingLogoSlot.Main,
                NewFormFile(new MemoryStream(PngHeader), "main-replacement.png", "image/png"));
            var second = await sut.GetCurrent();

            Assert.NotNull(first.MainLogoVersion);
            Assert.NotNull(second.MainLogoVersion);
            Assert.NotEqual(first.MainLogoVersion, second.MainLogoVersion);
            Assert.Null(first.SidebarLogoVersion);
            Assert.Null(second.SidebarLogoVersion);
        }
        finally
        {
            DeleteDirectory(root);
        }
    }

    [Fact]
    public async Task GetCurrent_returns_no_logo_token_for_invalid_slot_files()
    {
        var root = CreateTempDirectory();
        try
        {
            var enterprise = NewEnterprise();
            var outsidePath = Path.Combine(Path.GetTempPath(), $"lilith-invalid-branding-{Guid.NewGuid():N}.png");
            await System.IO.File.WriteAllBytesAsync(outsidePath, PngHeader);
            try
            {
                var mainFile = NewBrandingFile(enterprise.Id, outsidePath, "EnterpriseBranding:main");
                var sidebarFile = NewBrandingFile(enterprise.Id, Path.Combine(root, "missing.png"), "EnterpriseBranding:sidebar");
                enterprise.LogoMainFileId = mainFile.Id;
                enterprise.LogoSidebarFileId = sidebarFile.Id;
                var uow = new FakeUnitOfWork(enterprise);
                uow.FilesStore.Store.AddRange([mainFile, sidebarFile]);

                var response = await BuildSut(uow, root).GetCurrent();

                Assert.False(response.HasMainLogo);
                Assert.False(response.HasSidebarLogo);
                Assert.Null(response.MainLogoVersion);
                Assert.Null(response.SidebarLogoVersion);
            }
            finally
            {
                if (System.IO.File.Exists(outsidePath))
                    System.IO.File.Delete(outsidePath);
            }
        }
        finally
        {
            DeleteDirectory(root);
        }
    }

    [Fact]
    public async Task Enterprise_update_preserves_persisted_branding_fields()
    {
        var enterprise = NewEnterprise();
        enterprise.BrandName = "Current brand";
        enterprise.PrimaryColor = "#123456";
        enterprise.LogoMainFileId = Guid.NewGuid();
        enterprise.LogoSidebarFileId = Guid.NewGuid();
        var request = NewEnterprise();
        request.Id = enterprise.Id;
        request.Name = "Updated enterprise";
        request.BrandName = "Stale brand";
        request.PrimaryColor = "#FFFFFF";
        request.LogoMainFileId = Guid.NewGuid();
        request.LogoSidebarFileId = Guid.NewGuid();

        var service = new EnterpriseService(
            new FakeUnitOfWork(enterprise),
            new FakeLocalizationService(),
            new FakeBrandingService());

        var response = await service.Update(request);

        Assert.True(response.Result);
        Assert.Equal("Current brand", request.BrandName);
        Assert.Equal("#123456", request.PrimaryColor);
        Assert.Equal(enterprise.LogoMainFileId, request.LogoMainFileId);
        Assert.Equal(enterprise.LogoSidebarFileId, request.LogoSidebarFileId);
    }

    [Fact]
    public async Task UploadLogo_rejects_invalid_file_without_creating_storage()
    {
        var root = CreateTempDirectory();
        try
        {
            var enterprise = NewEnterprise();
            var uow = new FakeUnitOfWork(enterprise);
            var sut = BuildSut(uow, root);
            using var stream = new MemoryStream([1, 2, 3]);
            var file = NewFormFile(stream, "logo.png", "image/png");

            var response = await sut.UploadLogo(enterprise.Id, BrandingLogoSlot.Main, file);

            Assert.False(response.Result);
            Assert.Empty(uow.FilesStore.Store);
            Assert.Equal(0, uow.CompleteCallCount);
            Assert.Empty(Directory.GetFiles(root, "*", SearchOption.AllDirectories));
        }
        finally
        {
            DeleteDirectory(root);
        }
    }

    [Fact]
    public async Task UploadLogo_rejects_missing_file_with_localized_validation()
    {
        var root = CreateTempDirectory();
        try
        {
            var enterprise = NewEnterprise();
            var uow = new FakeUnitOfWork(enterprise);
            var response = await BuildSut(uow, root).UploadLogo(enterprise.Id, BrandingLogoSlot.Main, null);

            Assert.False(response.Result);
            Assert.Contains("BrandingLogoRequired", response.Errors);
            Assert.Empty(uow.FilesStore.Store);
            Assert.Equal(0, uow.CompleteCallCount);
        }
        finally
        {
            DeleteDirectory(root);
        }
    }

    [Fact]
    public async Task UploadLogo_removes_physical_file_when_database_commit_fails()
    {
        var root = CreateTempDirectory();
        try
        {
            var enterprise = NewEnterprise();
            var uow = new FakeUnitOfWork(enterprise)
            {
                CommitException = new InvalidOperationException("database unavailable")
            };
            var sut = BuildSut(uow, root);
            using var stream = new MemoryStream(PngHeader);
            var file = NewFormFile(stream, "logo.png", "image/png");

            var response = await sut.UploadLogo(enterprise.Id, BrandingLogoSlot.Main, file);

            Assert.False(response.Result);
            Assert.Equal(1, uow.CompleteCallCount);
            Assert.Empty(uow.FilesStore.Store);
            Assert.Empty(Directory.GetFiles(root, "*", SearchOption.AllDirectories));
        }
        finally
        {
            DeleteDirectory(root);
        }
    }

    [Fact]
    public async Task UploadLogo_preserves_committed_branding_when_response_read_fails()
    {
        var root = CreateTempDirectory();
        try
        {
            var enterprise = NewEnterprise();
            var uow = new FakeUnitOfWork(enterprise);
            uow.AfterCommit = () => uow.EnterprisesStore.ThrowOnFindAsync = true;
            var sut = BuildSut(uow, root);
            using var stream = new MemoryStream(PngHeader);
            var file = NewFormFile(stream, "logo.png", "image/png");

            var response = await sut.UploadLogo(enterprise.Id, BrandingLogoSlot.Main, file);

            Assert.True(response.Result);
            Assert.Null(response.Content);
            var committedFile = Assert.Single(uow.FilesStore.Store);
            Assert.Equal(committedFile.Id, enterprise.LogoMainFileId);
            Assert.True(System.IO.File.Exists(committedFile.Path));
        }
        finally
        {
            DeleteDirectory(root);
        }
    }

    [Fact]
    public async Task RemoveEnterpriseFiles_removes_orphaned_branding_rows_and_files()
    {
        var root = CreateTempDirectory();
        try
        {
            var enterprise = NewEnterprise();
            var referencedPath = Path.Combine(root, "EnterpriseBranding", "referenced.png");
            var orphanedPath = Path.Combine(root, "EnterpriseBranding", "orphaned.png");
            var sidebarPath = Path.Combine(root, "EnterpriseBranding", "sidebar.png");
            Directory.CreateDirectory(Path.GetDirectoryName(referencedPath)!);
            await System.IO.File.WriteAllBytesAsync(referencedPath, PngHeader);
            await System.IO.File.WriteAllBytesAsync(orphanedPath, PngHeader);
            await System.IO.File.WriteAllBytesAsync(sidebarPath, PngHeader);

            var uow = new FakeUnitOfWork(enterprise);
            uow.FilesStore.Store.AddRange(
            [
                NewBrandingFile(enterprise.Id, referencedPath),
                NewBrandingFile(enterprise.Id, orphanedPath),
                NewBrandingFile(enterprise.Id, sidebarPath, "EnterpriseBranding:sidebar"),
            ]);
            var sut = BuildSut(uow, root);

            var response = await sut.RemoveEnterpriseFiles(enterprise.Id);

            Assert.True(response.Result);
            Assert.Empty(uow.FilesStore.Store);
            Assert.False(System.IO.File.Exists(referencedPath));
            Assert.False(System.IO.File.Exists(orphanedPath));
            Assert.False(System.IO.File.Exists(sidebarPath));
        }
        finally
        {
            DeleteDirectory(root);
        }
    }

    private static BrandingService BuildSut(FakeUnitOfWork uow, string root) =>
        new(
            uow,
            Options.Create(new AppSettings
            {
                FileManagment = new FileManagmentSettings { UploadPath = root }
            }),
            new FakeLocalizationService(),
            NullLogger<BrandingService>.Instance);

    private static Enterprise NewEnterprise() => new()
    {
        Name = "Test Enterprise",
        Disabled = false,
    };

    private static Domain.Entities.File NewBrandingFile(
        Guid enterpriseId,
        string path,
        string entity = "EnterpriseBranding") => new()
    {
        Entity = entity,
        EntityId = enterpriseId,
        Type = FileType.Image,
        Path = path,
        OriginalName = Path.GetFileName(path),
        Size = PngHeader.Length,
    };

    private static FormFile NewFormFile(Stream stream, string fileName, string contentType) =>
        new(stream, 0, stream.Length, "file", fileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = contentType,
        };

    private static string CreateTempDirectory()
    {
        var path = Path.Combine(Path.GetTempPath(), "lilith-branding-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        return path;
    }

    private static void DeleteDirectory(string path)
    {
        if (Directory.Exists(path))
            Directory.Delete(path, recursive: true);
    }

    private sealed class FakeUnitOfWork : IUnitOfWork
    {
        public FakeRepository<Enterprise> EnterprisesStore { get; }
        public FakeRepository<Domain.Entities.File> FilesStore { get; } = new();
        public Exception? CommitException { get; init; }
        public Action? AfterCommit { get; set; }
        public int CompleteCallCount { get; private set; }

        public FakeUnitOfWork(Enterprise enterprise)
        {
            EnterprisesStore = new FakeRepository<Enterprise>();
            EnterprisesStore.Store.Add(enterprise);
        }

        public async Task<int> CompleteAsync()
        {
            CompleteCallCount++;
            if (CommitException is not null)
                throw CommitException;

            EnterprisesStore.Commit();
            FilesStore.Commit();
            AfterCommit?.Invoke();
            await Task.CompletedTask;
            return 2;
        }

        public void Dispose() { }

        public IRepository<Role, Guid> Roles => UnusedRepository<Role>.Instance;
        public IRepository<User, Guid> Users => UnusedRepository<User>.Instance;
        public IRepository<ApiKey, Guid> ApiKeys => UnusedRepository<ApiKey>.Instance;
        public IRepository<UserRefreshToken, Guid> UserRefreshTokens => UnusedRepository<UserRefreshToken>.Instance;
        public IRepository<UserFilter, Guid> UserFilters => UnusedRepository<UserFilter>.Instance;
        public IRepository<UserTableView, Guid> UserTableViews => UnusedRepository<UserTableView>.Instance;
        public IRepository<Profile, Guid> Profiles => UnusedRepository<Profile>.Instance;
        public IRepository<MenuItem, Guid> MenuItems => UnusedRepository<MenuItem>.Instance;
        public IRepository<MenuItemTranslation, Guid> MenuItemTranslations => UnusedRepository<MenuItemTranslation>.Instance;
        public IRepository<ProfileMenuItem, Guid> ProfileMenuItems => UnusedRepository<ProfileMenuItem>.Instance;
        public IRepository<Domain.Entities.File, Guid> Files => FilesStore;
        public IRepository<Parameter, Guid> Parameters => UnusedRepository<Parameter>.Instance;
        public IRepository<Exercise, Guid> Exercices => UnusedRepository<Exercise>.Instance;
        public IRepository<Tax, Guid> Taxes => UnusedRepository<Tax>.Instance;
        public IRepository<PaymentMethod, Guid> PaymentMethods => UnusedRepository<PaymentMethod>.Instance;
        public ILifecycleRepository Lifecycles => throw new NotImplementedException();
        public ILifecycleTagRepository LifecycleTags => throw new NotImplementedException();
        public IRepository<StatusLifecycleTag, Guid> StatusLifecycleTags => UnusedRepository<StatusLifecycleTag>.Instance;
        public IRepository<SupplierType, Guid> SupplierTypes => UnusedRepository<SupplierType>.Instance;
        public ISupplierRepository Suppliers => throw new NotImplementedException();
        public IPurchaseOrderRepository PurchaseOrders => throw new NotImplementedException();
        public IPurchaseInvoiceRepository PurchaseInvoices => throw new NotImplementedException();
        public IRepository<PurchaseInvoiceDueDate, Guid> PurchaseInvoiceDueDates => UnusedRepository<PurchaseInvoiceDueDate>.Instance;
        public IRepository<InvoiceSerie, Guid> InvoiceSeries => UnusedRepository<InvoiceSerie>.Instance;
        public IRepository<ExpenseType, Guid> ExpenseTypes => UnusedRepository<ExpenseType>.Instance;
        public IExpenseRepository Expenses => throw new NotImplementedException();
        public IReceiptRepository Receipts => throw new NotImplementedException();
        public IRepository<ReferenceFormat, Guid> ReferenceFormats => UnusedRepository<ReferenceFormat>.Instance;
        public IContractReader<ConsolidatedExpense> ConsolidatedExpenses => throw new NotImplementedException();
        public ITransportRateRepository TransportRates => throw new NotImplementedException();
        public IRepository<TransportRateDetail, Guid> TransportRateDetails => UnusedRepository<TransportRateDetail>.Instance;
        public IPurchaseRateRepository PurchaseRates => throw new NotImplementedException();
        public IRepository<PurchaseRateDetail, Guid> PurchaseRateDetails => UnusedRepository<PurchaseRateDetail>.Instance;
        public IRepository<CustomerType, Guid> CustomerTypes => UnusedRepository<CustomerType>.Instance;
        public ICustomerRepository Customers => throw new NotImplementedException();
        public IRepository<Reference, Guid> References => UnusedRepository<Reference>.Instance;
        public ISalesOrderHeaderRepository SalesOrderHeaders => throw new NotImplementedException();
        public ISalesOrderDetailRepository SalesOrderDetails => throw new NotImplementedException();
        public ISalesInvoiceRepository SalesInvoices => throw new NotImplementedException();
        public IRepository<SalesInvoiceVerifactuRequest, Guid> VerifactuRequests => UnusedRepository<SalesInvoiceVerifactuRequest>.Instance;
        public IDeliveryNoteRepository DeliveryNotes => throw new NotImplementedException();
        public IBudgetRepository Budgets => throw new NotImplementedException();
        public IContractReader<ConsolidatedIncomes> ConsolidatedIncomes => throw new NotImplementedException();
        public IRepository<Enterprise, Guid> Enterprises => EnterprisesStore;
        public IRepository<Site, Guid> Sites => UnusedRepository<Site>.Instance;
        public IAreaRepository Areas => throw new NotImplementedException();
        public IRepository<WorkcenterType, Guid> WorkcenterTypes => UnusedRepository<WorkcenterType>.Instance;
        public IWorkcenterRepository Workcenters => throw new NotImplementedException();
        public IRepository<WorkcenterCost, Guid> WorkcenterCosts => UnusedRepository<WorkcenterCost>.Instance;
        public IRepository<Operator, Guid> Operators => UnusedRepository<Operator>.Instance;
        public IRepository<OperatorType, Guid> OperatorTypes => UnusedRepository<OperatorType>.Instance;
        public IMachineStatusRepository MachineStatuses => throw new NotImplementedException();
        public IRepository<Shift, Guid> Shifts => UnusedRepository<Shift>.Instance;
        public IRepository<ShiftDetail, Guid> ShiftDetails => UnusedRepository<ShiftDetail>.Instance;
        public IWorkMasterRepository WorkMasters => throw new NotImplementedException();
        public IWorkOrderRepository WorkOrders => throw new NotImplementedException();
        public IProductionPartRepository ProductionParts => throw new NotImplementedException();
        public IWorkcenterShiftRepository WorkcenterShifts => throw new NotImplementedException();
        public IContractReader<DetailedWorkOrder> DetailedWorkOrders => throw new NotImplementedException();
        public IContractReader<ProductionCost> ProductionCosts => throw new NotImplementedException();
        public IContractReader<WorkcenterShiftHistoricalOperator> WorkcenterShiftHistoricalOperators => throw new NotImplementedException();
        public IWorkcenterProfitPercentageRepository WorkcenterProfitPercentages => throw new NotImplementedException();
        public IPhaseTemplateRepository PhaseTemplates => throw new NotImplementedException();
        public IWarehouseRepository Warehouses => throw new NotImplementedException();
        public IRepository<WorkcenterLocation, Guid> WorkcenterLocations => UnusedRepository<WorkcenterLocation>.Instance;
        public IRepository<ReferenceType, Guid> ReferenceTypes => UnusedRepository<ReferenceType>.Instance;
        public IRepository<Stock, Guid> Stocks => UnusedRepository<Stock>.Instance;
        public IStockMovementRepository StockMovements => throw new NotImplementedException();
    }

    private sealed class FakeRepository<TEntity> : IRepository<TEntity, Guid>
        where TEntity : class
    {
        public List<TEntity> Store { get; } = new();
        private readonly List<TEntity> _pending = new();
        public bool ThrowOnFindAsync { get; set; }

        public Task<TEntity?> Get(Guid id) =>
            Task.FromResult(Store.FirstOrDefault(entity => ((Entity)(object)entity).Id == id));

        public Task<IEnumerable<TEntity>> GetAll() => Task.FromResult<IEnumerable<TEntity>>(Store);

        public Task<List<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate)
        {
            if (ThrowOnFindAsync)
                throw new InvalidOperationException("response read failed");
            return Task.FromResult(Store.AsQueryable().Where(predicate).ToList());
        }

        public Task<List<TEntity>> FindAsyncWithQueryParams(
            Expression<Func<TEntity, bool>> predicate,
            Func<IQueryable<TEntity>, IQueryable<TEntity>>? includeFunc) =>
            Task.FromResult(Store.AsQueryable().Where(predicate).ToList());

        public IEnumerable<TEntity> Find(Expression<Func<TEntity, bool>> predicate) =>
            Store.AsQueryable().Where(predicate).ToList();

        public Task<bool> Exists(Guid id) => Task.FromResult(Store.Any(entity => ((Entity)(object)entity).Id == id));

        public Task Add(TEntity entity)
        {
            Store.Add(entity);
            return Task.CompletedTask;
        }

        public Task AddWithoutSave(TEntity entity)
        {
            _pending.Add(entity);
            return Task.CompletedTask;
        }

        public Task AddRange(IEnumerable<TEntity> entities)
        {
            Store.AddRange(entities);
            return Task.CompletedTask;
        }

        public Task AddRangeWithoutSave(IEnumerable<TEntity> entities)
        {
            _pending.AddRange(entities);
            return Task.CompletedTask;
        }

        public Task Update(TEntity entity) => Task.CompletedTask;

        public bool UpdateWithoutSave(TEntity entity) => true;

        public Task Remove(TEntity entity)
        {
            Store.Remove(entity);
            return Task.CompletedTask;
        }

        public Task RemoveRange(IEnumerable<TEntity> entities)
        {
            foreach (var entity in entities)
                Store.Remove(entity);
            return Task.CompletedTask;
        }

        public Task SaveChanges() => Task.CompletedTask;

        public void Commit()
        {
            Store.AddRange(_pending);
            _pending.Clear();
        }
    }

    private sealed class UnusedRepository<TEntity> : IRepository<TEntity, Guid>
        where TEntity : class
    {
        public static UnusedRepository<TEntity> Instance { get; } = new();
        public Task<TEntity?> Get(Guid id) => throw new NotImplementedException();
        public Task<IEnumerable<TEntity>> GetAll() => throw new NotImplementedException();
        public Task<List<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate) => throw new NotImplementedException();
        public Task<List<TEntity>> FindAsyncWithQueryParams(Expression<Func<TEntity, bool>> predicate, Func<IQueryable<TEntity>, IQueryable<TEntity>>? includeFunc) => throw new NotImplementedException();
        public IEnumerable<TEntity> Find(Expression<Func<TEntity, bool>> predicate) => throw new NotImplementedException();
        public Task<bool> Exists(Guid id) => throw new NotImplementedException();
        public Task Add(TEntity entity) => throw new NotImplementedException();
        public Task AddWithoutSave(TEntity entity) => throw new NotImplementedException();
        public Task AddRange(IEnumerable<TEntity> entities) => throw new NotImplementedException();
        public Task AddRangeWithoutSave(IEnumerable<TEntity> entities) => throw new NotImplementedException();
        public Task Update(TEntity entity) => throw new NotImplementedException();
        public bool UpdateWithoutSave(TEntity entity) => throw new NotImplementedException();
        public Task Remove(TEntity entity) => throw new NotImplementedException();
        public Task RemoveRange(IEnumerable<TEntity> entities) => throw new NotImplementedException();
        public Task SaveChanges() => throw new NotImplementedException();
    }

    private sealed class FakeLocalizationService : ILocalizationService
    {
        public string GetLocalizedString(string key, params object[] arguments) => key;
        public string GetLocalizedStringForCulture(string key, string culture, params object[] arguments) => key;
        public Dictionary<string, string> GetAllTranslations() => new();
        public Dictionary<string, string> GetAllTranslationsForCulture(string culture) => new();
        public string[] GetSupportedCultures() => [];
    }

    private sealed class FakeBrandingService : IBrandingService
    {
        public Task<BrandingResponse> GetCurrent() => Task.FromResult(BrandingResponse.Default);
        public Task<BrandingLogoContent?> GetCurrentLogo(BrandingLogoSlot slot) => Task.FromResult<BrandingLogoContent?>(null);
        public Task<GenericResponse> UpdateCurrent(BrandingUpdateRequest request) => Task.FromResult(new GenericResponse(true));
        public Task<GenericResponse> UploadCurrentLogo(BrandingLogoSlot slot, IFormFile? file) => Task.FromResult(new GenericResponse(true));
        public Task<GenericResponse> RemoveCurrentLogo(BrandingLogoSlot slot) => Task.FromResult(new GenericResponse(true));
        public Task<GenericResponse> UploadLogo(Guid enterpriseId, BrandingLogoSlot slot, IFormFile? file) => Task.FromResult(new GenericResponse(true));
        public Task<GenericResponse> RemoveLogo(Guid enterpriseId, BrandingLogoSlot slot) => Task.FromResult(new GenericResponse(true));
        public Task<GenericResponse> RemoveEnterpriseFiles(Guid enterpriseId) => Task.FromResult(new GenericResponse(true));
    }
}
