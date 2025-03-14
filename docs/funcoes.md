[Documentação](README.md) -> [Funções](#)

# Funções
As funções são blocos de código reutilizáveis que realizam uma tarefa específica. 
Elas ajudam a organizar o programa, evitando a repetição de código e facilitando a manutenção. Na Libra, as funções são definidas com a palavra-chave `funcao`.

**Definindo uma Função:**
```
funcao identificador(parametros)
    corpoFuncao
fim
```
**Descrição:**
- funcao: Palavra-chave para iniciar a definição de uma função.
- identificador: Nome da função, que segue as mesmas regras de identificadores das variáveis (letras, números e underscores, sem iniciar com números).
- parametros: Valores externos que a função receberá, separados por vírgula
- corpoFuncao: Bloco de código contendo as instruções que a função executará.
- fim: Indica o final da definição da função.

**Exemplo:**
```js
funcao saudacao()
    exibir("Olá, mundo!")
fim

// Chamando a função (exibe Olá, Mundo! na tela)
saudacao()
```

## Parâmetros e Argumentos
Os parâmetros são variáveis declaradas na definição da função. Eles atuam como "espaços reservados" para os valores que serão fornecidos no momento da chamada da função.
**Exemplo:**
```js
// nome é um parâmetro da função
funcao saudar(nome)
    exibir("Olá " + nome)
fim

// Chamando a função passando o argumento "Pedro"
saudar("Pedro")
// Resultado: Exibe "Olá Pedro" na tela.
```

## Retorno de Funções
As funções podem devolver valores para o código que as chamou, permitindo que o resultado de um cálculo ou operação seja utilizado posteriormente.
Isso é feito utilizando a palavra-chave `retornar`.

### Como funciona o retorno
O comando `retornar` encerra a execução da função e envia um valor de volta para o ponto onde a função foi chamada.
Se nenhuma instrução retorne for usada, a função retornará `Nulo`.

**Exemplo:**
```js
funcao soma(a, b)
  retornar a+b
fim

// O valor retornado da função será armazenado em resultado (que será 3)
var resultado = soma(1, 2)
```

Próximo Capítulo: [Bibliotecas](bibliotecas.md)
