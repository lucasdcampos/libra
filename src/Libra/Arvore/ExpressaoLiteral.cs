namespace Libra.Arvore;

public class ExpressaoLiteral : Expressao
{
    public ExpressaoLiteral(LocalFonte local, Token token)
    {
        Local = local;
        Token = token;
    }

    public object Valor => Token.Valor;
    public Token Token { get; private set; }

    public override T Aceitar<T>(IVisitor<T> visitor) => visitor.VisitarExpressaoLiteral(this);

    public static ExpressaoLiteral CriarInt(LocalFonte local, int valor)
    {
        return new ExpressaoLiteral(local, new Token(TokenTipo.NumeroLiteral, local, valor));
    }

    public static ExpressaoLiteral CriarTexto(LocalFonte local, string valor)
    {
        return new ExpressaoLiteral(local, new Token(TokenTipo.TextoLiteral, local, valor));
    }
}