[Documentação](README.md) -> [Controles de Fluxo](#)

# Controles de Fluxo
Os **controles de fluxo** são construções que permitem decidir quais partes do código serão executadas com base em condições ou repetições.
Eles são essenciais para criar lógica nos programas, controlando a execução de instruções de forma condicional ou iterativa.

## Condicional (`se`)
O comando `se` permite executar um bloco de código apenas se uma condição for verdadeira. Caso contrário, outros blocos podem ser avaliados ou executados com os comandos `senao se` ou `senao`.

**Exemplo**
```js
se x > 10 entao
    exibir("x é maior que 10")
senao se x == 10 entao
    exibir("x é igual a 10")
senao
    exibir("x é menor que 10")
fim
```
**Descrição**
- **expressao**: Uma condição que avalia para verdadeiro ou falso (Qualquer valor diferente de zero é considerado verdadeiro).
- **entao**: Marca o início do bloco de instruções a ser executado caso a condição seja verdadeira.
- **senao se**: Avalia outra condição se as anteriores forem falsas.
- **senao**: Executa um bloco de código caso nenhuma das condições anteriores seja verdadeira.
- **fim**: Indica o final do controle condicional.

## Laço (`enquanto`)
O comando `enquanto` permite repetir um bloco de código enquanto uma condição for verdadeira.
**Exemplo:**
```js
var contador = 0
enquanto contador < 5 repetir
    exibir(contador)
    contador = contador + 1
fim
// Resultado: Mostra 0, 1, 2, 3, 4 na tela.
```
**Descrição:**
- **expressao**: Uma condição que é avaliada antes de cada iteração. Se for falso, o laço é encerrado.
- **repetir**: Marca o início do bloco de instruções a ser repetido.
- **fim**: Indica o final do laço.

## Iteração em Vetores (`para cada`)
O comando `para cada` facilita percorrer todos os elementos de um vetor (lista).

**Exemplo:**
```js
var frutas = {"Maçã", "Banana", "Uva"}
para cada f em frutas
    exibir("Eu gosto de: " + f)
fim
```
**Descrição:**
- **identificador**: O nome da variável temporária que receberá o valor de cada item em cada volta do laço.
- **em**: Indica qual vetor será percorrido.
- **fim**: Indica o final do laço.

## Tratamento de Erros (`tentar / capturar`)
O bloco `tentar` permite capturar erros que ocorrem durante a execução do programa e tratá-los de forma elegante, evitando que o programa "quebre" inesperadamente.

**Exemplo:**
```js
tentar
    var resultado = 10 / 0
capturar(erro)
    exibir("Ocorreu um erro: " + erro)
fim
```
**Descrição:**
- **tentar**: Inicia o bloco de código que pode vir a falhar.
- **capturar(identificador)**: Se um erro ocorrer no bloco `tentar`, a execução pula para cá. O `identificador` recebe a mensagem do erro.
- **fim**: Indica o final do bloco de tratamento.

Próximo Capítulo: [Funções](funcoes.md)
