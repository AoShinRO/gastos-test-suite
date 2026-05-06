import { test, expect } from '@playwright/test';

/*
* Evidencia falha de Lógica Contábil na interface do usuário
*/
test.describe('PoC de Lógica de Dashboard (MonthlySummary)', () => {

    const API_URL = 'http://localhost:5173';

    test('Deve provar o bug de classificação errônea por saldo (Despesa virando Receita)', async ({ page }) => {
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

        // Espera o gráfico renderizar (aumentado timeout para segurança no CI)
        const chartSector = page.locator('.recharts-pie-sector path').first();
        await expect(chartSector).toBeVisible({ timeout: 60000 });

        // EVIDÊNCIA DO BUG:
        // O sistema deveria saber que 'Aluguel' é Despesa e pintar de Vermelho (#ef4444).
        // Mas, devido à lógica falha, ele vai pintar de Verde (#10b981) porque o saldo é 100.
        const fillColor = await chartSector.getAttribute('fill');

        console.log(`Cor detectada para Aluguel com saldo positivo: ${fillColor}`);

        // Se a cor for verde (#10b981), o bug de lógica está provado!
        expect(fillColor).toBe('#10b981');
    });
});
