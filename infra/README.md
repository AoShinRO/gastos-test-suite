# 1 - Inconsistência de Ambiente

O projeto contém um arquivo nuget.config com caminhos absolutos baseados em Windows (C:\Program Files\...), o que impede o restore de pacotes em ambientes de Integração Contínua (CI) baseados em Linux.

# 2 - Inconsistência entre Versões

O projeto de testes unitários estava configurado como net10.0, enquanto os demais projetos utilizam net9.0.  
Foi necessário alinhar a versão para net9.0, incluindo o downgrade da dependência Microsoft.EntityFrameworkCore.InMemory de 10.0.7 para 9.0.0, garantindo compatibilidade e execução adequada dos testes.

# 3 - Estrutura de Execução dos Testes

Para permitir a execução dos testes automatizados em ambiente isolado, foi necessário recriar/ajustar arquivos de solução (.sln) e projeto (.csproj), contendo apenas a estrutura mínima necessária para execução dos testes, sem inclusão do código da aplicação.