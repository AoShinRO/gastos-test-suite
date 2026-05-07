using Xunit;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using MinhasFinancas.Domain.Entities;
using MinhasFinancas.Infrastructure.Data;

namespace MinhasFinancas.Tests.Unit;

#pragma warning disable CS1591
#pragma warning disable CA1707

public sealed class TransacaoTests : IDisposable
{
    /*
    * Evidencia a inconsistência entre o atributo [Range] que usa double.MaxValue 
    * e a propriedade que é decimal (limites incompatíveis).
    */
    [Fact(Skip = "Comportamento identificado: Atributo [Range] inconsistente com tipo decimal, indicando inconsistência entre validação e tipo numérico utilizado")]
    public void EvidenciaFalhaRangeInconsistenteNoDominio()
    {
        // 1e30 é um valor que cabe em um 'double'
        // mas é muito maior que o 'decimal.MaxValue' (aprox 7.9e28).
        double valorIncompativel = 1e30; 
        var transacao = new Transacao();
        var contexto = new ValidationContext(transacao) { MemberName = nameof(Transacao.Valor) };
        var resultados = new List<ValidationResult>();

        // Usamos TryValidateProperty para validar os atributos da propriedade 'Valor'
        bool aprovadoPeloAtributo = Validator.TryValidateProperty(
            valorIncompativel, 
            contexto, 
            resultados
        );
        
        Assert.True(aprovadoPeloAtributo, "Bug Confirmado: O Atributo [Range] deveria bloquear valores maiores que o limite acima do tipo decimal.");
    }

    /*
    * Evidencia que o domínio aceita valores negativos que o front-end tenta proibir 
    * e comentário na definição de Transacao.Valor afirma que não aceitaria.
    */
    [Fact(Skip = "Comportamento identificado: O domínio aceita valores negativos, contrariando regra de negócio e comentário no código.")]
    public void EvidenciaFalhaValorNegativoNoDominio()
    {
        var transacao = new Transacao { Descricao = "Valor Negativo" };
        transacao.Valor = -100.00m;
        Assert.True(transacao.Valor < 0, "O sistema não deveria aceitar valores negativos no domínio.");
    }

    /*
    * Evidencia que o sistema recebe dados inválidos e não realiza validação.
    */
    [Theory(Skip = "Comportamento identificado: Os Domínios recebem dados inválidos e não realizam validação")]
    [InlineData("<script>alert(1)</script>", Transacao.ETipo.Despesa, Categoria.EFinalidade.Despesa)]
    [InlineData("", Transacao.ETipo.Despesa, Categoria.EFinalidade.Despesa)]    
    public void EvidenciaFalhaInconsistenciaDadosInvalidos(string stringMaliciosa, Transacao.ETipo tipoTransacao, Categoria.EFinalidade finalidade)
    {
        // Arrange
        // Evidencia que os três domínios, Pessoa, Categoria e Transacao,
        // recebem dados inválidos e não realiza validação.
        var pessoa = new Pessoa
        {
            Nome = stringMaliciosa,
            DataNascimento = DateTime.Today.AddYears(99)
        };

        var categoria = new Categoria
        {
            Descricao = stringMaliciosa,
            Finalidade = finalidade
        };

        var transacao = new Transacao
        {
            Descricao = stringMaliciosa,
            Valor = 1.0m,
            Tipo = tipoTransacao
        };

        // Act & Assert
        // Uso de reflection para simular atribuição interna controlada pelo domínio
        var pessoaProp = typeof(Transacao).GetProperty("Pessoa");
        var categoriaProp = typeof(Transacao).GetProperty("Categoria");

        pessoaProp?.SetValue(transacao, pessoa);
        categoriaProp?.SetValue(transacao, categoria);

        Assert.Equal(pessoa.Id, transacao.PessoaId);
        Assert.Equal(1.0m, transacao.Valor);
    }

    /*
    * Valida se o sistema permite ou bloqueia transações para pessoas que fazem 18 anos amanhã, previne regressão de regra de negócio.
    */
    [Theory]
    [InlineData("Despesa", Transacao.ETipo.Despesa, Categoria.EFinalidade.Despesa)]
    [InlineData("Receita", Transacao.ETipo.Receita, Categoria.EFinalidade.Receita)]
    public void ValidaRegraTransacaoProibidoParaMenorQuaseAdulto(string desc, Transacao.ETipo tipoTransacao, Categoria.EFinalidade finalidade)
    {
        // Arrange
        var pessoa = new Pessoa
        {
            Nome = "Jovem",
            DataNascimento = DateTime.Today.AddYears(-18).AddDays(1)
        };

        var categoria = new Categoria
        {
            Descricao = "Categoria de Teste",
            Finalidade = finalidade
        };

        var transacao = new Transacao
        {
            Descricao = desc,
            Valor = 1.0m,
            Tipo = tipoTransacao
        };

        // Act & Assert
        // Uso de reflection para simular atribuição interna controlada pelo domínio
        var pessoaProp = typeof(Transacao).GetProperty("Pessoa");
        var categoriaProp = typeof(Transacao).GetProperty("Categoria");

        // Valida regra do negócio: Menor de idade pode ter receita? Não!
        if (tipoTransacao == Transacao.ETipo.Receita)
        {
            var ex = Assert.Throws<System.Reflection.TargetInvocationException>(() =>
                pessoaProp?.SetValue(transacao, pessoa));
            
            Assert.IsType<InvalidOperationException>(ex.InnerException);
            Assert.Equal("Menores de 18 anos não podem registrar receitas.", ex.InnerException.Message);
        }
        else
        {
            // Valida regra do negócio: Menor de idade pode ter despesa? Sim!
            pessoaProp?.SetValue(transacao, pessoa);
            categoriaProp?.SetValue(transacao, categoria);

            Assert.Equal(pessoa.Id, transacao.PessoaId);
            Assert.Equal(1.0m, transacao.Valor);
        }
    }

    private MinhasFinancasDbContext? _context;

    /*
    * Valida exclusão em cascata de transações ao excluir pessoa
    */
    [Fact]
    public async Task ValidaRegraExclusaoEmCascataLimpaTransacoes()
    {

        var options = new DbContextOptionsBuilder<MinhasFinancasDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new MinhasFinancasDbContext(options);

        // Arrange
        var pessoa = new Pessoa { Nome = "Pessoa Teste", DataNascimento = new DateTime(1990, 1, 1) };
        var categoria = new Categoria { Descricao = "Lanches", Finalidade = Categoria.EFinalidade.Despesa };
        _context.Pessoas.Add(pessoa);
        _context.Categorias.Add(categoria);
        await _context.SaveChangesAsync();

        var transacao = new Transacao { Tipo = Transacao.ETipo.Despesa, Valor = 50, Data = DateTime.Today };
        // Uso de reflection para simular atribuição interna controlada pelo domínio
        typeof(Transacao).GetProperty("Pessoa")!.SetValue(transacao, pessoa);
        typeof(Transacao).GetProperty("Categoria")!.SetValue(transacao, categoria);
        
        _context.Transacoes.Add(transacao);
        await _context.SaveChangesAsync();

        // Garantir que existe antes de deletar
        Assert.Equal(1, await _context.Transacoes.CountAsync());

        // Act
        _context.Pessoas.Remove(pessoa);
        await _context.SaveChangesAsync();

        // Assert
        var transacoesRestantes = await _context.Transacoes.CountAsync();
        Assert.Equal(0, transacoesRestantes);

        await _context.Database.EnsureDeletedAsync();
        await _context.DisposeAsync();
    }

    /*
    * Valida se a categoria só pode ser usada conforme sua finalidade (receita/despesa/ambas)
    */
    [Theory]
    [InlineData(Transacao.ETipo.Despesa, Categoria.EFinalidade.Receita, false)]
    [InlineData(Transacao.ETipo.Receita, Categoria.EFinalidade.Despesa, false)]
    [InlineData(Transacao.ETipo.Despesa, Categoria.EFinalidade.Despesa, true)]
    [InlineData(Transacao.ETipo.Receita, Categoria.EFinalidade.Receita, true)]
    [InlineData(Transacao.ETipo.Despesa, Categoria.EFinalidade.Ambas, true)]
    [InlineData(Transacao.ETipo.Receita, Categoria.EFinalidade.Ambas, true)]
    public void ValidaRegraCategoriaRespeitaFinalidade(Transacao.ETipo tipoTransacao, Categoria.EFinalidade finalidadeCategoria, bool deveFuncionar)
    {
        // Arrange
        var categoria = new Categoria { Finalidade = finalidadeCategoria };
        var transacao = new Transacao { Tipo = tipoTransacao };

        // Act & Assert
        // Uso de reflection para simular atribuição interna controlada pelo domínio
        var prop = typeof(Transacao).GetProperty("Categoria")!;

        if (deveFuncionar)
        {
            prop.SetValue(transacao, categoria);
            Assert.Equal(categoria.Id, transacao.CategoriaId);
        }
        else
        {
            var ex = Assert.Throws<System.Reflection.TargetInvocationException>(() => prop.SetValue(transacao, categoria));
            Assert.IsType<InvalidOperationException>(ex.InnerException);
        }
    }

    /*
    * Valida comportamento esperado de precisão decimal em operações financeiras.
    * Este teste garante que o uso de decimal mantém exatidão em somas sucessivas,
    * evitando problemas comuns de precisão binária (float/double).
    * Não evidencia falha atual, mas atua como proteção contra regressão.
    */
    [Fact]
    public void ValidaRegraSomaCentavosExatos()
    {
        // Cenário: 10 transações de 0,10 centavos.
        decimal soma = 0m;
        decimal valorUnitario = 0.1m;

        for (int i = 0; i < 10; i++)
        {
            var t = new Transacao { Valor = valorUnitario };
            soma += t.Valor;
        }

        Assert.Equal(1.0m, soma);
    }

    public void Dispose()
    {
        _context?.Dispose();
    }
}
