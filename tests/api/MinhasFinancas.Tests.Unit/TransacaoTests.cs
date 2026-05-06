using Xunit;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MinhasFinancas.Domain.Entities;
using MinhasFinancas.Infrastructure.Data;

namespace MinhasFinancas.Tests.Unit;

#pragma warning disable CS1591
#pragma warning disable CA1707

public sealed class TransacaoTests : IDisposable
{
    /*
    * Valida se o sistema permite ou bloqueia transações para pessoas que fazem 18 anos amanhã, previne regressão de regra de negócio.
    */
    [Theory]
    [InlineData(Transacao.ETipo.Despesa, Categoria.EFinalidade.Despesa)]
    [InlineData(Transacao.ETipo.Receita, Categoria.EFinalidade.Receita)]
    public void Valida_Regra_Transacao_Proibido_Para_Menor_Quase_Adulto(Transacao.ETipo tipoTransacao, Categoria.EFinalidade finalidade)
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
            Descricao = "Teste",
            Valor = 1.0m,
            Tipo = tipoTransacao
        };

        // Act & Assert
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
    
    /*
    * Valida a precisão decimal em somas sucessivas (evitando imprecisão binária).
    * Isso é importante para evitar erros de arredondamento em transações financeiras,
    * apesar de .NET lidar bem com floats, previne regressão de regra de negócio.
    */
    [Fact]
    public void Valida_Regra_Soma_Centavos_Exatos()
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

    /*
    * Evidencia que o domínio aceita valores que o front-end tenta proibir.
    */
    [Fact]
    public void Evidencia_Valor_Negativo_No_Dominio()
    {
        var transacao = new Transacao { Descricao = "Valor Negativo" };
        transacao.Valor = -100.00m;
        Assert.True(transacao.Valor < 0, "O sistema não deveria aceitar valores negativos no domínio.");
    }

    /*
    * Evidencia a inconsistência entre o atributo [Range] que usa double.MaxValue 
    * e a propriedade que é decimal (limites incompatíveis).
    */
    [Fact]
    public void Evidencia_Range_Inconsistencia_No_Dominio()
    {
        // 1e30 é um valor que cabe em um 'double'
        // mas é muito maior que o 'decimal.MaxValue' (aprox 7.9e28).
        double valorIncompativel = 1e30; 
        var transacao = new Transacao();
        var contexto = new ValidationContext(transacao) { MemberName = nameof(Transacao.Valor) };
        var resultados = new List<ValidationResult>();
        // O Validator.TryValidateValue com essa sobrecarga vai validar o 
        // valor 'double' contra as regras da propriedade 'Valor' da classe Transacao.
        // Ele retorna 'true' se o valor passar no [Range].
        bool aprovadoPeloAtributo = Validator.TryValidateValue(
            valorIncompativel, 
            contexto, 
            resultados, 
            true // Indica que deve validar todos os atributos da propriedade
        );
        
        Assert.True(aprovadoPeloAtributo, "Bug Confirmado: O Atributo [Range] deveria bloquear valores maiores que o limite acima do tipo decimal.");
    }

    private readonly MinhasFinancasDbContext _context;

    /*
    * Valida exclusão em cascata de transações ao excluir pessoa
    */
    [Fact]
    public async Task Valida_Regra_Exclusao_Em_Cascata_Deve_Limpar_Transacoes()
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
        typeof(Transacao).GetProperty("Pessoa").SetValue(transacao, pessoa);
        typeof(Transacao).GetProperty("Categoria").SetValue(transacao, categoria);
        
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

        _context.Database.EnsureDeleted();
        _context.Dispose();
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
    public void Valida_Regra_Categoria_Deve_Respeitar_Finalidade(Transacao.ETipo tipoTransacao, Categoria.EFinalidade finalidadeCategoria, bool deveFuncionar)
    {
        // Arrange
        var categoria = new Categoria { Finalidade = finalidadeCategoria };
        var transacao = new Transacao { Tipo = tipoTransacao };

        // Act & Assert
        var prop = typeof(Transacao).GetProperty("Categoria");

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

    public void Dispose()
    {
    }
}
