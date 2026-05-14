namespace Libra.Arvore;

public class ExpressaoVariavel : Expressao
{
    public Token Identificador { get; private set ;}

    public ExpressaoVariavel(LocalFonte local, Token identificador)
    {
        Local = local;
        Identificador = identificador;
    }

    public override T Aceitar<T>(IVisitor<T> visitor) => visitor.VisitarExpressaoVariavel(this);
}