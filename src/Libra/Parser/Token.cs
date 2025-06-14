namespace Libra;

public struct LocalFonte
{
    public string Arquivo = "";
    public int Linha = 1;
    public string CaminhoCompleto = "";
    public LocalFonte(string caminho, string arquivo, int linha)
    {
        Arquivo = arquivo;
        Linha = linha;
        CaminhoCompleto = caminho;
    }

    public override string ToString()
    {
        return $"{Arquivo}:{Linha}";
    }
}

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