using MinhasFinancas.Domain.Entities;
using Xunit;

namespace MinhasFinancas.Tests.Unit;

#pragma warning disable CS1591
#pragma warning disable CA1707

/// <summary>
/// Testes unitários para a entidade Pessoa.
/// </summary>
public sealed class PessoaTests : IDisposable
{
    /// <summary>
    /// Valida as regras de cálculo de idade e maioridade.
    /// </summary>
    [Theory]
    [MemberData(nameof(GetPessoaTestData))]
    public void ValidarIdadeEMaioridade(string nome, object dataNascimentoInput, bool esperadoMaiorIdade)
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

    public static IEnumerable<object[]> GetPessoaTestData()
    {
        // Testes de String
        yield return new object[] { new string('A', 205), DateTime.Today.AddYears(-20), true };
        yield return new object[] { "Caracteres malucos ¨%&%$@#@!&*$", DateTime.Today.AddYears(-18), true };   
        yield return new object[] { "<script>alert(1)</script>", DateTime.Today.AddYears(-18), true };   
        yield return new object[] { "'; DROP TABLE Pessoas; --", DateTime.Today.AddYears(-30), true };         
        yield return new object[] { "", DateTime.Today.AddYears(-25), true };
        
        // Testes Cronológicos
        yield return new object[] { "Pessoa com data futura", DateTime.Today.AddYears(3), false };
        yield return new object[] { "Pessoa com data histórica", new DateTime(1, 1, 1), true };      
        yield return new object[] { "Pessoa com 17-18 anos aniversariante de hoje", DateTime.Today.AddYears(-18), true };
    }

    public void Dispose()
    {
    }
}
