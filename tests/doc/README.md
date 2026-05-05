### 1 - Database Queries

Identificado necessidade de revisão em todas as Database Queries para garantir a atomicidade das operações financeiras, visto que o uso atual de métodos assíncronos e cache não possuem estratégias de bloqueio para evitar condições de corrida (Race Conditions), o ideal é garantir sincronicidade total das operações financeiras.
As inconsistências identificadas logo no build inicial (Shadow States e Null Warnings) são sintomas de uma arquitetura que prioriza a estética do código (async/await, Clean Arch no papel) em detrimento da correção semântica e integridade dos dados. O projeto falha em garantir que o que está no código reflete o que está no banco de dados, prejudicando movimentações financeiras futuras.

### 2 -
