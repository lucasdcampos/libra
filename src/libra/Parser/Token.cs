namespace Libra;

public enum TokenTipo
{
    
    // Tipos
    NumeroLiteral,
    CaractereLiteral,
    TextoLiteral,
    Vetor,
    Identificador,
    TokenInvalido,

    // Operadores
    OperadorSoma,
    OperadorSub,
    OperadorMult, 
    OperadorDiv,
    OperadorComparacao,
    OperadorDefinir,
    OperadorMaiorQue,
    OperadorMenorQue,
    OperadorMaiorIgualQue,
    OperadorMenorIgualQue,
    OperadorE,
    OperadorOu,
    OperadorOuExclusivo,
    OperadorDiferente,
    OperadorNeg,

    // Simbolos
    AbrirParen, 
    FecharParen,
    AbrirCol,
    FecharCol,
    PontoEVirgula,
    Virgula,
    NovaLinha,
    FimDoArquivo,

    // Palavras Reservadas
    Sair, 
    Var,
    Const,
    Funcao,
    Se,
    Senao,
    Entao,
    Enquanto,
    Faca,
    Romper,
    Retornar,
    Fim,
    Tipo,
    Bytes,
    Exibir
    
}

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

    public override string ToString()
    {
        if(Valor != null)
        {
            return $"Token: {Tipo} | Valor: {Valor}";
        }

        return $"Token: {Tipo}";
    }

    public static string TipoParaString(TokenTipo tipo)
    {
        switch(tipo)
        {
            case TokenTipo.PontoEVirgula:
                return "';'";
            case TokenTipo.AbrirParen:
                return "'('";
            case TokenTipo.FecharParen:
                return "')'";
            case TokenTipo.OperadorComparacao:
                return "'=='";
            case TokenTipo.OperadorDefinir:
                return "'='";
            case TokenTipo.OperadorSoma:
                return "'+'";
            case TokenTipo.OperadorSub:
                return "'-'";
            case TokenTipo.OperadorMult:
                return "'*'";
            case TokenTipo.OperadorDiv:
                return "'/'";
            case TokenTipo.Sair:
                return "sair";
            case TokenTipo.Exibir:
                return "exibir()";
            case TokenTipo.TextoLiteral:
                return "Texto";
            case TokenTipo.NumeroLiteral:
                return "Numero";
            default:
                return tipo.ToString();
        }

    }

}