using System.Data.SqlTypes;
using MinhasFinancas.Domain.ValueObjects;
using Xunit;

namespace MinhasFinancas.Tests.Unit;

/// <summary>
/// Prova o bug de arredondamento de data ao usar .AddTicks(-1) com SQL Server.
/// </summary>
public sealed class DataFilterTests : IDisposable
{
    /// <summary>
    /// Este teste prova que o uso de .AddTicks(-1) cria uma data tão próxima do próximo mês
    /// que o SQL Server (simulado por SqlDateTime) a arredonda para cima, 
    /// causando inclusão indevida de registros do mês seguinte.
    /// </summary>
    [Fact]
    public void ProveRoundingBugSemBancoDeDados()
    {
        // Arrange: Filtro para Maio de 2026
        var filter = new DataFilter { Mes = 5, Ano = 2026 };

        // Act: Normaliza o filtro (gera DataFim usando AddTicks(-1))
        var normalized = filter.Normalize();
        DateTime dataFimOriginal = normalized.DataFim!.Value;

        // Simulando a precisão do tipo 'datetime' do SQL Server (3.33ms)
        SqlDateTime sqlDateTime = new SqlDateTime(dataFimOriginal);
        DateTime dataArredondadaPeloSql = sqlDateTime.Value;

        // Assert
        // A data original deve ser o último milissegundo de Maio
        Assert.Equal(5, dataFimOriginal.Month);
        Assert.Equal(31, dataFimOriginal.Day);
        Assert.Equal(23, dataFimOriginal.Hour);

        // PROVA DO ERRO: 
        // O SQL Server arredonda 23:59:59.9999999 para 00:00:00.000 do dia seguinte.
        Assert.Equal(6, dataArredondadaPeloSql.Month);
        Assert.Equal(1, dataArredondadaPeloSql.Day);
        Assert.Equal(0, dataArredondadaPeloSql.Hour);
    }

    public void Dispose()
    {
    }
}
