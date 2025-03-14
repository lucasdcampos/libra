[Documentação](README.md) -> [Bibliotecas](#)

# Bibliotecas
Uma biblioteca é um conjunto de código pré-escrito que pode ser reutilizado em diferentes projetos.
As bibliotecas facilitam o desenvolvimento, fornecendo funções, constantes e outros recursos que podem ser importados para o código, economizando tempo e esforço.

## Como importar uma biblioteca?
Na Libra, bibliotecas podem ser importadas utilizando a palavra-chave `importar` seguida pelo nome do arquivo contendo o código-fonte.

## O que acontece ao importar uma biblioteca?
Quando você importa um arquivo:

O conteúdo do arquivo é "copiado" para dentro do programa no ponto onde o comando importar foi escrito.
Todas as funções, variáveis e outros elementos definidos no arquivo importado ficam disponíveis para uso.

**Exemplo:**
Vamos supor que você tenha um arquivo `arquivo2.libra`, com o seguinte código:
```js
funcao exemplo()
  exibir("Função do Arquivo 2")
fim
```
No seu arquivo principal, você pode fazer o seguinte:
```js
importar "arquivo2.libra"

exibir("Estou no arquivo 1")
exemplo() // Chamando uma função definida em "arquivo2.libra"
```

## Boas práticas ao usar bibliotecas
**Organização:** Separe funcionalidades em arquivos diferentes para melhorar a legibilidade e manutenção. <br>
**Evite conflitos:** Certifique-se de que os nomes de funções e variáveis não entrem em conflito entre o código principal e as bibliotecas. <br>
**Importe apenas o necessário:** Evite carregar bibliotecas que não serão usadas no programa. <br>

## Biblioteca Padrão da Libra
Na maioria das instalações, a Libra vem com uma biblioteca padrão com diversas funções que você pode usar, como funções matemáticas, funções para interagir com o sistema operacional, etc.

Para mais informações, veja https://github.com/lucasdcampos/libra/blob/master/biblioteca/README.md
