using Xunit;
using MinhasFinancas.Infrastructure.Queries;
using MinhasFinancas.Domain.Entities;
using MinhasFinancas.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MinhasFinancas.Tests.Unit;

#pragma warning disable CS1591
#pragma warning disable CA1707

public class TotaisQueryTests
{
    /*
    * Este teste evidencia que há falta de tratamento para nulo em TotaisQuery
    */
    [Fact(Skip = "Comportamento identificado: Retorno de dados fora do período esperado quando filtro não é informado")]
    public async Task EvidenciaFalhaFiltroNuloRetornaTodoHistorico()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<MinhasFinancasDbContext>()
            .UseInMemoryDatabase(databaseName: "TotaisBugDb")
            .Options;

        using var context = new MinhasFinancasDbContext(options);
        var cache = new MemoryCache(new MemoryCacheOptions());
        var query = new TotaisQuery(context, cache);

        var pessoa = new Pessoa { Nome = "João" };
        context.Pessoas.Add(pessoa);

        var t1 = new Transacao { Valor = 100, Tipo = Transacao.ETipo.Receita, Data = DateTime.Today };
        // Uso de reflection para simular atribuição interna controlada pelo domínio
        typeof(Transacao).GetProperty("PessoaId")?.SetValue(t1, pessoa.Id);
        context.Transacoes.Add(t1);

        // Transação de 5 anos atrás (deveria ser ignorada num dashboard mensal normal)
        var t2 = new Transacao { Valor = 50, Tipo = Transacao.ETipo.Receita, Data = DateTime.Today.AddYears(-5) };
        // Uso de reflection para simular atribuição interna controlada pelo domínio
        typeof(Transacao).GetProperty("PessoaId")?.SetValue(t2, pessoa.Id);
        context.Transacoes.Add(t2);

        await context.SaveChangesAsync();

        // Act
        // Passamos NULL no filtro, o que dispara o aviso CS8603
        var result = await query.GetTotaisPorPessoaAsync(null);

        // Assert
        var totalJoao = result.Items.First().TotalReceitas;

        // EVIDÊNCIA: O total é 150 (soma tudo), provando que não há um filtro padrão seguro.
        // Se o sistema fosse seguro, ou daria erro ou filtraria o mês atual por padrão.
        Assert.Equal(150m, totalJoao);

        // Limpa Cache
        cache.Dispose();
    }
}
