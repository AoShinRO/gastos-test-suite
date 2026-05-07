import { test, expect } from '@playwright/test';

test.describe('PoC Validacao de Seguranca (API Bypass)', () => {

  // Foi necessário substituir o localhost por 127.0.0.1 para rodar no Github Action, 
  // pois o localhost não funcionou. Em caso de problemas para rodar local
  // testar http://localhost:5135/api/v1.0

  const API_URL = 'http://127.0.0.1:5135/api/v1.0';

  /**
   * Este teste evidencia a falta de sanitização de dados na criação de Pessoas via API.
   */
  test('Deveria recusar script malicioso (Stored XSS)', async ({ request }) => {
    const xssPayload = "<script>alert('XSS_REAL')</script>";

    const response = await request.post(`${API_URL}/Pessoas`, {
      data: {
        nome: xssPayload,
        dataNascimento: '1985-10-10T00:00:00'
      }
    });

    expect(response.status()).toBe(201);

    const person = await response.json();
    expect(person.nome).toBe(xssPayload);
    console.log('XSS persistido com sucesso no banco de dados.');
  });

  /**
   * Evidencia que a API possui validação para nomes excessivamente grandes
   * (controle presente na camada de API, mas não no domínio)
   */
  test('Deve recusar nome gigante via API (Tentativa Bypass de Validação)', async ({ request }) => {
    const hugeName = 'A'.repeat(1000);

    const response = await request.post(`${API_URL}/Pessoas`, {
      data: {
        nome: hugeName,
        dataNascimento: '1990-01-01T00:00:00'
      }
    });

    console.log(`Status do Bypass de Comprimento: ${response.status()}`);
    expect(response.status()).toBe(400);
  });

  /**
   * Evidencia que a API possui validação para valores negativos
   * (controle presente na camada de API, mas não no domínio)
   */
  test('Deve recusar valor negativo via API (Tentativa Bypass de Regra de Negócio)', async ({ request }) => {
    const response = await request.post(`${API_URL}/Transacoes`, {
      data: {
        descricao: 'Ataque de Valor Negativo',
        valor: -500.50,
        tipo: 0, // Despesa
        categoriaId: '00000000-0000-0000-0000-000000000000', // ID fictício
        pessoaId: '00000000-0000-0000-0000-000000000000',
        data: new Date().toISOString()
      }
    });

    console.log(`Status do Bypass de Valor Negativo: ${response.status()}`);
    expect(response.status()).toBe(400);
  });
});
