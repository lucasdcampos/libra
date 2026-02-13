using Libra.Runtime;

namespace Libra.Arvore;
public class ExpressaoVariavel : Expressao
{
    public Token Identificador { get; private set ;}

    public ExpressaoVariavel(LocalFonte local, Token identificador)
    {
        Local = local;
        Identificador = identificador;
    }

    public override LibraObjeto Aceitar(IVisitor visitor) => visitor.VisitarExpressaoVariavel(this);
}