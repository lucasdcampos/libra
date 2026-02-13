using Libra.Runtime;

namespace Libra.Arvore;

public class ExpressaoAcessoVetor : Expressao
{
    public ExpressaoAcessoVetor(LocalFonte local, string ident, Expressao expr)
    {
        Local = local;
        Identificador = ident;
        Expressao = expr;
    }
    public string Identificador {  get; private set; }
    public Expressao Expressao { get; private set; }

    public override LibraObjeto Aceitar(IVisitor visitor) => visitor.VisitarExpressaoAcessoVetor(this);
}