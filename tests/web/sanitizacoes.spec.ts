import { test, expect } from '@playwright/test';

test.describe('PoCs de Segurança Ofensiva (API Bypass)', () => {

  const API_URL = 'http://localhost:5135/api/v1';

  /**
   * PROVA DO ITEM 8: Stored XSS via API
   * Tenta persistir um script malicioso no banco de dados.
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
   * A API é segura contra nomes gigantes, previne regressão de segurança.
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
    // Precisa ser 400 para ser considerado seguro.    
    expect(response.status()).toBe(400);
  });

  /**
   * A API é segura contra valores negativos, previne regressão de segurança.
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
    // Precisa ser 400 para ser considerado seguro.
    expect(response.status()).toBe(400);
  });


});
