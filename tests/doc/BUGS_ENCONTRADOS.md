# 1 - Shadow States (EF Core):

Configuração Incorreta de Fluent API ou Data Annotations:
O sistema gera propriedades em Shadow State (CategoriaId1), podendo causar inconsistência no relacionamento entre entidades.

# 2 - Observação de Padronização (Code Style):

O projeto utiliza uma nomenclatura híbrida (Português/Inglês) para classes e funções. Recomenda-se a escolher uma só lingua para o projeto manter a padronização e aumentar a manutenibilidade do sistema.


# Testes Unitários

Os problemas identificados estão relacionados principalmente à ausência de validação no domínio e inconsistências de configuração. Questões de segurança como SQL Injection e XSS dependem também da forma como os dados são utilizados em outras camadas da aplicação.

# 3 - Testes Unitários da classe Pessoa.cs - Violação de Regras de Domínio Críticas

O método valida maioridade apenas com base na diferença de datas, sem considerar limites realistas, permitindo registros inconsistentes.

### Falha de Consistência e Validação de Dados

A entidade Pessoa não possui validação em suas propriedades, permitindo a criação de objetos com dados inconsistentes diretamente em memória.

### Cenários Identificados:

- Estouro de Caracteres (Overflow):
O sistema permite nomes acima do limite definido (200 caracteres), podendo causar erro ao persistir no banco.

- Conteúdo não confiável:
A propriedade aceita entradas como `<script>alert(1)</script>`, o que pode gerar problemas caso esses dados sejam exibidos sem tratamento na interface.

- Entradas não validadas:
Strings contendo comandos SQL são aceitas como válidas, evidenciando ausência de validação no domínio.

- Strings vazias:
O sistema permite criação de pessoas sem nome.

# 4 - Testes Unitários da classe Transacao.cs - Incompatibilidade de Tipos de Ponto Flutuante

Existe uma inconsistência de "atenção". O sistema protege a regra de idade, mas não valida adequadamente os dados de entrada.

### Falha de Encapsulamento e Validação de Dados

- Em Valor apesar do atributo [Range], a entidade aceita valores como -100.00 sem disparar exceções de domínio.
- O teste ValidarInconsistenciaRangeValor confirma que o atributo [Range] utiliza double.MaxValue, superando em ordens de magnitude a capacidade de armazenamento do tipo decimal da propriedade Valor.
- Possibilidade de possíveis erros de processamento (Overflow) em processamentos de grandes volumes ou valores monetários extremos, onde a validação "aprova" um dado que o tipo de dado não consegue carregar.

# 5 - Testes Unitários da classe Categoria.cs - Falha de Consistência e Sanitização de Dados

A propriedade "Descricao" da categoria não possui validação de comprimento mínimo ou sanitização.

Efeito: O sistema permite a criação de categorias sem nome, o que é inconsistente com o objetivo do domínio de organizar transações de forma significativa.

# 6 - Testes Unitários da classe DataFilter.cs - Bug de Arredondamento Temporal (DataFilter)

### Falha de Compatibilidade de Precisão (Precision Mismatch)

O uso de .AddTicks(-1) gera uma precisão incompatível com o tipo datetime do SQL Server (3.33ms) causando o arredondamento da data final para o primeiro milissegundo do mês seguinte, resultando em inclusão indevida de registros fora do intervalo esperado.

Validado via teste unitário ProveRoundingBugSemBancoDeDados utilizando a estrutura SqlDateTime.
