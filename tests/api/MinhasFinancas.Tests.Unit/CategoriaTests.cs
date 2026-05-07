using Xunit;
using MinhasFinancas.Domain.Entities;
using MinhasFinancas.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace MinhasFinancas.Tests.Unit;

#pragma warning disable CS1591
#pragma warning disable CA1707
#pragma warning disable CS8602

public sealed class CategoriaTests : IDisposable
{
    /*
    * Valida a regra de Cadastro de Categoria e sua persistência no banco de dados.
    */
    [Fact]
    public async Task ValidaRegraCategoriaCadastroEPersistencia()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<MinhasFinancasDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new MinhasFinancasDbContext(options);
        
        var novaCategoria = new Categoria 
        { 
            Descricao = "Educação", 
            Finalidade = Categoria.EFinalidade.Despesa 
        };

        // Act
        context.Categorias.Add(novaCategoria);
        await context.SaveChangesAsync();

        // Assert
        var categoriaNoBanco = await context.Categorias.FirstOrDefaultAsync(c => c.Descricao == "Educação");
        
        Assert.NotNull(categoriaNoBanco);
        Assert.Equal(Categoria.EFinalidade.Despesa, categoriaNoBanco.Finalidade);
        Assert.NotEqual(Guid.Empty, categoriaNoBanco.Id); // Garante que o EF gerou o ID corretamente
    }

    public void Dispose()
    {
    }
}
