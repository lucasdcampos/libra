[Documentação](README.md) -> [Variáveis](#)

# Variáveis
Uma variável é uma forma de armazenar dados na memória para que possam ser usados posteriormente no programa. 
Na Libra, as variáveis podem ser declaradas com as palavras-chave var ou const, dependendo do comportamento desejado.

**Declaração de Variável:**
```
(var | const) identificador = expressao
```

**Exemplos:**
```js
var x = 10 // Cria uma variável `x` com valor 10
const PI = 3.14 // Cria uma constante `PI` com valor 3.14, que não pode ser alterado
```
Por convenção, constantes são definidas com letras maiusculas, isso não é obrigatório, mas recomendado.

**Acessando uma Variável:**
```js
var a = 10
var b = 5
var c = a + b // Resulta em 15
```

**Modificando uma Variável**
```js
var a = 1
a = 2 // Trocamos o valor de `a` para 2.
```
**Importante:** Note que não usamos `var` aqui para modificar o valor de **a**, porque ela já foi declarada previamente no programa.

Variáveis também podem conter outros tipos de valores, como Textos ou Vetores:
```js
const NOME = "Libra" 
var lista = {1, 3, 5, 7, 9}
```

Próximo Capítulo: [Controles de Fluxo](controles_de_fluxo.md)
