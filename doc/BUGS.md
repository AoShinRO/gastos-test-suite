# 📄 Relatório de Bugs e Falhas de Implementação

Este documento consolida os principais comportamentos inesperados durante a análise do sistema.  
Os itens estão organizados por nível de impacto e priorizam regras de negócio, segurança e integridade de dados.

---

# 🔴 Críticos (Segurança, Estabilidade, Perda de Dados)

## 1. Persistência de Payloads

### ValidacaoSeguranca.spec.ts

<details>
  <summary>Ver evidência</summary>
  <img src="https://github.com/AoShinRO/gastos-test-suite/blob/main/doc/evidencias/1-evidencia_xss_dashboard.jpg">
</details>

**Resumo:**  
Dados são persistidos sem sanitização nas entidades.
 
**Impacto:**                                                         
Permite armazenamento persistente de payloads maliciosos (Stored XSS),                           
abrindo vetor de ataque para múltiplos clientes consumidores da API.

**Evidência:**  
Teste E2E aceita e persiste `<script>alert(1)</script>` com retorno 201.

**Observação:**  
O React escapa na UI atual, mas o problema permanece no nível de persistência.

**Solução sugerida:**  
Sanitização na camada de aplicação (ex: `Ganss.XSS`).

---

## 2. Cache sem Invalidação

### Comportamento observável

**Resumo:**  
Cache não é atualizado após mutações.

**Impacto:**  
Dados inconsistentes na UI, podendo induzir decisões financeiras incorretas.

**Evidência:**  
Dashboard não reflete nova transação até refresh.

**Solução sugerida:**  
Invalidar cache após alterações.

---

## 3. Falta de Filtro Padrão Seguro

### TotaisQueryTests.cs / EvidenciaFalhaFiltroNuloRetornaTodoHistorico()

**Resumo:**  
Filtro nulo retorna todas as transações.

**Impacto:**  
Totais incorretos (inclui dados históricos).

**Evidência:**  
Teste retorna soma acumulada (150 ao invés de período atual).

**Solução sugerida:**  
Definir período padrão (ex: mês atual).

---

## 4. Falha de Tratamento de Regra de Negócio

<details>
  <summary>Ver evidência</summary>
  <img src="https://github.com/AoShinRO/gastos-test-suite/blob/main/doc/evidencias/2-evidencia_crash_servidor.jpg">
</details>

**Resumo:**  
Inconsistência entre UI e domínio gera exceção não tratada.

**Cenário:**
Tentar registrar despesa com categoria de receita.

**Impacto:**  
Erro 500 causando instabilidade do sistema.

**Evidência:**  
`InvalidOperationException` ao registrar categoria incompatível.

**Solução sugerida:**  
Tratamento no controller + validação alinhada com frontend.

---

# 🟠 Alto (Integridade e Regras de Negócio)

## 5. Falta de Validação nos Domínios

### TransacaoTests.cs / EvidenciaFalhaInconsistenciaDadosInvalidos()

**Resumo:**  
Domínios aceitam dados inválidos sem restrição.

**Impacto:**  
Corrupção de dados e dependência total do frontend.

**Evidência:**  
Objetos com strings gigantes, strings vazias,
números negativos e dados inválidos são aceitos.

**Solução sugerida:**  
Self-validating entities.

---

## 6. Lógica Incorreta no Dashboard

### LogicaContabil.spec.ts

**Resumo:**  
Classificação baseada em saldo, não na natureza da categoria.

**Impacto:**  
Despesa pode aparecer como receita.

**Evidência:**  
Cor verde para categoria de despesa com saldo positivo.

**Solução sugerida:**  
Usar tipo da categoria ao invés do saldo.

---

## 7. CORS Inseguro (AllowAll)

### Definido em Program.cs

**Resumo:**  
API permite qualquer origem, método e header.

**Impacto:**  
Exposição a CSRF e consumo indevido da API por origens não confiáveis.

**Solução sugerida:**  
Whitelist de origens confiáveis.

---

# 🟡 Médio (UX / Consistência)

## 8. Shadow Properties (EF Core)

### ShadowEFCoreTests.cs

**Resumo:**  
EF gera colunas fantasmas por mapeamento incorreto.

**Impacto:**  
Possível degradação e inconsistência em queries.

**Evidência:**  
Presença de propriedades como `CategoriaId1` e `PessoaId1`.

---

## 9. Range Incompatível com Decimal

### TransacaoTests.cs / EvidenciaFalhaRangeInconsistenteNoDominio()

**Resumo:**  
`[Range]` usa `double` em propriedade `decimal`.

**Impacto:**  
Falha na validação devido a incompatibilidade de tipos.

**Solução sugerida:**  
Definir um teto realista para o negócio, evitando que
valores absurdos quebrem o layout ou outros cálculos do sistema.
( ex: `[Range(typeof(decimal), "0.00", "1000000000000")]` )

---

## 10. Validação de Pessoa Inconsistente

### PessoaTests.cs / EvidenciaFalhaInconsistenciasPessoa()

**Resumo:**  
Datas inválidas são aceitas (ex: ano 0001).

**Impacto:**  
Dados irreais no sistema.

**Solução sugerida:**  
Uso de Value Objects.

---

## 11. Falha de Renderização por Dados Não Validados

<details>
  <summary>Ver evidência</summary>
  <img src="https://github.com/AoShinRO/gastos-test-suite/blob/main/doc/evidencias/3-evidencia_ux_renderizacao.jpg">
</details>

**Resumo:**  
Strings maliciosas quebram renderização do dashboard.

**Impacto:**  
Interface inutilizável (`width(-1)` no gráfico).

**Evidência:**  
Categoria com nome inválido impede renderização.

**Solução sugerida:**  
Limite de tamanho + truncate na UI.

---

## 12. Problema em Selects

### Comportamento observável

**Resumo:**  
Seleção de strings em listas (dropdowns) exige interação redundante.

**Impacto:**  
Frustração e erro de uso.

---

## 13. Feedback Visual Incorreto

<details>
  <summary>Ver evidência</summary>
  <img src="https://github.com/AoShinRO/gastos-test-suite/blob/main/doc/evidencias/3-evidencia_ux_renderizacao.jpg">
</details>

**Resumo:**  
Valores negativos exibidos como positivos (verde).

**Impacto:**  
Interpretação errada pelo usuário.

---

# 🔵 Baixo (Manutenibilidade)

## 14. Padronização de Código

**Resumo:**  
Mistura de português e inglês.

**Impacto:**  
Dificulta manutenção e onboarding.

---

# 🟣 Observações Complementares (CodeQL)

Além da análise comportamental e dos testes automatizados desenvolvidos manualmente,
também foi executada análise estática com CodeQL como verificação complementar.

Os alertas abaixo não foram explorados diretamente nos testes implementados,
mas indicam potenciais pontos de atenção adicionais no projeto:

- Missing function level access control
- Insecure Direct Object Reference (IDOR)
- Comparison between inconvertible types
- Unused or undefined state property
- Generic catch clause
- Path.Combine silently dropping arguments

```
Os itens acima representam sinais identificados pela análise estática,
mas não foram classificados neste relatório com o mesmo nível de severidade
dos problemas reproduzidos via testes automatizados e evidências práticas.
```

---

# 🧠 Observações Finais

- Alguns problemas não se manifestam diretamente no ambiente atual (ex: SQLite, React),
  mas representam riscos reais em cenários de evolução do sistema.
- A maioria das falhas está relacionada à ausência de validação no domínio e inconsistência entre camadas.
