using MinhasFinancas.Domain.Entities;
using Xunit;

namespace MinhasFinancas.Tests.Unit;

#pragma warning disable CS1591
#pragma warning disable CA1707

public sealed class PessoaTests : IDisposable
{
    /*
    * Valida as regras de cálculo de idade e maioridade.
    */
    public static IEnumerable<object[]> Valida_Pessoa_Maioridade()
    {       
        // Valida Regra de Negócio - Maioridade
        yield return new object[] { "Pessoa com 18 anos aniversariante de hoje", DateTime.Today.AddYears(-18), true };

        // Testes Cronológicos - Evidencia Inconsistencia em Data Nascimento impossiveis ao ser humano.
        yield return new object[] { "Pessoa com 0 anos", DateTime.Now, false };
        yield return new object[] { "Pessoa com data futura", DateTime.Today.AddYears(3), false };
        yield return new object[] { "Pessoa com data histórica", new DateTime(1, 1, 1), true };   

        // Testes de String - Evidencia Falta de Sanitização de dados
        yield return new object[] { new string('A', 5000), DateTime.Today.AddYears(-20), true }; 
        yield return new object[] { "<script>alert(1)</script>", DateTime.Today.AddYears(-18), true };
        yield return new object[] { "", DateTime.Today.AddYears(-25), true };        
    }
    
    /*
    * Valida as regras de cálculo de idade e maioridade.
    */
    [Theory]
    [MemberData(nameof(Valida_Pessoa_Maioridade))]
    public void Valida_Regra_Pessoa_Maioridade(string nome, object dataNascimentoInput, bool esperadoMaiorIdade)
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
