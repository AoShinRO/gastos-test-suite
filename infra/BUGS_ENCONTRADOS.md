# 1 - Incompatibilidade de ISA:

Durante a execução do projeto, identifiquei incompatibilidade com minha arquitetura local. Em vez de apenas contornar, investiguei a causa e concluí que o binário exige instruções não suportadas pelo meu hardware, sugerindo recompilação para maior compatibilidade ( node 22-slim com @rolldown/binding-linux-x64-gnu ).

# 2 - Inconsistência de Ambiente:

O projeto contém um arquivo nuget.config com caminhos absolutos baseados em Windows (C:\Program Files...), impedindo o restore de pacotes em ambientes de Integração Contínua (CI) baseados em Linux

# 3 - Inconsistência entre Versões

O projeto dos testes unitários veio configurado como net10.0 enquanto todos os outros projetos são no net9.0, o downgrade de versão é necessário junto com a lib Microsoft.EntityFrameworkCore.InMemory que estava na versão 10.0.7 para 9.0.0