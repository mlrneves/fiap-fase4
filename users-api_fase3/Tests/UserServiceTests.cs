using Core.Entity;
using Core.Events;
using Core.Input;
using Core.Repository;
using Core.Services;
using FluentAssertions;
using Infrastructure.CrossCutting.Correlation;
using Infrastructure.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Tests;

public class UserServiceTests
{
    private readonly Mock<IUserRepository> _repoMock = new();
    private readonly Mock<IIntegrationEventPublisher> _publisherMock = new();
    private readonly Mock<ICorrelationIdGenerator> _correlationMock = new();

    private UserService CreateService() =>
        new(_repoMock.Object, _publisherMock.Object, _correlationMock.Object,
            NullLogger<UserService>.Instance);

    private static UserInput ValidInput(string email = "user@test.com") => new()
    {
        Name = "Test User",
        Email = email,
        Password = "Senha123!",
        Role = UserRole.User
    };

    [Fact]
    public async Task CadastrarAsync_EmailInvalido_LancaExcecao()
    {
        var input = ValidInput("nao-e-email");

        var act = () => CreateService().CadastrarAsync(input);

        await act.Should().ThrowAsync<Exception>()
            .WithMessage("*e-mail*");
    }

    [Fact]
    public async Task CadastrarAsync_SenhaFraca_LancaExcecao()
    {
        _repoMock.Setup(r => r.GetByEmail(It.IsAny<string>())).Returns((UserDto?)null);

        var input = ValidInput();
        input.Password = "fraca";

        var act = () => CreateService().CadastrarAsync(input);

        await act.Should().ThrowAsync<Exception>()
            .WithMessage("*Senha*");
    }

    [Fact]
    public async Task CadastrarAsync_EmailDuplicado_LancaExcecao()
    {
        _repoMock
            .Setup(r => r.GetByEmail("existente@test.com"))
            .Returns(new UserDto { Name = "Existente", Email = "existente@test.com", Password = "Senha123!", Role = UserRole.User });

        var input = ValidInput("existente@test.com");

        var act = () => CreateService().CadastrarAsync(input);

        await act.Should().ThrowAsync<Exception>()
            .WithMessage("*cadastrado*");
    }

    [Fact]
    public async Task CadastrarAsync_InputValido_CadastrarEPublicaEvento()
    {
        _repoMock.Setup(r => r.GetByEmail(It.IsAny<string>())).Returns((UserDto?)null);
        _correlationMock.Setup(c => c.Get()).Returns("corr-123");

        var result = await CreateService().CadastrarAsync(ValidInput());

        result.Should().NotBeNull();
        _publisherMock.Verify(p => p.PublishAsync(It.IsAny<IntegrationEvent>()), Times.Once);
    }
}
