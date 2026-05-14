[Documentação](README.md) -> [Bibliotecas](#)

# Bibliotecas
Uma biblioteca é um conjunto de código pré-escrito que pode ser reutilizado em diferentes projetos.
As bibliotecas facilitam o desenvolvimento, fornecendo funções, constantes e outros recursos que podem ser importados para o código, economizando tempo e esforço.

## Como importar uma biblioteca?
Na Libra, bibliotecas podem ser importadas utilizando a palavra-chave `importar` seguida pelo nome do arquivo contendo o código-fonte (em aspas) ou o nome do módulo (sem aspas, se estiver no caminho de busca).

### Aliases (Apelidos)
Você pode usar a palavra-chave `como` para dar um nome mais curto ou conveniente para um módulo importado.

**Exemplo:**
```js
importar "matematica.libra" como mat

exibir(mat.PI)
exibir(mat.raizq(64))
```

## O que acontece ao importar uma biblioteca?
Quando você importa um arquivo:
1. O conteúdo do arquivo é processado e executado.
2. Se você usou um apelido (`como`), os elementos ficam disponíveis dentro desse apelido (ex: `mat.funcao()`).
3. Se você **não** usou um apelido, os elementos são injetados diretamente no seu escopo atual.

**Exemplo sem apelido:**
```js
// arquivo: util.libra
funcao teste()
  exibir("Oi")
fim

// arquivo principal
importar "util.libra"
teste() // Chamada direta
```

## Biblioteca Padrão da Libra
A Libra vem com uma biblioteca padrão que não precisa ser instalada separadamente. Alguns módulos comuns incluem:

- **`matematica`**: Funções matemáticas avançadas.
- **`vetores`**: Utilidades para manipular listas.
- **`tempo`**: Medição de tempo e datas.
- **`sistema`**: Interação com arquivos e o sistema operacional.
- **`json`**: Processamento de dados em formato JSON.

Para mais detalhes sobre as funções de cada uma, consulte o [README da pasta biblioteca](https://github.com/linguagem-libra/libra/blob/master/biblioteca/README.md).

## Boas práticas
- **Organização**: Separe funcionalidades em arquivos diferentes.
- **Evite conflitos**: Prefira sempre usar `como` para evitar que nomes de funções da biblioteca sobrescrevam as suas.
- **Caminhos**: Scripts Libra procuram por arquivos no diretório atual, na pasta `biblioteca/` local e na pasta de instalação da linguagem.
