[Documentação](README.md) -> [Classes e Objetos](#)

# Classes e Objetos
A Libra suporta Programação Orientada a Objetos (POO), permitindo criar seus próprios tipos de dados com propriedades (dados) e métodos (comportamentos).

## Definindo uma Classe
Uma classe é definida usando a palavra-chave `classe`. Dentro dela, você pode declarar variáveis e funções.

**Exemplo:**
```js
classe Pessoa
    var nome: Texto
    var idade: Int

    // Construtor: Uma função com o mesmo nome da classe
    funcao Pessoa(n: Texto, i: Int)
        auto.nome = n
        auto.idade = i
    fim

    funcao saudar()
        exibir("Olá, meu nome é " + auto.nome)
    fim
fim
```

## A palavra-chave `auto`
Dentro de um método de classe, a palavra-chave `auto` refere-se à instância atual do objeto. Ela é usada para acessar as propriedades e outros métodos da própria classe.

## Instanciando Objetos
Para criar um objeto a partir de uma classe, basta chamar o nome da classe como se fosse uma função.

**Exemplo:**
```js
var p1 = Pessoa("Lucas", 25)
p1.saudar() // Exibe: Olá, meu nome é Lucas

exibir(p1.idade) // Acesso direto à propriedade: 25
```

## Métodos e Propriedades
- **Propriedades**: São as variáveis definidas dentro da classe. Elas armazenam o estado do objeto.
- **Métodos**: São as funções definidas dentro da classe. Elas definem o que o objeto pode fazer.

Você pode alterar propriedades diretamente ou através de métodos:
```js
p1.idade = 26
exibir(p1.idade) // 26
```

## Classes sem Construtor
Se você não definir uma função com o mesmo nome da classe, a Libra criará um objeto com os valores padrão definidos nas variáveis.

```js
classe Ponto
    var x = 0
    var y = 0
fim

var pt = Ponto()
exibir(pt.x) // 0
```

Próximo Capítulo: [FAQ](faq.md)
