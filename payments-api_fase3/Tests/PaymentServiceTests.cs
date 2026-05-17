using Core.Entity;
using Core.Input;
using Core.Repository;
using FluentAssertions;
using Infrastructure.Services;
using Moq;

namespace Tests;

public class PaymentServiceTests
{
    private readonly Mock<IPaymentRepository> _repoMock = new();

    private PaymentService CreateService() => new(_repoMock.Object);

    private static PaymentInput ValidInput() => new()
    {
        PurchaseId = 1,
        UserId = 1,
        GameId = 1,
        Amount = 59.90m
    };

    [Fact]
    public async Task ProcessPaymentAsync_InputNulo_LancaExcecao()
    {
        var act = () => CreateService().ProcessPaymentAsync(null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task ProcessPaymentAsync_AmountZero_LancaExcecao()
    {
        var input = ValidInput();
        input.Amount = 0;

        var act = () => CreateService().ProcessPaymentAsync(input);

        await act.Should().ThrowAsync<Exception>()
            .WithMessage("*Amount*");
    }

    [Fact]
    public async Task ProcessPaymentAsync_PurchaseIdInvalido_LancaExcecao()
    {
        var input = ValidInput();
        input.PurchaseId = 0;

        var act = () => CreateService().ProcessPaymentAsync(input);

        await act.Should().ThrowAsync<Exception>()
            .WithMessage("*PurchaseId*");
    }

    [Fact]
    public async Task ProcessPaymentAsync_InputValido_RetornaApprovedOuRejected()
    {
        _repoMock
            .Setup(r => r.Add(It.IsAny<Payment>()))
            .Returns<Payment>(p => p);

        var result = await CreateService().ProcessPaymentAsync(ValidInput());

        result.Should().NotBeNull();
        result.Status.Should().BeOneOf("Approved", "Rejected");
        result.Amount.Should().Be(59.90m);
    }

    [Fact]
    public async Task ProcessPaymentAsync_InputValido_PersisteNoBanco()
    {
        _repoMock
            .Setup(r => r.Add(It.IsAny<Payment>()))
            .Returns<Payment>(p => p);

        await CreateService().ProcessPaymentAsync(ValidInput());

        _repoMock.Verify(r => r.Add(It.IsAny<Payment>()), Times.Once);
    }
}
