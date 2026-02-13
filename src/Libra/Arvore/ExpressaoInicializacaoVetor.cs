using Libra.Runtime;

namespace Libra.Arvore;

public class ExpressaoInicializacaoVetor : Expressao
{
    public ExpressaoInicializacaoVetor(LocalFonte local, List<Expressao> expressoes)
    {
        Local = local;
        Expressoes = expressoes;
    }

    public List<Expressao> Expressoes { get; private set; }

    public override LibraObjeto Aceitar(IVisitor visitor) => visitor.VisitarExpressaoInicializacaoVetor(this);
}