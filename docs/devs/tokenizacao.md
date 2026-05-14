# Tokenização
É o processo de transformar o código-fonte de um programa Libra em partes menores chamadas de **Tokens**.

## Token
Um Token representa o menor componente lógico da linguagem. Cada token carrega informações sobre seu tipo, sua localização no código-fonte e, opcionalmente, um valor literal.

Definição de Token na Libra: (`Token.cs`)
```cs
public class Token
{
    public Token(TokenTipo tipo, LocalFonte local, object valor = null)
    {
        Tipo = tipo;
        Valor = valor;
        Local = local;
    }

    public TokenTipo Tipo { get; private set; }
    public object Valor { get; internal set; }
    public LocalFonte Local { get; private set; }
}
```

### LocalFonte
Diferente das versões iniciais que guardavam apenas a linha, agora utilizamos a estrutura `LocalFonte`, que armazena:
- **Arquivo**: Nome do arquivo de origem.
- **Linha**: Número da linha onde o token foi encontrado.
- **CaminhoCompleto**: Caminho absoluto para facilitar a resolução de imports.

### Tipos de Token
A Libra possui uma vasta gama de tokens, incluindo literais, identificadores, operadores e palavras reservadas. Alguns exemplos:
- **Literais**: `NumeroLiteral` (suporta decimais, binários `0b` e hexadecimais `0x`), `TextoLiteral`, `CaractereLiteral`.
- **Palavras Reservadas**: `var`, `const`, `funcao`, `classe`, `se`, `enquanto`, `para cada`, `tentar`, `capturar`, `importar`, etc.
- **Operadores**: Aritméticos (`+`, `-`, `*`, `/`, `%`, `^`), Lógicos (`e`, `ou`, `nao`), Comparação (`==`, `!=`, `>`, `<`, `>=`, `<=`).

## O Tokenizador (`Tokenizador.cs`)
O Tokenizador é responsável por percorrer o código caractere por caractere e agrupar esses caracteres em Tokens válidos.

```cs
public List<Token> Tokenizar()
{
    while (Atual() != '\0')
    {
        if (char.IsDigit(Atual()))
            TokenizarNumero();
        else if (char.IsLetter(Atual()) || Atual() == '_')
            TokenizarPalavra();
        else
            TokenizarSimbolo();
    }

    AdicionarTokenALista(TokenTipo.FimDoArquivo);
    return _tokens;
}
```

### Como a Tokenização funciona?

1.  **Números**: Se o caractere for um dígito, o método `TokenizarNumero` entra em ação. Ele consegue identificar se o número é um inteiro, um número real (com ponto), um binário (iniciado por `0b`) ou um hexadecimal (iniciado por `0x`). Underscores (`_`) são ignorados para facilitar a leitura de números grandes (ex: `1_000_000`).
2.  **Palavras e Identificadores**: Se começar com uma letra ou underscore, o Tokenizador lê a palavra completa. Em seguida, verifica se essa palavra é uma **Palavra Reservada** (como `se` ou `funcao`). Se não for, ela é tratada como um **Identificador** (nome de variável ou função).
3.  **Símbolos e Comentários**: Outros caracteres são processados como símbolos (parênteses, chaves, operadores). O Tokenizador também lida com:
    - **Comentários de Linha**: Iniciados por `//`.
    - **Comentários de Bloco**: Delimitados por `/* ... */`.
    - **Strings**: Delimitadas por aspas duplas `"`.

### Tratamento de Espaços e Quebras de Linha
Espaços em branco e quebras de linha são utilizados apenas para separar tokens e não geram tokens próprios (com exceção do controle interno de incremento do número da linha em `LocalFonte`).

Essa lista de tokens gerada é então passada ao **Parser**, que organizará os tokens em uma Árvore de Sintaxe Abstrata (AST). Veja [Parsing](parsing.md) para mais informações.
