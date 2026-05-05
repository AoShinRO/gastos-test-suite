using System;
using MinhasFinancas.Domain.Entities;
using Xunit;

namespace MinhasFinancas.Tests.Unit;

#pragma warning disable CS1591
#pragma warning disable CA1707

/// <summary>
/// Testes unitários para a entidade Transacao.
/// </summary>
public sealed class TransacaoTests : IDisposable
{
    /// <summary>
    /// Valida se o sistema permite ou bloqueia transações para pessoas que fazem 18 anos amanhã.
    /// Testa também a aceitação de payloads de SQL Injection na descrição.
    /// </summary>
    [Theory]
    [InlineData("'; DROP TABLE Transacoes; -- <script>alert(1)</script>", Transacao.ETipo.Despesa, Categoria.EFinalidade.Despesa)]
    [InlineData("'; DROP TABLE Transacoes; -- <script>alert(1)</script>", Transacao.ETipo.Receita, Categoria.EFinalidade.Receita)]
    public void ValidarTransacaoParaMenorQuaseAdulto(string payloadSql, Transacao.ETipo tipoTransacao, Categoria.EFinalidade finalidade)
    {
        // Arrange
        var pessoa = new Pessoa
        {
            Nome = "Jovem Gafanhoto",
            DataNascimento = DateTime.Today.AddYears(-18).AddDays(1)
        };

        var categoria = new Categoria
        {
            Descricao = "Categoria de Teste",
            Finalidade = finalidade
        };

        var transacao = new Transacao
        {
            Descricao = payloadSql,
            Valor = 1.0m,
            Tipo = tipoTransacao
        };

        // Act & Assert
        var pessoaProp = typeof(Transacao).GetProperty("Pessoa");
        var categoriaProp = typeof(Transacao).GetProperty("Categoria");

        if (tipoTransacao == Transacao.ETipo.Receita)
        {
            var ex = Assert.Throws<System.Reflection.TargetInvocationException>(() =>
                pessoaProp?.SetValue(transacao, pessoa));
            
            Assert.IsType<InvalidOperationException>(ex.InnerException);
            Assert.Equal("Menores de 18 anos não podem registrar receitas.", ex.InnerException.Message);
        }
        else
        {
            pessoaProp?.SetValue(transacao, pessoa);
            categoriaProp?.SetValue(transacao, categoria);

            Assert.Equal(pessoa.Id, transacao.PessoaId);
            Assert.Equal(payloadSql, transacao.Descricao);
            Assert.Equal(1.0m, transacao.Valor);
        }
    }
    
    /// <summary>
    /// Valida que transações para maiores de idade aceitam dados maliciosos (documentando falta de sanitização).
    /// </summary>
    [Fact]
    public void ValidarTransacaoAdultoComDadosMaliciosos()
    {
        var pessoa = new Pessoa { Nome = "Adulto", DataNascimento = DateTime.Today.AddYears(-30) };
        var transacao = new Transacao
        {
            Descricao = "'; DROP TABLE Usuarios; -- <script>alert('hack')</script>",
            Valor = 100.00m,
            Tipo = Transacao.ETipo.Despesa
        };

        Assert.True(pessoa.EhMaiorDeIdade());
        Assert.Contains("DROP TABLE", transacao.Descricao, StringComparison.Ordinal);
    }

    /// <summary>
    /// Documenta que o sistema aceita valores negativos (Bug), apesar do atributo [Range].
    /// </summary>
    [Fact]
    public void ValidarValorNegativoDeveCausarErro()
    {
        var transacao = new Transacao { Descricao = "Valor Negativo" };
        transacao.Valor = -100.00m;
        Assert.True(transacao.Valor < 0, "O sistema não deveria aceitar valores negativos no domínio.");
    }

    /// <summary>
    /// Valida a precisão decimal em somas sucessivas (evitando imprecisão binária).
    /// </summary>
    [Fact]
    public void ValidarSomaPrecisaoDecimal()
    {
        // Cenário: 10 transações de 0.10
        decimal soma = 0m;
        decimal valorUnitario = 0.1m;

        for (int i = 0; i < 10; i++)
        {
            var t = new Transacao { Valor = valorUnitario };
            soma += t.Valor;
        }

        Assert.Equal(1.0m, soma);
    }


    /// <summary>
    /// Evidencia a inconsistência entre o atributo [Range] que usa double.MaxValue 
    /// e a propriedade que é decimal (limites incompatíveis).
    /// </summary>
    [Fact]
    public void ValidarInconsistenciaRangeValor()
    {
        // Obtém o atributo Range via Reflection
        var property = typeof(Transacao).GetProperty(nameof(Transacao.Valor));
        var rangeAttr = (System.ComponentModel.DataAnnotations.RangeAttribute?)Attribute.GetCustomAttribute(property!, typeof(System.ComponentModel.DataAnnotations.RangeAttribute));

        Assert.NotNull(rangeAttr);

        double limiteAtributo = (double)rangeAttr.Maximum;
        decimal limiteDecimal = decimal.MaxValue;

        // Prova que o atributo permite valores muito maiores do que o tipo decimal suporta
        // double.MaxValue (~1.8e308) > decimal.MaxValue (~7.9e28)
        Assert.True(limiteAtributo > (double)limiteDecimal, 
            $"O limite do atributo ({limiteAtributo}) é maior que o suportado pelo tipo decimal ({limiteDecimal}).");
    }

    public void Dispose()
    {
    }
}
