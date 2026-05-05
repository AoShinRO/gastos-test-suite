using MinhasFinancas.Domain.Entities;
using Xunit;

namespace MinhasFinancas.Tests.Unit;

#pragma warning disable CS1591
#pragma warning disable CA1707

/// <summary>
/// Testes unitários para a entidade Categoria.
/// </summary>
public sealed class CategoriaTests : IDisposable
{
    /// <summary>
    /// Prova que o sistema permite descrições inválidas (vazias ou com SQL Injection) na Categoria.
    /// </summary>
    [Theory]
    [InlineData("")]
    [InlineData("'; DROP TABLE Categorias; --")]
    public void ValidarSanitizacaoDescricaoCategoria(string descricaoInvalida)
    {
        // Arrange
        var categoria = new Categoria();

        // Act
        categoria.Descricao = descricaoInvalida;

        // Assert
        // O teste documenta que o domínio aceita strings vazias ou maliciosas sem sanitização
        Assert.Equal(descricaoInvalida, categoria.Descricao);
    }

    public void Dispose()
    {
    }
}
