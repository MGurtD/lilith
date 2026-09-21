using Application.Contracts;
using Application.Services.Sales;
using Domain.Entities;
using Domain.Entities.Sales;
using Domain.Entities.Shared;
using NSubstitute;
using System.Linq.Expressions;
using Xunit;

namespace Application.Tests.Services.Sales;

/// <summary>
/// Tests de regressió per al flux Deliver/UnDeliver de SalesOrderService.
/// Objectiu: verificar que ChangeDeliveryStatus prepara tots els canvis
/// sense saves individuals i crida exactament un CompleteAsync per a tota
/// l'etapa de comandes.
/// </summary>
public class SalesOrderServiceDeliveryTests
{
    // -----------------------------------------------------------------------
    // Deliver
    // -----------------------------------------------------------------------

    [Fact(DisplayName = "Deliver ha de cridar CompleteAsync exactament una vegada per a N comandes i M detalls")]
    public async Task Deliver_should_call_CompleteAsync_exactly_once_for_all_orders_and_details()
    {
        var ctx = BuildContext(isDelivered: false, orderCount: 2, detailsPerOrder: 3);

        var response = await ctx.Service.Deliver(ctx.DeliveryNoteId);

        Assert.True(response.Result);
        await ctx.UnitOfWork.Received(1).CompleteAsync();
    }

    [Fact(DisplayName = "Deliver ha de marcar tots els detalls com IsDelivered = true")]
    public async Task Deliver_should_set_IsDelivered_true_on_all_details()
    {
        var ctx = BuildContext(isDelivered: false, orderCount: 1, detailsPerOrder: 2);

        var response = await ctx.Service.Deliver(ctx.DeliveryNoteId);

        Assert.True(response.Result);
        Assert.All(ctx.UpdatedDetailsWithoutSave, d => Assert.True(d.IsDelivered));
    }

    [Fact(DisplayName = "Deliver ha de preparar cada detall sense Update individual")]
    public async Task Deliver_should_use_UpdateWithoutSave_not_Update_per_detail()
    {
        const int detailsPerOrder = 3;
        var ctx = BuildContext(isDelivered: false, orderCount: 1, detailsPerOrder: detailsPerOrder);

        await ctx.Service.Deliver(ctx.DeliveryNoteId);

        // UpdateWithoutSave s'ha de cridar per cada detall
        ctx.SalesOrderDetails.Received(detailsPerOrder).UpdateWithoutSave(Arg.Any<SalesOrderDetail>());
        // UpdateDetail (amb save individual) NO s'ha de cridar
        await ctx.SalesOrderHeaders.DidNotReceive().UpdateDetail(Arg.Any<SalesOrderDetail>());
    }

    [Fact(DisplayName = "Deliver ha de cridar UpdateWithoutSave al header (sense Update individual)")]
    public async Task Deliver_should_use_UpdateWithoutSave_not_Update_per_header()
    {
        var ctx = BuildContext(isDelivered: false, orderCount: 2, detailsPerOrder: 1);

        await ctx.Service.Deliver(ctx.DeliveryNoteId);

        // UpdateWithoutSave s'ha de cridar per cada capçalera
        ctx.SalesOrderHeaders.Received(2).UpdateWithoutSave(Arg.Any<SalesOrderHeader>());
        // Update (amb save individual) NO s'ha de cridar en el flux de lliurament
        await ctx.SalesOrderHeaders.DidNotReceive().Update(Arg.Any<SalesOrderHeader>());
    }

    [Fact(DisplayName = "Deliver ha d'assignar l'estat ComandaServida a totes les capçaleres")]
    public async Task Deliver_should_set_ComandaServida_status_on_all_headers()
    {
        var ctx = BuildContext(isDelivered: false, orderCount: 2, detailsPerOrder: 1);

        var response = await ctx.Service.Deliver(ctx.DeliveryNoteId);

        Assert.True(response.Result);
        Assert.All(ctx.UpdatedHeadersWithoutSave, h => Assert.Equal(ctx.ComandaServida.Id, h.StatusId));
    }

    [Fact(DisplayName = "Deliver no ha de reassignar DeliveryNoteId si ja estava assignat")]
    public async Task Deliver_should_not_reassign_DeliveryNoteId_if_already_set()
    {
        var ctx = BuildContext(isDelivered: false, orderCount: 1, detailsPerOrder: 1);

        await ctx.Service.Deliver(ctx.DeliveryNoteId);

        // El DeliveryNoteId ja estava assignat (per AddOrder); ha de mantenir-se igual
        Assert.All(ctx.UpdatedHeadersWithoutSave, h => Assert.Equal(ctx.DeliveryNoteId, h.DeliveryNoteId));
    }

    // -----------------------------------------------------------------------
    // UnDeliver
    // -----------------------------------------------------------------------

    [Fact(DisplayName = "UnDeliver ha de cridar CompleteAsync exactament una vegada per a N comandes i M detalls")]
    public async Task UnDeliver_should_call_CompleteAsync_exactly_once()
    {
        var ctx = BuildContext(isDelivered: true, orderCount: 2, detailsPerOrder: 3);

        var response = await ctx.Service.UnDeliver(ctx.DeliveryNoteId);

        Assert.True(response.Result);
        await ctx.UnitOfWork.Received(1).CompleteAsync();
    }

    [Fact(DisplayName = "UnDeliver ha de marcar tots els detalls com IsDelivered = false")]
    public async Task UnDeliver_should_set_IsDelivered_false_on_all_details()
    {
        var ctx = BuildContext(isDelivered: true, orderCount: 1, detailsPerOrder: 2);

        var response = await ctx.Service.UnDeliver(ctx.DeliveryNoteId);

        Assert.True(response.Result);
        Assert.All(ctx.UpdatedDetailsWithoutSave, d => Assert.False(d.IsDelivered));
    }

    [Fact(DisplayName = "UnDeliver ha d'assignar l'estat Comanda a totes les capçaleres")]
    public async Task UnDeliver_should_set_Comanda_status_on_all_headers()
    {
        var ctx = BuildContext(isDelivered: true, orderCount: 2, detailsPerOrder: 1);

        var response = await ctx.Service.UnDeliver(ctx.DeliveryNoteId);

        Assert.True(response.Result);
        Assert.All(ctx.UpdatedHeadersWithoutSave, h => Assert.Equal(ctx.Comanda.Id, h.StatusId));
    }

    [Fact(DisplayName = "UnDeliver ha de preparar cada detall sense Update individual")]
    public async Task UnDeliver_should_use_UpdateWithoutSave_not_Update_per_detail()
    {
        const int detailsPerOrder = 4;
        var ctx = BuildContext(isDelivered: true, orderCount: 1, detailsPerOrder: detailsPerOrder);

        await ctx.Service.UnDeliver(ctx.DeliveryNoteId);

        ctx.SalesOrderDetails.Received(detailsPerOrder).UpdateWithoutSave(Arg.Any<SalesOrderDetail>());
        await ctx.SalesOrderHeaders.DidNotReceive().UpdateDetail(Arg.Any<SalesOrderDetail>());
    }

    // -----------------------------------------------------------------------
    // Persistència única — contracte principal
    // -----------------------------------------------------------------------

    [Fact(DisplayName = "Cap Update ni UpdateDetail (amb save individual) s'ha de cridar durant Deliver ni UnDeliver")]
    public async Task Deliver_and_UnDeliver_must_never_call_Update_with_save()
    {
        var ctxDeliver = BuildContext(isDelivered: false, orderCount: 3, detailsPerOrder: 4);
        await ctxDeliver.Service.Deliver(ctxDeliver.DeliveryNoteId);

        await ctxDeliver.SalesOrderHeaders.DidNotReceive().Update(Arg.Any<SalesOrderHeader>());
        await ctxDeliver.SalesOrderHeaders.DidNotReceive().UpdateDetail(Arg.Any<SalesOrderDetail>());
        await ctxDeliver.UnitOfWork.Received(1).CompleteAsync();

        var ctxUnDeliver = BuildContext(isDelivered: true, orderCount: 3, detailsPerOrder: 4);
        await ctxUnDeliver.Service.UnDeliver(ctxUnDeliver.DeliveryNoteId);

        await ctxUnDeliver.SalesOrderHeaders.DidNotReceive().Update(Arg.Any<SalesOrderHeader>());
        await ctxUnDeliver.SalesOrderHeaders.DidNotReceive().UpdateDetail(Arg.Any<SalesOrderDetail>());
        await ctxUnDeliver.UnitOfWork.Received(1).CompleteAsync();
    }

    // -----------------------------------------------------------------------
    // Infrastructure helpers
    // -----------------------------------------------------------------------

    private sealed record TestContext(
        SalesOrderService Service,
        IUnitOfWork UnitOfWork,
        ISalesOrderHeaderRepository SalesOrderHeaders,
        ISalesOrderDetailRepository SalesOrderDetails,
        Guid DeliveryNoteId,
        Status Comanda,
        Status ComandaServida,
        List<SalesOrderDetail> UpdatedDetailsWithoutSave,
        List<SalesOrderHeader> UpdatedHeadersWithoutSave);

    private static TestContext BuildContext(bool isDelivered, int orderCount, int detailsPerOrder)
    {
        var deliveryNoteId = Guid.NewGuid();
        var comanda = new Status { Id = Guid.NewGuid(), Name = StatusConstants.Statuses.Comanda };
        var comandaServida = new Status { Id = Guid.NewGuid(), Name = StatusConstants.Statuses.ComandaServida };

        // Lifecycle amb els dos estats
        var lifecycle = new Lifecycle
        {
            Id = Guid.NewGuid(),
            Name = StatusConstants.Lifecycles.SalesOrder,
            Statuses = new List<Status> { comanda, comandaServida },
        };

        // Construir comandes amb detalls.
        // Tant per a Deliver com per a UnDeliver, les comandes ja tenen el DeliveryNoteId
        // assignat (AddOrder ho fa prèviament al lliurament de l'albarà).
        var orders = Enumerable.Range(0, orderCount).Select(_ =>
        {
            var details = Enumerable.Range(0, detailsPerOrder).Select(__ => new SalesOrderDetail
            {
                Id = Guid.NewGuid(),
                ReferenceId = Guid.NewGuid(),
                Description = "Test",
                Quantity = 1,
                IsDelivered = isDelivered,
            }).ToList();

            return new SalesOrderHeader
            {
                Id = Guid.NewGuid(),
                DeliveryNoteId = deliveryNoteId,   // sempre assignat: AddOrder ho fa avant
                StatusId = isDelivered ? comandaServida.Id : comanda.Id,
                SalesOrderDetails = details,
            };
        }).ToList();

        var unitOfWork = Substitute.For<IUnitOfWork>();
        var salesOrderHeaders = Substitute.For<ISalesOrderHeaderRepository>();
        var salesOrderDetails = Substitute.For<ISalesOrderDetailRepository>();

        // Capturem els detalls i capçaleres actualitzats sense save
        var updatedDetailsWithoutSave = new List<SalesOrderDetail>();
        var updatedHeadersWithoutSave = new List<SalesOrderHeader>();

        salesOrderHeaders
            .Find(Arg.Any<Expression<Func<SalesOrderHeader, bool>>>())
            .Returns(call =>
                orders.AsQueryable()
                      .Where(call.Arg<Expression<Func<SalesOrderHeader, bool>>>())
                      .ToList());

        salesOrderDetails
            .UpdateWithoutSave(Arg.Any<SalesOrderDetail>())
            .Returns(call =>
            {
                updatedDetailsWithoutSave.Add(call.Arg<SalesOrderDetail>());
                return true;
            });

        salesOrderHeaders
            .UpdateWithoutSave(Arg.Any<SalesOrderHeader>())
            .Returns(call =>
            {
                updatedHeadersWithoutSave.Add(call.Arg<SalesOrderHeader>());
                return true;
            });

        // Lifecycle: GetByName retorna el lifecycle amb els estats
        var lifecycles = Substitute.For<ILifecycleRepository>();
        lifecycles
            .GetByName(StatusConstants.Lifecycles.SalesOrder)
            .Returns(Task.FromResult<Lifecycle?>(lifecycle));

        unitOfWork.SalesOrderHeaders.Returns(salesOrderHeaders);
        unitOfWork.SalesOrderDetails.Returns(salesOrderDetails);
        unitOfWork.Lifecycles.Returns(lifecycles);
        unitOfWork.CompleteAsync().Returns(Task.FromResult(0));

        var localization = Substitute.For<ILocalizationService>();
        localization.GetLocalizedString(Arg.Any<string>(), Arg.Any<object[]>()).Returns(x => (string)x[0]);
        localization.GetLocalizedString(Arg.Any<string>()).Returns(x => (string)x[0]);

        var service = new SalesOrderService(
            unitOfWork,
            Substitute.For<IEnterpriseService>(),
            Substitute.For<IExerciseService>(),
            Substitute.For<IBudgetService>(),
            localization,
            Substitute.For<Microsoft.Extensions.Logging.ILogger<SalesOrderService>>());

        return new TestContext(service, unitOfWork, salesOrderHeaders, salesOrderDetails, deliveryNoteId,
            comanda, comandaServida, updatedDetailsWithoutSave, updatedHeadersWithoutSave);
    }
}
