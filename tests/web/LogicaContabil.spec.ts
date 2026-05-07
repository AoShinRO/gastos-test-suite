import { test, expect } from '@playwright/test';

test.describe('PoC de falha de lógica contábil no Dashboard', () => {

    const API_URL = 'http://localhost:5173';
    /*
    * Este teste evidencia falha de lógica contábil na interface do usuário
    */
    test('Evidencia falha de lógica contábil por saldo (Despesa categorizada como Receita)', async ({ page }) => {
        // Cenário: Estorno de Aluguel (Despesa) sendo categorizado como Receita
        // Intercepta a chamada da API de totais por categoria
        await page.route('**/api/v1.0/totais/categorias*', async route => {
            const json = {
                items: [
                    {
                        descricao: 'Aluguel (Estorno)',
                        totalReceitas: 1000,
                        totalDespesas: 900,
                        saldo: 100 // Saldo positivo, mas a natureza da categoria é DESPESA
                    }
                ],
                totalCount: 1,
                page: 1,
                pageSize: 10
            };
            await route.fulfill({ json });
        });

        // Navega para a página inicial
        await page.goto(API_URL, { waitUntil: 'networkidle', timeout: 60000 });

        // Espera o gráfico renderizar
        const chartSector = page.locator('.recharts-pie-sector path').first();
        await expect(chartSector).toBeVisible({ timeout: 60000 });

        // EVIDÊNCIA DO BUG:
        const fillColor = await chartSector.getAttribute('fill');
        console.log(`Cor detectada para Aluguel com saldo positivo: ${fillColor}`);

        // Se a cor for verde (#10b981), evidencia falha de lógica contábil,
        // deveria ser vermelho (#ef4444) pois é despesa.
        expect(fillColor).toBe('#10b981');
    });
});
