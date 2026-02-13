using Libra.Runtime;

namespace Libra.Arvore;

public class ExpressaoNovoVetor : Expressao
{
    public ExpressaoNovoVetor(LocalFonte local, Expressao expr)
    {
        Local = local;
        Expressao = expr;
    }

    public Expressao Expressao { get; private set; }

    public override LibraObjeto Aceitar(IVisitor visitor) => visitor.VisitarExpressaoNovoVetor(this);
}