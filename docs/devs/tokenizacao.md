# Tokenização
É o processo de transformar um arquivo com código-fonte de um programa Libra em partes menores chamadas de Tokens.

## Token
Definição de Token na Libra: (`Token.cs`)
```cs
public class Token
{
    public Token(TokenTipo tipo, int linha, object valor = null)
    {
        Tipo = tipo;
        Valor = valor;
        Linha = linha;
    }

    public TokenTipo Tipo { get; private set; }
    public object Valor { get; private set; }
    public int Linha { get; private set; }
}
```
Todo Token possui um Tipo, a Linha correspondente no código-fonte e opcionalmente um Valor.

### Tipos de Token:
`NumeroLiteral, CaractereLiteral, TextoLiteral, Vetor, Identificador, Nulo, TokenInvalido, OperadorSoma, OperadorSub, OperadorMult, OperadorDiv, OperadorPot, OperadorComparacao, OperadorDefinir, OperadorMaiorQue, OperadorMenorQue, OperadorMaiorIgualQue, OperadorMenorIgualQue, OperadorE, OperadorOu, OperadorOuExclusivo, OperadorDiferente, OperadorNeg, OperadorResto, AbrirParen, FecharParen, AbrirCol, FecharCol, AbrirChave, FecharChave, PontoEVirgula, Virgula, NovaLinha, FimDoArquivo, Var, Const, Funcao, Se, Senao, SenaoSe, Entao, Enquanto, repetir, Romper, Retornar, Fim`

Alguns tokens necessitam de valor, enquanto outros não, por exemplo: Um Token de tipo `NumeroLiteral` deve possuir um valor, pois precisamos saber qual número em questão está associado a esse Token, enquanto um Token de tipo
`AbrirChave` (`{`) não.

## Tokenizando um Arquivo (`Tokenizador.cs`)
```cs
public List<Token> Tokenizar(string source) 
    {
        PreTokenizar();

        var texto = "";
        try
        {
            while (Atual() != '\0')
            {
                if(char.IsDigit(Atual()))
                {
                    TokenizarNumero();
                }

                else if(char.IsLetter(Atual()) || Atual() == '_')
                {
                    TokenizarPalavra();
                }
                else 
                {
                    TokenizarSimbolo();
                }
            }
        
            AdicionarTokenALista(TokenTipo.FimDoArquivo);

            return _tokens;
        }
        catch (Exception e)
        {
            Erro.MensagemBug(e);
        }

        return null;
    }
```
O método `Tokenizar()` recebe uma string como argumento, que corresponde ao código-fonte que se deseja tokenizar, como por exemplo `exibir("Olá, Mundo!")`. Enquanto o Tokenizador não tiver percorrido todo o arquivo
(encontrado um caractere de fim de arquivo), ele irá Tokenizar os caracteres específicos.

Antes disso, há a etapa de `pré-tokenização`, responsável por importar arquivos externos ao código fonte (Quando o usuário usa `importar "arquivo.libra"`). Isso simplesmente copia tudo que está dentro do arquivo
para o código final, não importando se a sintaxe é válida ou não.

### Como a Tokenização funciona?
Cada familia de Token será tokenizada de uma forma (números, palavras ou símbolos), mas todas seguem o mesmo princípio:

Ir percorrendo o código-fonte caractere por caractere, fazendo checagens e tentando formar Tokens. No final do método `Tokenizar()`, uma `List<Token>` foi gerada e é retornada a quem o chamou.

Essa lista então é passada ao Parser, veja [Parsing](parsing.md) para mais informações.

