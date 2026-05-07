# 🧪 Test Suite - Controle Financeiro de Pessoas

Este repositório contém a implementação de uma suíte de testes automatizados para um sistema de controle financeiro em .NET e React/TypeScript, com foco em validação de regras de negócio, identificação de falhas e garantia de comportamento esperado.

> ✔️ O código da aplicação original **não está incluído**, conforme orientação.

---

# 🎯 Objetivo

Avaliar o sistema existente através de:

- Validação das regras de negócio
- Construção de uma pirâmide de testes
- Identificação de falhas de implementação
- Aplicação de boas práticas em qualidade de código

---

### ✔️ Regras validadas

✔️ Operações CRUD de Pessoas

✔️ Cadastro de Categorias

✔️ Cadastro de Transações

✔️ Menor de idade não pode ter receitas

✔️ Exclusão em cascata de transações

### ❌ Regras com falha

❌ Consultas de totais por pessoa -> Falha ao lidar com filtro nulo (retorna dados históricos indevidos)

❌ Categoria fora da finalidade -> Gera exception não tratada (erro 500)

❌ Persistência sem sanitização -> Vulnerável a Stored XSS

❌ Cache inconsistente -> Totais não refletem dados atualizados após operações

---

# 🧱 Estrutura da Pirâmide de Testes

A estratégia adotada segue o conceito de pirâmide:

### 🔹 Testes Unitários (.NET / xUnit)

- Foco no domínio (`Entities`)
- Validação direta de regras de negócio
- Execução isolada, sem dependências externas

---

### 🔹 Testes de Integração (.NET / EF Core InMemory)

- Validação de comportamento envolvendo múltiplas camadas
- Testes de queries, persistência e regras dependentes do EF Core
- Simulação de cenários reais de uso da aplicação

---

### 🔹 Testes End-to-End (Playwright)

- Simulação de comportamento real do usuário
- Validação completa (frontend + backend)
- Provas práticas de falhas (segurança, UX e lógica)

---

# ⚠️ Ajustes Necessários para Execução

Para viabilizar a execução dos testes em ambiente isolado:

- Correção de `nuget.config` com paths Windows
- Ajuste de versões (.NET 10 para .NET 9)
- Criação de arquivos `.sln` e `.csproj` mínimos

✔️ Nenhuma alteração foi feita no código da aplicação.

---

# 🛠️ Como Executar

## 📋 Pré-requisitos

```bash
.NET SDK (latest)
Bun (0.7.3)
Playwright (latest)
```

Os testes devem ser executados sobre uma cópia local do projeto original.

- Copie a pasta `tests/` para a raiz do projeto
- Copie os arquivos de `infra/api/` (.sln e .csproj) para a raiz da API

---

## 🧪 Testes Unitários

```bash
cd api
dotnet test
```

---

## 🌐 Testes E2E (Playwright)

```bash
cd web
bun install
bunx playwright install
bunx playwright test
```

---

## ⚙️ CI (GitHub Actions)

O repositório possui pipeline automatizado que executa:

- Testes unitários (.NET)
- Testes E2E (Playwright)
- Análise estática com CodeQL

Execução automática em:

- Push na branch `main`
- Pull Requests

---

# 📋 Bugs Identificados (Resumo)

Durante a análise foram identificados problemas relacionados a:

- Persistência de dados sem sanitização
- Falta de validação no domínio
- Inconsistências em regras de negócio
- Problemas de cache e atualização de dados
- Falhas de UX que podem induzir erro
- Questões estruturais no mapeamento do banco
- Questões práticas que inviabilizariam escalonamento do produto

📂 Detalhamento completo disponível em:

[Bugs Encontrados](https://github.com/AoShinRO/gastos-test-suite/blob/main/doc/BUGS.md)

---

# 🧠 Decisões Técnicas

### ✔️ Foco no Domínio

Grande parte dos testes unitários foi direcionada às entidades, garantindo validação direta das regras de negócio.

### ✔️ Uso de E2E como prova de falhas

Os testes com Playwright foram utilizados não apenas para validação funcional, mas também como evidência prática de inconsistências do sistema.

### ✔️ Abordagem investigativa

Os testes foram construídos a partir da análise do comportamento real do sistema, e não apenas da especificação.

---

# 🚀 Considerações

A suíte foi construída com foco em:

- Clareza
- Reprodutibilidade
- Capacidade de evidenciar problemas reais

Mais do que validar comportamento esperado, o foco foi identificar falhas reais de negócio,
riscos de evolução do sistema e inconsistências entre camadas.

---

# 📂 Estrutura do Repositório

```
.
├── README.md
├── .github/
│   └── workflows/
│       └── main.yml
├── doc/
│   ├── BUGS.md
│   └── evidencias/
│       ├── 1-evidencia_xss_dashboard.jpg
│       ├── 2-evidencia_crash_servidor.jpg
│       └── 3-evidencia_ux_renderizacao.jpg
├── infra/
│   ├── README.md
│   └── api/
│       ├── MinhasFinancas.Tests.Unit/
│       │   └── MinhasFinancas.Tests.Unit.csproj
│       └── MinhasFinancas.slnx
└── tests/
    ├── api/
    │   └── MinhasFinancas.Tests.Unit/
    │       ├── CategoriaTests.cs
    │       ├── DataFilterTests.cs
    │       ├── PessoaTests.cs
    │       ├── ShadowEFCoreTests.cs
    │       ├── TotaisQueryTests.cs
    │       └── TransacaoTests.cs
    └── web/
        ├── LogicaContabil.spec.ts
        ├── ValidacaoSeguranca.spec.ts
        └── playwright.config.ts

```

---

# 🧾 Observações Finais

- Nem todos os problemas se manifestam diretamente no ambiente atual
  (ex: SQLite, comportamento do React), mas representam riscos reais em cenários de evolução.

- A maior parte das falhas identificadas está relacionada à falta de validação no domínio
  e inconsistência entre camadas da aplicação.
