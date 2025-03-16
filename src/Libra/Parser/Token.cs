namespace Libra;

public struct LocalToken
{
    public string Arquivo = "";
    public int Linha = 1;

    public LocalToken(string arquivo, int linha)
    {
        Arquivo = arquivo;
        Linha = linha;
    }

    public override string ToString()
    {
        return $"{Arquivo}:{Linha}";
    }
}

public enum TokenTipo
{
    
    // Tipos
    NumeroLiteral,
    CaractereLiteral,
    TextoLiteral,
    Identificador,
    Vetor,
    Nulo,
    TokenInvalido,

    // Operadores
    OperadorSoma,
    OperadorSub,
    OperadorMult, 
    OperadorDiv,
    OperadorPot,
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
    OperadorResto,

    // Simbolos
    AbrirParen, 
    FecharParen,
    AbrirCol,
    FecharCol,
    AbrirChave,
    FecharChave,
    PontoEVirgula,
    Virgula,
    FimDoArquivo,

    // Palavras Reservadas
    Var,
    Const,
    Funcao,
    Se,
    Senao,
    SenaoSe,
    Entao,
    Enquanto,
    Faca,
    Romper,
    Continuar,
    Retornar,
    Fim,    
}

public class Token
{
    public Token(TokenTipo tipo, LocalToken local, object valor = null)
    {
        Tipo = tipo;
        Valor = valor;
        Local = local;
    }

    public TokenTipo Tipo { get; private set; }
    public object Valor { get; internal set; }
    public LocalToken Local { get; private set; }

    public override string ToString()
    {
        if(Valor != null)
        {
            return $"Token: {Tipo} | Valor: {Valor} | Arquivo {Local.Arquivo} - Linha {Local.Linha}";
        }

        return $"Token: {Tipo}";
    }

    public static string TipoParaString(TokenTipo tipo)
    {
        // TODO: Adicionar resto dos Tokens
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
            case TokenTipo.OperadorPot:
                return "'^'";
            case TokenTipo.TextoLiteral:
                return "Texto";
            case TokenTipo.NumeroLiteral:
                return "Numero";
            default:
                return tipo.ToString();
        }
    }

}