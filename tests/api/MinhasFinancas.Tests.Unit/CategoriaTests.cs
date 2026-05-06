using Xunit;
using MinhasFinancas.Domain.Entities;
using MinhasFinancas.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace MinhasFinancas.Tests.Unit;

#pragma warning disable CS1591
#pragma warning disable CA1707
#pragma warning disable CS8602

public sealed class CategoriaTests : IDisposable
{
    /*
    * Este teste evidencia a presença de colunas "fantasmas" criadas pelo EF Core devido a falha de mapeamento.
    */
    [Fact(Skip = "Comportamento identificado: Shadow Properties indicam possível falha de mapeamento no EF Core.")]
    public void Evidencia_Fantasmas_No_Banco_De_Dados()
    {
        var options = new DbContextOptionsBuilder<MinhasFinancasDbContext>()
            .UseInMemoryDatabase(databaseName: "BugEvidenceDb")
            .Options;
        using var context = new MinhasFinancasDbContext(options);
        // Busca propriedades que existem no banco mas NÃO na classe C#
        var shadowProperties = context.Model.FindEntityType(typeof(Transacao))
            .GetProperties()
            .Where(p => p.IsShadowProperty())
            .Select(p => p.Name)
            .ToList();
        // Evidencia que existem shadow properties
        Assert.NotEmpty(shadowProperties);
    }

    public void Dispose()
    {
    }
}
