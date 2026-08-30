using Application.Contracts;
using Application.Services.Warehouse;
using Domain.Entities.Shared;
using Domain.Entities.Warehouse;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System.Linq.Expressions;
using Xunit;
using WarehouseEntity = Domain.Entities.Warehouse.Warehouse;

namespace Application.Tests.Services.Batch
{

/// <summary>
/// Tests unitaris per a StockMovementService.ApplyDeliveryNoteStockBatch.
/// Cobreix: batch normal, referències repetides acumulades i referència Service ignorada.
/// </summary>
public class StockMovementServiceBatchTests
{
    // ------------------------------------------------------------------ setup

    private static BatchContext BuildContext(
        IEnumerable<Reference>? references = null,
        IEnumerable<Stock>? existingStocks = null,
        IEnumerable<Lot>? lots = null,
        bool hasDefaultLocation = true)
    {
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var warehouses = Substitute.For<IWarehouseRepository>();
        var refsRepo = Substitute.For<IRepository<Reference, Guid>>();
        var stocksRepo = Substitute.For<IRepository<Stock, Guid>>();
        var stockMovementsRepo = Substitute.For<IStockMovementRepository>();
        var lotsRepo = Substitute.For<ILotRepository>();
        var localization = Substitute.For<ILocalizationService>();
        var logger = Substitute.For<ILogger<StockMovementService>>();

        var defaultLocationId = hasDefaultLocation ? Guid.NewGuid() : (Guid?)null;

        // Warehouse per obtenir DefaultLocationId
        var warehouse = new WarehouseEntity { DefaultLocationId = defaultLocationId };
        warehouses.Find(Arg.Any<Expression<Func<WarehouseEntity, bool>>>())
            .Returns(hasDefaultLocation ? [warehouse] : Array.Empty<WarehouseEntity>());

        unitOfWork.Warehouses.Returns(warehouses);
        unitOfWork.References.Returns(refsRepo);
        unitOfWork.Stocks.Returns(stocksRepo);
        unitOfWork.StockMovements.Returns(stockMovementsRepo);
        unitOfWork.Lots.Returns(lotsRepo);

        var allRefs = references?.ToList() ?? [];
        refsRepo.Find(Arg.Any<Expression<Func<Reference, bool>>>())
            .Returns(allRefs);

        var allStocks = existingStocks?.ToList() ?? [];
        stocksRepo.Find(Arg.Any<Expression<Func<Stock, bool>>>())
            .Returns(allStocks);

        var allLots = lots?.ToList() ?? [];
        lotsRepo.Find(Arg.Any<Expression<Func<Lot, bool>>>())
            .Returns(allLots);

        localization.GetLocalizedString(Arg.Any<string>(), Arg.Any<object[]>())
            .Returns(call => call.ArgAt<string>(0));
        localization.GetLocalizedString(Arg.Any<string>())
            .Returns(call => call.ArgAt<string>(0));

        // Configurar CompleteAsync al final, sense intercalar altres mocks
        unitOfWork.CompleteAsync().Returns(Task.FromResult(1));

        var service = new StockMovementService(unitOfWork, localization, logger);

        return new BatchContext(service, unitOfWork, stocksRepo, stockMovementsRepo, lotsRepo, defaultLocationId);
    }

    private sealed record BatchContext(
        StockMovementService Service,
        IUnitOfWork UnitOfWork,
        IRepository<Stock, Guid> StocksRepo,
        IStockMovementRepository MovementsRepo,
        ILotRepository LotsRepo,
        Guid? DefaultLocationId);

    // ------------------------------------------------------------------ tests

    [Fact]
    public async Task Batch_normal_adds_stock_and_movements_with_single_save()
    {
        var refId = Guid.NewGuid();
        var reference = new Reference { Id = refId, CategoryName = ReferenceCategories.Product };
        var ctx = BuildContext(references: [reference]);

        var movements = new[]
        {
            new StockMovement
            {
                ReferenceId = refId,
                MovementType = StockMovementType.OUTPUT,
                Quantity = 5,
                Description = "Test",
                MovementDate = DateTime.Now,
                CreatedOn = DateTime.Now,
            },
        };

        var response = await ctx.Service.ApplyDeliveryNoteStockBatch(movements);

        Assert.True(response.Result);
        await ctx.StocksRepo.Received(1).AddWithoutSave(Arg.Any<Stock>());
        await ctx.MovementsRepo.Received(1).AddWithoutSave(Arg.Any<StockMovement>());
        await ctx.UnitOfWork.Received(1).CompleteAsync();
    }

    [Fact]
    public async Task Batch_with_repeated_reference_accumulates_into_single_stock()
    {
        var refId = Guid.NewGuid();
        var reference = new Reference { Id = refId, CategoryName = ReferenceCategories.Product };
        var ctx = BuildContext(references: [reference]);

        // Dos moviments OUTPUT amb la mateixa referència i dimensions per defecte
        var movements = new[]
        {
            new StockMovement
            {
                ReferenceId = refId,
                MovementType = StockMovementType.OUTPUT,
                Quantity = 3,
                Description = "Test",
                MovementDate = DateTime.Now,
                CreatedOn = DateTime.Now,
            },
            new StockMovement
            {
                ReferenceId = refId,
                MovementType = StockMovementType.OUTPUT,
                Quantity = 7,
                Description = "Test",
                MovementDate = DateTime.Now,
                CreatedOn = DateTime.Now,
            },
        };

        var response = await ctx.Service.ApplyDeliveryNoteStockBatch(movements);

        Assert.True(response.Result);
        // Un únic stock creat (no duplicat), però dos moviments
        await ctx.StocksRepo.Received(1).AddWithoutSave(
            Arg.Is<Stock>(stock => stock.Quantity == -10));
        await ctx.MovementsRepo.Received(2).AddWithoutSave(Arg.Any<StockMovement>());
        Assert.All(movements, movement => Assert.Equal(-Math.Abs(movement.Quantity), movement.Quantity));
        Assert.NotEqual(Guid.Empty, movements[0].StockId);
        Assert.Equal(movements[0].StockId, movements[1].StockId);
        await ctx.UnitOfWork.Received(1).CompleteAsync();
    }

    [Fact]
    public async Task Batch_with_repeated_reference_updates_existing_stock()
    {
        var refId = Guid.NewGuid();
        var locationId = Guid.NewGuid();
        var reference = new Reference { Id = refId, CategoryName = ReferenceCategories.Material };
        var existingStock = new Stock
        {
            Id = Guid.NewGuid(),
            ReferenceId = refId,
            LocationId = locationId,
            Quantity = 10,
        };
        var ctx = BuildContext(references: [reference], existingStocks: [existingStock]);

        var movements = new[]
        {
            new StockMovement
            {
                ReferenceId = refId,
                LocationId = locationId,
                MovementType = StockMovementType.OUTPUT,
                Quantity = 4,
                Description = "Test",
                MovementDate = DateTime.Now,
                CreatedOn = DateTime.Now,
            },
        };

        var response = await ctx.Service.ApplyDeliveryNoteStockBatch(movements);

        Assert.True(response.Result);
        // Stock existent actualitzat, no creat nou
        await ctx.StocksRepo.DidNotReceive().AddWithoutSave(Arg.Any<Stock>());
        ctx.StocksRepo.Received(1).UpdateWithoutSave(
            Arg.Is<Stock>(stock => stock.Id == existingStock.Id && stock.Quantity == 6));
        await ctx.UnitOfWork.Received(1).CompleteAsync();
    }

    [Fact]
    public async Task Batch_service_reference_is_skipped_without_error()
    {
        var refId = Guid.NewGuid();
        var reference = new Reference { Id = refId, CategoryName = ReferenceCategories.Service };
        var ctx = BuildContext(references: [reference]);

        var movements = new[]
        {
            new StockMovement
            {
                ReferenceId = refId,
                MovementType = StockMovementType.OUTPUT,
                Quantity = 2,
                Description = "Service line",
                MovementDate = DateTime.Now,
                CreatedOn = DateTime.Now,
            },
        };

        var response = await ctx.Service.ApplyDeliveryNoteStockBatch(movements);

        Assert.True(response.Result);
        await ctx.StocksRepo.DidNotReceive().AddWithoutSave(Arg.Any<Stock>());
        await ctx.MovementsRepo.DidNotReceive().AddWithoutSave(Arg.Any<StockMovement>());
        // No s'executa CompleteAsync perquè no hi ha res a persistir
        await ctx.UnitOfWork.DidNotReceive().CompleteAsync();
    }

    [Fact]
    public async Task Batch_unknown_reference_returns_error()
    {
        // No afegim cap referència → el lookup retorna buit
        var ctx = BuildContext(references: []);

        var movements = new[]
        {
            new StockMovement
            {
                ReferenceId = Guid.NewGuid(),
                MovementType = StockMovementType.OUTPUT,
                Quantity = 1,
                Description = "Bad",
                MovementDate = DateTime.Now,
                CreatedOn = DateTime.Now,
            },
        };

        var response = await ctx.Service.ApplyDeliveryNoteStockBatch(movements);

        Assert.False(response.Result);
        await ctx.UnitOfWork.DidNotReceive().CompleteAsync();
    }

    [Fact]
    public async Task Batch_without_default_location_returns_error()
    {
        var ctx = BuildContext(hasDefaultLocation: false);

        var response = await ctx.Service.ApplyDeliveryNoteStockBatch(
            [new StockMovement { ReferenceId = Guid.NewGuid(), Quantity = 1 }]);

        Assert.False(response.Result);
        await ctx.UnitOfWork.DidNotReceive().CompleteAsync();
    }

    [Fact]
    public async Task Batch_empty_list_succeeds_without_db_calls()
    {
        var ctx = BuildContext();

        var response = await ctx.Service.ApplyDeliveryNoteStockBatch([]);

        Assert.True(response.Result);
        await ctx.UnitOfWork.DidNotReceive().CompleteAsync();
    }

    [Fact]
    public async Task Batch_keeps_stock_separated_by_lot()
    {
        var referenceId = Guid.NewGuid();
        var firstLotId = Guid.NewGuid();
        var secondLotId = Guid.NewGuid();
        var reference = new Reference { Id = referenceId, CategoryName = ReferenceCategories.Product };
        var ctx = BuildContext(
            references: [reference],
            lots:
            [
                new Lot { Id = firstLotId },
                new Lot { Id = secondLotId },
            ]);

        var response = await ctx.Service.ApplyDeliveryNoteStockBatch(
        [
            new StockMovement
            {
                ReferenceId = referenceId,
                LotId = firstLotId,
                MovementType = StockMovementType.OUTPUT,
                Quantity = 3,
            },
            new StockMovement
            {
                ReferenceId = referenceId,
                LotId = secondLotId,
                MovementType = StockMovementType.OUTPUT,
                Quantity = 4,
            },
        ]);

        Assert.True(response.Result);
        await ctx.StocksRepo.Received(2).AddWithoutSave(Arg.Any<Stock>());
        await ctx.StocksRepo.Received(1).AddWithoutSave(
            Arg.Is<Stock>(stock => stock.LotId == firstLotId && stock.Quantity == -3));
        await ctx.StocksRepo.Received(1).AddWithoutSave(
            Arg.Is<Stock>(stock => stock.LotId == secondLotId && stock.Quantity == -4));
        await ctx.UnitOfWork.Received(1).CompleteAsync();
    }

    [Fact]
    public async Task Batch_rejects_input_into_closed_lot()
    {
        var referenceId = Guid.NewGuid();
        var lotId = Guid.NewGuid();
        var reference = new Reference { Id = referenceId, CategoryName = ReferenceCategories.Product };
        var ctx = BuildContext(
            references: [reference],
            lots: [new Lot { Id = lotId, ClosedDate = DateTime.Now }]);

        var response = await ctx.Service.ApplyDeliveryNoteStockBatch(
        [
            new StockMovement
            {
                ReferenceId = referenceId,
                LotId = lotId,
                MovementType = StockMovementType.INPUT,
                Quantity = 3,
            },
        ]);

        Assert.False(response.Result);
        await ctx.StocksRepo.DidNotReceive().AddWithoutSave(Arg.Any<Stock>());
        await ctx.UnitOfWork.DidNotReceive().CompleteAsync();
    }

    [Fact]
    public async Task Batch_updates_lot_remaining_quantity_in_same_save()
    {
        var referenceId = Guid.NewGuid();
        var locationId = Guid.NewGuid();
        var lot = new Lot { Id = Guid.NewGuid(), RemainingQuantity = 10 };
        var reference = new Reference { Id = referenceId, CategoryName = ReferenceCategories.Product };
        var stock = new Stock
        {
            Id = Guid.NewGuid(),
            ReferenceId = referenceId,
            LocationId = locationId,
            LotId = lot.Id,
            Quantity = 10,
        };
        var ctx = BuildContext(references: [reference], existingStocks: [stock], lots: [lot]);

        var response = await ctx.Service.ApplyDeliveryNoteStockBatch(
        [
            new StockMovement
            {
                ReferenceId = referenceId,
                LocationId = locationId,
                LotId = lot.Id,
                MovementType = StockMovementType.OUTPUT,
                Quantity = 3,
            },
        ]);

        Assert.True(response.Result);
        ctx.LotsRepo.Received(1).UpdateWithoutSave(
            Arg.Is<Lot>(updatedLot => updatedLot.Id == lot.Id && updatedLot.RemainingQuantity == 7));
        await ctx.UnitOfWork.Received(1).CompleteAsync();
    }
}
}
