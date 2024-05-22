public enum TokenTipo
{
    TokenInvalido, Numero, PontoEVirgula, Mais, Menos, Divisao, Multiplicacao, AbrirParen, FecharParen,
    Sair, Identificador, FimDoArquivo
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