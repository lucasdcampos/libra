namespace Libra.Arvore;

public class ExpressaoLiteral : Expressao
{
    public ExpressaoLiteral(LocalFonte local, object valor)
    {
        Local = local;
        Valor = valor;
    }

    public object Valor { get; private set; }

    public override T Aceitar<T>(IVisitor<T> visitor) => visitor.VisitarExpressaoLiteral(this);

    public static ExpressaoLiteral CriarInt(LocalFonte local, int valor)
    {
        return new ExpressaoLiteral(local, valor);
    }

    public static ExpressaoLiteral CriarTexto(LocalFonte local, string valor)
    {
        return new ExpressaoLiteral(local, valor);
    }
}