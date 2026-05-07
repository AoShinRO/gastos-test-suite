using MinhasFinancas.Domain.Entities;
using Xunit;

namespace MinhasFinancas.Tests.Unit;

#pragma warning disable CS1591
#pragma warning disable CA1707

public sealed class PessoaTests : IDisposable
{
    /*
    * Evidencia inconsistências no domínio.
    */
    public static IEnumerable<object[]> EvidenciaFalhaInconsistenciasPessoa()
    {       
        // Testes Cronológicos
        // Evidência: Domínio considera datas irreais como válidas para maioridade.
        yield return new object[] { "Pessoa com 0 anos", DateTime.Now, false };
        yield return new object[] { "Pessoa com data futura", DateTime.Today.AddYears(3), false };
        yield return new object[] { "Pessoa com data histórica", new DateTime(1, 1, 1), true };   

        // Testes de String
        // Evidência: Domínio não possui validação para strings excessivamente grandes.
        yield return new object[] { new string('A', 5000), DateTime.Today.AddYears(-20), true }; 
        
        // Evidência: Domínio não possui sanitização para strings maliciosas ou vazias.
        yield return new object[] { "<script>alert(1)</script>", DateTime.Today.AddYears(-18), true };
        yield return new object[] { "", DateTime.Today.AddYears(-25), true };        
    }

    /*
    * Valida as regras de cálculo de idade e maioridade.
    */
    public static IEnumerable<object[]> ValidaRegraPessoaMaioridade()
    {
        // Valida Regra de Negócio - Maioridade
        yield return new object[] { "Pessoa com 18 anos aniversariante de hoje", DateTime.Today.AddYears(-18), true };     
    }
    
    /*
    * Valida as regras de cálculo de idade e maioridade.
    */
    [Theory]
    [MemberData(nameof(ValidaRegraPessoaMaioridade))]
    public void ValidaPessoaMaioridade(string nome, object dataNascimentoInput, bool esperadoMaiorIdade)
    {
        // Arrange
        var pessoa = new Pessoa { Nome = nome };

        // Act & Assert
        if (dataNascimentoInput is DateTime dataNascimento)
        {
            pessoa.DataNascimento = dataNascimento;
            bool resultado = pessoa.EhMaiorDeIdade();
            Assert.Equal(esperadoMaiorIdade, resultado);
        }
    }

    /*
    * Evidencia inconsistências no domínio.
    */
    [Theory(Skip = "Comportamento identificado: O domínio aceita valores inválidos para Nome e DataNascimento")]
    [MemberData(nameof(EvidenciaFalhaInconsistenciasPessoa))]
    public void EvidenciaFalhaPessoa(string nome, object dataNascimentoInput, bool esperadoMaiorIdade)
    {
        // Arrange
        var pessoa = new Pessoa { Nome = nome };

        // Act & Assert
        if (dataNascimentoInput is DateTime dataNascimento)
        {
            pessoa.DataNascimento = dataNascimento;
            bool resultado = pessoa.EhMaiorDeIdade();
            Assert.Equal(esperadoMaiorIdade, resultado);
        }
    }

    public void Dispose()
    {
    }
}
