using Core.Entity;
using Core.Input;
using Core.Repository;
using Core.Services;
using FluentAssertions;
using Infrastructure.Services;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System.Text;
using System.Text.Json;

namespace Tests;

public class GameServiceTests
{
    private readonly Mock<IGameRepository> _repoMock = new();
    private readonly Mock<IDistributedCache> _cacheMock = new();
    private readonly Mock<ISearchService> _searchMock = new();
    private readonly Mock<IAuditLogRepository> _auditMock = new();

    private GameService CreateService() =>
        new(_repoMock.Object, _cacheMock.Object, _searchMock.Object, _auditMock.Object,
            NullLogger<GameService>.Instance);

    private static byte[] Serialize<T>(T value) =>
        Encoding.UTF8.GetBytes(JsonSerializer.Serialize(value));

    [Fact]
    public void ObterTodosDto_CacheHit_NaoConsultaRepositorio()
    {
        var cached = new List<GameDto> { new() { Id = 1, Title = "Zelda", Price = 99m } };
        _cacheMock
            .Setup(c => c.Get(It.IsAny<string>()))
            .Returns(Serialize(cached));

        var result = CreateService().ObterTodosDto();

        result.Should().HaveCount(1);
        result[0].Title.Should().Be("Zelda");
        _repoMock.Verify(r => r.ObterTodos(), Times.Never);
    }

    [Fact]
    public void ObterTodosDto_CacheMiss_ConsultaRepositorioEPopulaCache()
    {
        _cacheMock.Setup(c => c.Get(It.IsAny<string>())).Returns((byte[]?)null);
        _repoMock.Setup(r => r.ObterTodos()).Returns(new List<Game>
        {
            new() { Id = 2, Title = "Mario", Price = 59m }
        });

        var result = CreateService().ObterTodosDto();

        result.Should().HaveCount(1);
        result[0].Title.Should().Be("Mario");
        _repoMock.Verify(r => r.ObterTodos(), Times.Once);
        _cacheMock.Verify(c => c.Set(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<DistributedCacheEntryOptions>()), Times.Once);
    }

    [Fact]
    public void Cadastrar_InvalidaCache_IndexaEAuditaJogo()
    {
        var game = new Game { Id = 3, Title = "Metroid", Price = 79m };

        CreateService().Cadastrar(game);

        _cacheMock.Verify(c => c.Remove(It.IsAny<string>()), Times.Once);
        _searchMock.Verify(s => s.IndexGameAsync(game), Times.Once);
        _auditMock.Verify(a => a.AddAsync(It.Is<AuditLog>(l => l.Action == "Created" && l.EntityName == "Game")), Times.Once);
    }

    [Fact]
    public void Alterar_InvalidaCache_ReindexaJogo()
    {
        var game = new Game { Id = 4, Title = "Kirby", Price = 49m };

        CreateService().Alterar(game);

        _cacheMock.Verify(c => c.Remove(It.IsAny<string>()), Times.Once);
        _searchMock.Verify(s => s.IndexGameAsync(game), Times.Once);
        _auditMock.Verify(a => a.AddAsync(It.Is<AuditLog>(l => l.Action == "Updated")), Times.Once);
    }

    [Fact]
    public void Deletar_InvalidaCache_RemoveDoIndice()
    {
        CreateService().Deletar(5);

        _cacheMock.Verify(c => c.Remove(It.IsAny<string>()), Times.Once);
        _searchMock.Verify(s => s.RemoveGameAsync(5), Times.Once);
        _auditMock.Verify(a => a.AddAsync(It.Is<AuditLog>(l => l.Action == "Deleted" && l.EntityId == "5")), Times.Once);
    }
}
