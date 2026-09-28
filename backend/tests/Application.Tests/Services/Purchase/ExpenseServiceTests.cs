using Application.Contracts;
using Application.Services.Purchase;
using Application.Tests.TestSupport;
using Domain.Entities.Purchase;
using NSubstitute;
using System.Linq.Expressions;
using Xunit;

namespace Application.Tests.Services.Purchase;

/// <summary>
/// Unit tests for recurring expenses in <see cref="ExpenseService"/> (#159):
/// editing an expense of a series must keep it and the earlier instances, dates
/// must not drift, and a series never goes past its end date.
/// </summary>
public class ExpenseServiceTests
{
    private static readonly Guid ExpenseTypeId = Guid.NewGuid();

    // -------- Create --------

    [Fact]
    public async Task Create_generates_monthly_instances_up_to_the_end_date()
    {
        var context = BuildSut();
        var first = Recurring(new DateTime(2026, 1, 1), paymentDay: 1, endDate: new DateTime(2026, 4, 1));

        await context.Sut.Create(first);

        Assert.Equal(
            [new DateTime(2026, 1, 1), new DateTime(2026, 2, 1), new DateTime(2026, 3, 1), new DateTime(2026, 4, 1)],
            context.Store.Select(e => e.PaymentDate).OrderBy(d => d));
        Assert.All(Instances(context, first), e => Assert.Equal(first.Id.ToString(), e.RelatedExpenseId));
    }

    [Fact]
    public async Task Create_keeps_the_payment_day_without_drifting()
    {
        var context = BuildSut();
        var first = Recurring(new DateTime(2026, 1, 15), paymentDay: 5, endDate: new DateTime(2026, 5, 31));

        await context.Sut.Create(first);

        Assert.Equal(
            [new DateTime(2026, 2, 5), new DateTime(2026, 3, 5), new DateTime(2026, 4, 5), new DateTime(2026, 5, 5)],
            Instances(context, first).Select(e => e.PaymentDate).OrderBy(d => d));
    }

    [Fact]
    public async Task Create_clamps_the_payment_day_to_the_length_of_the_month()
    {
        var context = BuildSut();
        var first = Recurring(new DateTime(2026, 1, 31), paymentDay: 31, endDate: new DateTime(2026, 3, 31));

        await context.Sut.Create(first);

        Assert.Equal(
            [new DateTime(2026, 2, 28), new DateTime(2026, 3, 31)],
            Instances(context, first).Select(e => e.PaymentDate).OrderBy(d => d));
    }

    [Fact]
    public async Task Create_without_frequency_generates_no_instances()
    {
        var context = BuildSut();
        var first = Recurring(new DateTime(2026, 1, 1), paymentDay: 1, endDate: new DateTime(2026, 12, 31), frequency: 0);

        await context.Sut.Create(first);

        Assert.Equal(first, Assert.Single(context.Store));
    }

    // -------- Update --------

    [Fact]
    public async Task Update_of_the_first_expense_keeps_it_and_regenerates_the_following_ones()
    {
        var context = BuildSut();
        var first = Recurring(new DateTime(2026, 1, 1), paymentDay: 1, endDate: new DateTime(2026, 4, 1));
        await context.Sut.Create(first);

        var edited = Copy(first);
        edited.Amount = 150;
        var response = await context.Sut.UpdateExpense(edited);

        Assert.True(response.Result);
        Assert.Equal(150, Assert.Single(context.Store, e => e.Id == first.Id).Amount);
        Assert.Equal(4, context.Store.Count);
        Assert.All(context.Store, e => Assert.Equal(150, e.Amount));
    }

    [Fact]
    public async Task Update_of_an_instance_keeps_the_earlier_ones_unchanged()
    {
        var context = BuildSut();
        var first = Recurring(new DateTime(2026, 1, 1), paymentDay: 1, endDate: new DateTime(2026, 4, 1));
        await context.Sut.Create(first);
        var march = Assert.Single(context.Store, e => e.PaymentDate == new DateTime(2026, 3, 1));

        var edited = Copy(march);
        edited.Amount = 200;
        await context.Sut.UpdateExpense(edited);

        var byDate = context.Store.ToDictionary(e => e.PaymentDate);
        Assert.Equal(4, byDate.Count);
        Assert.Equal(100, byDate[new DateTime(2026, 1, 1)].Amount);
        Assert.Equal(100, byDate[new DateTime(2026, 2, 1)].Amount);
        Assert.Equal(march.Id, byDate[new DateTime(2026, 3, 1)].Id);
        Assert.Equal(200, byDate[new DateTime(2026, 3, 1)].Amount);
        Assert.Equal(200, byDate[new DateTime(2026, 4, 1)].Amount);
        Assert.Equal(first.Id.ToString(), byDate[new DateTime(2026, 4, 1)].RelatedExpenseId);
    }

    // -------- helpers --------

    private static Expenses Recurring(DateTime paymentDate, int paymentDay, DateTime endDate, int frequency = 1) => new()
    {
        Id = Guid.NewGuid(),
        Description = "Rent",
        CreationDate = paymentDate,
        PaymentDate = paymentDate,
        EndDate = endDate,
        Amount = 100,
        Recurring = true,
        Frecuency = frequency,
        PaymentDay = paymentDay,
        ExpenseTypeId = ExpenseTypeId,
    };

    // The API receives a detached copy of the expense, not the tracked instance.
    private static Expenses Copy(Expenses source) => new()
    {
        Id = source.Id,
        Description = source.Description,
        CreationDate = source.CreationDate,
        PaymentDate = source.PaymentDate,
        EndDate = source.EndDate,
        Amount = source.Amount,
        Recurring = source.Recurring,
        Frecuency = source.Frecuency,
        PaymentDay = source.PaymentDay,
        RelatedExpenseId = source.RelatedExpenseId,
        ExpenseTypeId = source.ExpenseTypeId,
    };

    private static IEnumerable<Expenses> Instances(TestContext context, Expenses first) =>
        context.Store.Where(e => e.Id != first.Id);

    private static TestContext BuildSut()
    {
        var store = new List<Expenses>();
        var repository = Substitute.For<IExpenseRepository>();
        repository
            .Find(Arg.Any<Expression<Func<Expenses, bool>>>())
            .Returns(call => store.AsQueryable().Where(call.Arg<Expression<Func<Expenses, bool>>>()).ToList());
        repository
            .Exists(Arg.Any<Guid>())
            .Returns(call => store.Any(e => e.Id == call.Arg<Guid>()));
        repository
            .Add(Arg.Any<Expenses>())
            .Returns(call =>
            {
                store.Add(call.Arg<Expenses>());
                return Task.CompletedTask;
            });
        repository
            .Update(Arg.Any<Expenses>())
            .Returns(call =>
            {
                var expense = call.Arg<Expenses>();
                store.RemoveAll(e => e.Id == expense.Id);
                store.Add(expense);
                return Task.CompletedTask;
            });
        repository
            .Remove(Arg.Any<Expenses>())
            .Returns(call =>
            {
                store.RemoveAll(e => e.Id == call.Arg<Expenses>().Id);
                return Task.CompletedTask;
            });
        repository
            .RemoveRange(Arg.Any<IEnumerable<Expenses>>())
            .Returns(call =>
            {
                var ids = call.Arg<IEnumerable<Expenses>>().Select(e => e.Id).ToHashSet();
                store.RemoveAll(e => ids.Contains(e.Id));
                return Task.CompletedTask;
            });

        var unitOfWork = Substitute.For<IUnitOfWork>();
        unitOfWork.Expenses.Returns(repository);

        var sut = new ExpenseService(unitOfWork, new NullLocalizationService());
        return new TestContext(sut, store);
    }

    private sealed record TestContext(ExpenseService Sut, List<Expenses> Store);
}
