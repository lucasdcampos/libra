# Biblioteca Padrão da Libra

Aqui você encontra toda a biblioteca padrão da Libra. Há funções básicas para você importar
ao seu projeto, como funções de matemática, funções para manipular vetores, funções para
interagir com o Sistema Operacional (Ler arquivos, Escrever em arquivos, etc).

## Como usar?

Para importar as bibliotecas ao seu código, faça conforme o exemplo:

```js
importar "matematica.libra"
importar "vetores.libra"

var vetor = { 1, 2, 3}
vetor = reverter(vetor) // Reverte para { 3, 2, 1 }

exibir(PI) // Exibe a constante PI da biblioteca de matemática
```

## Vantagens

Essa abordagem permite a reutilização de código em diversos projetos e diminui trabalho repetitivo.

## O que são arquivos .cs.libra?

Esses arquivos são códigos C# que serão carregados internamente
pela Libra, isso da a possibilidade de interagir com o ambiente .NET dentro da Libra.