# 1 - Inconsistência de Ambiente:

O projeto contém um arquivo nuget.config com caminhos absolutos baseados em Windows (C:\Program Files...), impedindo o restore de pacotes em ambientes de Integração Contínua (CI) baseados em Linux

# 2 - Inconsistência entre Versões

O projeto dos testes unitários veio configurado como net10.0 enquanto todos os outros projetos são no net9.0, o downgrade de versão é necessário junto com a lib Microsoft.EntityFrameworkCore.InMemory que estava na versão 10.0.7 para 9.0.0