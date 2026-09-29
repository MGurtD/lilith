using Application.Contracts;
using Application.Services.Shared;
using Application.Services.System;
using Application.Tests.TestSupport;
using Domain.Entities;
using Domain.Entities.Shared;
using NSubstitute;
using Xunit;

namespace Application.Tests.Services.Shared;

/// <summary>
/// Payment methods, exercises and references in use must not be deleted (#159):
/// invoices, delivery notes, purchase orders, work orders, document lines and
/// stock cascade from them.
/// </summary>
public class SharedMasterDeleteGuardTests
{
    private static readonly Dictionary<string, string> LocalizationKeys = new()
    {
        ["PaymentMethodInUse"] = "La forma de pagament {0} està en ús",
        ["ExerciseInUse"] = "L'exercici {0} està en ús",
        ["ExerciseNotFound"] = "L'exercici no existeix",
        ["EntityNotFound"] = "L'entitat amb ID {0} no existeix",
        ["Reference.Delete.Header"] = "Referència amb dependències:",
        ["Reference.Delete.SalesOrders"] = "- Té comandes de venda",
        ["Reference.Delete.PurchaseOrders"] = "- Té comandes de compra",
        ["Reference.Delete.Stock"] = "- Té estoc o lots",
    };

    private static KeyedLocalizationService Localization() => new(LocalizationKeys);

    [Fact]
    public async Task RemovePaymentMethod_refuses_a_payment_method_in_use()
    {
        var paymentMethod = new PaymentMethod { Name = "Transferència" };
        var (sut, repository) = BuildPaymentMethodSut(paymentMethod, inUse: true);

        var response = await sut.RemovePaymentMethod(paymentMethod.Id);

        Assert.False(response.Result);
        Assert.Equal("La forma de pagament Transferència està en ús", Assert.Single(response.Errors));
        await repository.DidNotReceive().Remove(Arg.Any<PaymentMethod>());
    }

    [Fact]
    public async Task RemovePaymentMethod_deletes_an_unused_payment_method()
    {
        var paymentMethod = new PaymentMethod { Name = "Transferència" };
        var (sut, repository) = BuildPaymentMethodSut(paymentMethod, inUse: false);

        var response = await sut.RemovePaymentMethod(paymentMethod.Id);

        Assert.True(response.Result);
        await repository.Received(1).Remove(paymentMethod);
    }

    [Fact]
    public async Task RemoveExercise_refuses_an_exercise_with_documents()
    {
        var exercise = new Exercise { Name = "2026" };
        var (sut, repository) = BuildExerciseSut(exercise, inUse: true);

        var response = await sut.Remove(exercise.Id);

        Assert.False(response.Result);
        Assert.Equal("L'exercici 2026 està en ús", Assert.Single(response.Errors));
        await repository.DidNotReceive().Remove(Arg.Any<Exercise>());
    }

    [Fact]
    public async Task RemoveExercise_deletes_an_unused_exercise()
    {
        var exercise = new Exercise { Name = "2026" };
        var (sut, repository) = BuildExerciseSut(exercise, inUse: false);

        var response = await sut.Remove(exercise.Id);

        Assert.True(response.Result);
        await repository.Received(1).Remove(exercise);
    }

    [Fact]
    public async Task CanDelete_lists_every_use_of_the_reference()
    {
        var reference = new Reference { Code = "REF-1" };
        var (sut, _) = BuildReferenceSut(reference, ReferenceUsage.SalesOrders | ReferenceUsage.Stock);

        var response = await sut.CanDelete(reference.Id);

        Assert.False(response.Result);
        var lines = Assert.Single(response.Errors)
            .Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
        Assert.Equal(["Referència amb dependències:", "- Té comandes de venda", "- Té estoc o lots"], lines);
    }

    [Fact]
    public async Task RemoveReference_refuses_a_reference_in_use()
    {
        var reference = new Reference { Code = "REF-1" };
        var (sut, repository) = BuildReferenceSut(reference, ReferenceUsage.PurchaseOrders);

        var response = await sut.RemoveReference(reference.Id);

        Assert.False(response.Result);
        Assert.Contains("- Té comandes de compra", Assert.Single(response.Errors));
        await repository.DidNotReceive().Remove(Arg.Any<Reference>());
    }

    [Fact]
    public async Task RemoveReference_deletes_an_unused_reference()
    {
        var reference = new Reference { Code = "REF-1" };
        var (sut, repository) = BuildReferenceSut(reference, ReferenceUsage.None);

        var response = await sut.RemoveReference(reference.Id);

        Assert.True(response.Result);
        await repository.Received(1).Remove(reference);
    }

    private static (PaymentMethodService, IPaymentMethodRepository) BuildPaymentMethodSut(PaymentMethod paymentMethod, bool inUse)
    {
        var repository = Substitute.For<IPaymentMethodRepository>().Serving(paymentMethod);
        repository.IsInUse(paymentMethod.Id).Returns(inUse);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        unitOfWork.PaymentMethods.Returns(repository);
        return (new PaymentMethodService(unitOfWork, Localization()), repository);
    }

    private static (ExerciseService, IExerciseRepository) BuildExerciseSut(Exercise exercise, bool inUse)
    {
        var repository = Substitute.For<IExerciseRepository>().Serving(exercise);
        repository.IsInUse(exercise.Id).Returns(inUse);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        unitOfWork.Exercices.Returns(repository);
        return (new ExerciseService(unitOfWork, Localization()), repository);
    }

    private static (ReferenceService, IReferenceRepository) BuildReferenceSut(Reference reference, ReferenceUsage usage)
    {
        var repository = Substitute.For<IReferenceRepository>().Serving(reference);
        repository.GetUsage(reference.Id).Returns(usage);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        unitOfWork.References.Returns(repository);
        return (new ReferenceService(unitOfWork, Localization()), repository);
    }
}
