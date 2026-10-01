using System.Text.Json;
using Application.Contracts;
using Domain.Entities;
using Domain.Entities.Production;
using Domain.Entities.Purchase;
using Domain.Entities.Sales;
using Domain.Entities.Warehouse;
using Infrastructure.Persistance;
using Infrastructure.Persistance.MasterData;
using Infrastructure.Persistance.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;
using WarehouseEntity = Domain.Entities.Warehouse.Warehouse;

namespace Application.Tests.Persistance;

/// <summary>
/// The generic repository refuses to delete master data in use
/// (docs/adr/0001-master-data-delete-guard.md). The first tests keep the declarations in
/// <see cref="MasterDataCatalog"/> complete as the EF model grows; the rest exercise the
/// guard through <see cref="Repository{Entity, Id}.Remove"/>.
/// </summary>
public class MasterDataDeleteGuardTests
{
    [Fact]
    public void Every_master_data_type_is_named()
    {
        var masterData = typeof(Entity).Assembly.GetTypes()
            .Where(t => typeof(IMasterData).IsAssignableFrom(t) && t is { IsClass: true, IsAbstract: false })
            .ToHashSet();

        Assert.Equal(masterData.OrderBy(t => t.Name), MasterDataCatalog.Names.Keys.OrderBy(t => t.Name));
        Assert.Subset(masterData, MasterDataCatalog.CanBeDisabled.ToHashSet());
    }

    [Fact]
    public void Every_owned_part_refers_to_its_owner_through_one_foreign_key()
    {
        using var context = NewContext();

        foreach (var part in MasterDataCatalog.OwnedParts)
            MasterDataDeleteGuard.OwnerReference(context.Model, part);
    }

    [Fact]
    public void Master_data_cascades_only_to_its_unfiltered_owned_parts()
    {
        using var context = NewContext();
        var ownedParts = MasterDataCatalog.OwnedParts
            .Where(p => p.Filter is null)
            .Select(p => (p.Owner, p.Part))
            .ToHashSet();

        var masterDataKeys = context.Model.GetEntityTypes()
            .SelectMany(t => t.GetForeignKeys())
            .Where(fk => typeof(IMasterData).IsAssignableFrom(fk.PrincipalEntityType.ClrType))
            .ToList();

        Assert.All(masterDataKeys.Where(fk => ownedParts.Contains((fk.PrincipalEntityType.ClrType, fk.DeclaringEntityType.ClrType))),
            fk => Assert.Equal(DeleteBehavior.Cascade, fk.DeleteBehavior));
        Assert.DoesNotContain(masterDataKeys,
            fk => fk.DeleteBehavior == DeleteBehavior.Cascade
                && !ownedParts.Contains((fk.PrincipalEntityType.ClrType, fk.DeclaringEntityType.ClrType)));
    }

    [Fact]
    public void Every_record_that_can_keep_master_data_in_use_has_a_document_kind()
    {
        using var context = NewContext();
        var unnamed = new List<string>();

        foreach (var principal in DeletedTypes())
        {
            var ownedHere = MasterDataCatalog.OwnedParts
                .Where(p => p.Owner == principal && p.Filter is null)
                .Select(p => p.Part)
                .ToHashSet();

            unnamed.AddRange(MasterDataDeleteGuard.IncomingReferences(context.Model, principal)
                .Where(r => !ownedHere.Contains(r.Dependent) && MasterDataDeleteGuard.DocumentKindKey(r.Dependent) is null)
                .Select(r => $"{r.Dependent.Name}.{r.Property} -> {principal.Name}"));
        }

        Assert.Empty(unnamed);
    }

    [Fact]
    public void Every_message_key_exists_in_every_culture()
    {
        var keys = MasterDataCatalog.DocumentKinds.Select(k => k.Key)
            .Append("DocumentKind.Other")
            .Append("MasterData.InUse")
            .Append("MasterData.InUseNamed")
            .Append("MasterData.DisableInstead")
            .Distinct()
            .ToList();

        foreach (var culture in new[] { "ca", "es", "en" })
        {
            var texts = LoadResource(culture);
            Assert.All(keys, key => Assert.True(texts.ContainsKey(key), $"{culture}.json has no {key}"));
        }
    }

    [Fact]
    public async Task Master_data_that_nothing_refers_to_is_deleted()
    {
        var tax = new Tax { Name = "IVA 21%" };
        var database = Seed(tax);

        await Remove<Tax>(database, tax.Id);

        await using var context = NewContext(database);
        Assert.False(await context.Set<Tax>().AnyAsync());
    }

    [Fact]
    public async Task Master_data_in_use_is_refused_with_its_name_and_where_it_is_used()
    {
        var tax = new Tax { Name = "IVA 21%" };
        var database = Seed(tax, new Domain.Entities.Shared.Reference { Code = "REF-1", TaxId = tax.Id });

        var refusal = await Assert.ThrowsAsync<EntityInUseException>(() => Remove<Tax>(database, tax.Id));

        Assert.Equal("IVA 21%", refusal.EntityName);
        Assert.Equal(["DocumentKind.References"], refusal.DocumentKindKeys);
        Assert.True(refusal.CanBeDisabled);
        await using var context = NewContext(database);
        Assert.True(await context.Set<Tax>().AnyAsync());
    }

    [Fact]
    public async Task Kinds_are_listed_once_in_display_order_including_references_without_a_foreign_key()
    {
        var supplier = new Supplier { ComercialName = "Acme" };
        var database = Seed(
            supplier,
            new PurchaseOrder { SupplierId = supplier.Id },
            new PurchaseOrder { SupplierId = supplier.Id },
            new BudgetTransport { LogisticSupplierId = Guid.NewGuid(), DestinationSupplierId = supplier.Id });

        var refusal = await Assert.ThrowsAsync<EntityInUseException>(() => Remove<Supplier>(database, supplier.Id));

        Assert.Equal(["DocumentKind.Budgets", "DocumentKind.PurchaseOrders"], refusal.DocumentKindKeys);
        Assert.False(refusal.CanBeDisabled);
    }

    [Fact]
    public async Task Owned_parts_do_not_keep_their_owner_in_use()
    {
        var customer = new Customer { ComercialName = "Client" };
        var database = Seed(
            customer,
            new CustomerAddress { CustomerId = customer.Id },
            new CustomerContact { CustomerId = customer.Id });

        await Remove<Customer>(database, customer.Id);
    }

    [Fact]
    public async Task A_record_that_refers_to_an_owned_part_keeps_the_owner_in_use()
    {
        var stopped = new MachineStatus { Name = "Aturada" };
        var reason = new MachineStatusReason { MachineStatusId = stopped.Id };
        var database = Seed(
            stopped,
            reason,
            new WorkcenterShiftDetail { MachineStatusId = Guid.NewGuid(), MachineStatusReasonId = reason.Id });

        var refusal = await Assert.ThrowsAsync<EntityInUseException>(() => Remove<MachineStatus>(database, stopped.Id));

        Assert.Equal(["DocumentKind.ShiftHistory"], refusal.DocumentKindKeys);
    }

    [Fact]
    public async Task A_location_with_empty_stock_is_deleted_but_stock_on_hand_keeps_it_in_use()
    {
        var empty = new Location { Name = "A-01" };
        var full = new Location { Name = "A-02" };
        var database = Seed(
            empty,
            full,
            new Stock { LocationId = empty.Id, Quantity = 0 },
            new Stock { LocationId = full.Id, Quantity = 5 });

        await Remove<Location>(database, empty.Id);
        var refusal = await Assert.ThrowsAsync<EntityInUseException>(() => Remove<Location>(database, full.Id));

        Assert.Equal(["DocumentKind.Stock"], refusal.DocumentKindKeys);
        await using var context = NewContext(database);
        Assert.Equal([full.Id], await context.Set<Stock>().Select(s => s.LocationId).ToListAsync());
    }

    // Empty stock is restricted in the database, so the repository deletes it explicitly.
    [Fact]
    public async Task A_warehouse_is_deleted_with_its_locations_and_their_empty_stock()
    {
        var warehouse = new WarehouseEntity { Name = "Central" };
        var location = new Location { Name = "A-01", WarehouseId = warehouse.Id };
        var database = Seed(warehouse, location, new Stock { LocationId = location.Id, Quantity = 0 });

        await Remove<WarehouseEntity>(database, warehouse.Id);

        await using var context = NewContext(database);
        Assert.False(await context.Set<Location>().AnyAsync());
        Assert.False(await context.Set<Stock>().AnyAsync());
    }

    [Fact]
    public async Task A_warehouse_is_in_use_through_the_movements_of_its_locations()
    {
        var warehouse = new WarehouseEntity { Name = "Central" };
        var location = new Location { Name = "A-01", WarehouseId = warehouse.Id };
        var emptyStock = new Stock { LocationId = location.Id, Quantity = 0 };
        var database = Seed(
            warehouse,
            location,
            emptyStock,
            new StockMovement { StockId = emptyStock.Id, LocationId = location.Id });

        var refusal = await Assert.ThrowsAsync<EntityInUseException>(() => Remove<WarehouseEntity>(database, warehouse.Id));

        Assert.Equal(["DocumentKind.StockMovements"], refusal.DocumentKindKeys);
    }

    [Fact]
    public async Task Only_other_warehouses_defaulting_to_its_locations_keep_a_warehouse_in_use()
    {
        var central = new WarehouseEntity { Name = "Central" };
        var location = new Location { Name = "A-01", WarehouseId = central.Id };
        central.DefaultLocationId = location.Id;
        var other = new WarehouseEntity { Name = "Other" };
        var database = Seed(central, location, other);

        await Remove<WarehouseEntity>(database, central.Id);

        var shared = new WarehouseEntity { Name = "Shared" };
        var sharedLocation = new Location { Name = "B-01", WarehouseId = shared.Id };
        var borrower = new WarehouseEntity { Name = "Borrower", DefaultLocationId = sharedLocation.Id };
        var secondDatabase = Seed(shared, sharedLocation, borrower);

        var refusal = await Assert.ThrowsAsync<EntityInUseException>(() => Remove<WarehouseEntity>(secondDatabase, shared.Id));
        Assert.Equal(["DocumentKind.Warehouses"], refusal.DocumentKindKeys);
    }

    [Fact]
    public async Task References_that_are_set_to_null_on_delete_do_not_keep_master_data_in_use()
    {
        var site = new Site { Name = "Planta 1" };
        var database = Seed(site, new Enterprise { Name = "Empresa", DefaultSiteId = site.Id });

        await Remove<Site>(database, site.Id);
    }

    [Fact]
    public async Task Disabled_records_still_keep_master_data_in_use()
    {
        var supplier = new Supplier { ComercialName = "Acme" };
        var database = Seed(supplier, new PurchaseRate { SupplierId = supplier.Id, Disabled = true });

        var refusal = await Assert.ThrowsAsync<EntityInUseException>(() => Remove<Supplier>(database, supplier.Id));

        Assert.Equal(["DocumentKind.PurchaseRates"], refusal.DocumentKindKeys);
    }

    [Fact]
    public async Task Records_that_are_not_master_data_are_not_guarded()
    {
        var order = new PurchaseOrder();
        var database = Seed(order, new PurchaseOrderDetail { PurchaseOrderId = order.Id });

        await Remove<PurchaseOrder>(database, order.Id);
    }

    // Every type a master data delete can remove: the master data and, recursively, its owned parts.
    private static IEnumerable<Type> DeletedTypes()
    {
        var types = new HashSet<Type>(MasterDataCatalog.Names.Keys);
        var pending = new Queue<Type>(types);
        while (pending.Count > 0)
        {
            var owner = pending.Dequeue();
            foreach (var part in MasterDataCatalog.OwnedParts.Where(p => p.Owner == owner))
            {
                if (types.Add(part.Part))
                    pending.Enqueue(part.Part);
            }
        }

        return types;
    }

    // Deletes the way services do: load without tracking, then remove through the repository.
    private static async Task Remove<T>(string database, Guid id) where T : Entity
    {
        await using var context = NewContext(database);
        var entity = await context.Set<T>().AsNoTracking().SingleAsync(e => e.Id == id);
        await new Repository<T, Guid>(context).Remove(entity);
    }

    private static string Seed(params Entity[] entities)
    {
        var database = Guid.NewGuid().ToString();
        using var context = NewContext(database);
        context.AddRange(entities);
        context.SaveChanges();
        return database;
    }

    private static ApplicationDbContext NewContext(string? database = null) =>
        new(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(database ?? Guid.NewGuid().ToString())
            .Options);

    private static Dictionary<string, string> LoadResource(string culture)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !Directory.Exists(Path.Combine(directory.FullName, "src", "Api")))
            directory = directory.Parent;
        Assert.NotNull(directory);

        var path = Path.Combine(directory.FullName, "src", "Api", "Resources", "LocalizationService", $"{culture}.json");
        using var document = JsonDocument.Parse(System.IO.File.ReadAllText(path));
        return document.RootElement.GetProperty("texts").EnumerateObject()
            .ToDictionary(p => p.Name, p => p.Value.GetString() ?? string.Empty);
    }
}
