namespace Libra.Arvore;

public class ExpressaoLiteral : Expressao
{
    public ExpressaoLiteral(LocalFonte local, LibraObjeto valor)
    {
        Local = local;
        Valor = valor;
    }

    public LibraObjeto Valor { get; private set; }

    public override T Aceitar<T>(IVisitor<T> visitor) => visitor.VisitarExpressaoLiteral(this);

    public static ExpressaoLiteral CriarInt(LocalFonte local, int valor)
    {
        return new ExpressaoLiteral(local, new LibraInt(valor));
    }

    public static ExpressaoLiteral CriarTexto(LocalFonte local, string valor)
    {
        return new ExpressaoLiteral(local, new LibraTexto(valor));
    }
}