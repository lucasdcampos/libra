public enum TokenTipo
{
    TokenInvalido, Numero, PontoEVirgula, OperadorSoma, OperadorSub, OperadorDiv, OperadorMult, AbrirParen, FecharParen,
    Sair, Identificador, FimDoArquivo, Var, OperadorDefinir, OperadorComparacao
}

public class Token 
{
    public Token(TokenTipo tipo, object valor = null) 
    {
        Tipo = tipo;
        Valor = valor;
    }

    public TokenTipo Tipo { get; private set; }
    public object Valor { get; private set; }

    public override string ToString()
    {
        if(Valor != null)
        {
            return $"Token: {Tipo.ToString()} | Valor: {Valor}";
        }

        return $"Token: {Tipo.ToString()}";
    }

}