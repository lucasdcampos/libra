# Estrutura do Projeto
Para começar a se locomover pelo código-fonte da Libra, é importante se ter ideia de como se está estruturado o projeto, aqui vai uma visão geral.

## Pastas
As pastas na raíz do repositório são:

### biblioteca/
Aqui está toda a biblioteca padrão da Libra, com diversas funções básicas que o usuário pode importar aos seus projetos. Todo o código de lá é escrito em Libra.

### docs/
Documentação da Libra e documentação do desenvolvimento da Linguagem (para contribuidores).

### exemplos/
Exemplos de códigos simples de programas escritos em Libra, para quem quer conhecer a sintaxe da linguagem.

### scripts/
Scripts usados para automatizar processos de compilação e outras tarefas.

### src/
O código-fonte da Libra em si, o coração do projeto. Contém todo o código C# do Interpretador da Libra, é onde a linguagem é implementada.

### testes/
Contém scripts de teste para verificar se nenhuma funcionalidade da Libra foi quebrada, os testes apontam com precisão onde algo deu errado e são úteis para identificar problemas.

<hr>

## Libra
Como qualquer outra linguagem, a Libra é separada em 3 fases principais: **Tokenização (lexing), Parsing e Execução.**

O Tokenizador e Parser, ou front-end da linguagem, está em `src/libra/Parser/`, a ordem de chamada é `Tokenizador.cs` -> `Parser.cs` e `Interpretador.cs` (que está em `src/libra/Libra/`).

`src/libra/Arvore` contém as definições dos nós da AST, `src/libra/Libra/` contém as classes associadas ao Runtime da Libra, `src/libra/Modulos/` contém funções nativas escritas em C# que podem 
ser chamadas internamente da Libra, e por fim, `src/libra/Utils/` contém arquivos com funções feitas para facilitar a vida do desenvolvedor.

<hr>
Se quiser saber por onde começar a entender o código, veja [Tokenização](tokenizacao.md)
