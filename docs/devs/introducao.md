# Introdução Libra

Libra é uma linguagem de programação simples desenvolvida em C#.

Para funcionar, o código fonte escrito em Libra é lido pelo compilador, onde passa pelas seguintes etapas:

**Lexer** &#8594; O código é lido como uma *string*, onde então é analisado caractere por caractere, sendo transformado em uma lista de *tokens*.

**Parser** &#8594; A lista de tokens não possui sentido a principio, é dever do Parser organizá-la em uma lista de *instruções*, que serão depois interpretadas pelo Interpretador.

**Interpretador** &#8594; A lista de instruções geradas pelo Parser virão na forma de um *programa*, a função do Interpretador é analisar cada instrução individualmente, e executá-la.

### O que é um Token?
Token é uma forma mais eficiente de análisarmos pedaços do código. Por exemplo:

```js
var x = 10
```
Esse código é lido pelo Lexer (também chamado de *Tokenizador*), gerando os seguintes tokens:

Token &#8594; Tipo: Var <br>
Token &#8594; Tipo: Identificador | Valor: "x" <br>
Token &#8594; Tipo: OperadorDefinir <br>
Token &#8594; Tipo: NumeroLiteral | Valor: 10 <br>
Token &#8594; Tipo: FimDoArquivo
