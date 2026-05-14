namespace Libra.Arvore;

public class ExpressaoNovoVetor : Expressao
{
    public ExpressaoNovoVetor(LocalFonte local, Expressao expr)
    {
        Local = local;
        Expressao = expr;
    }

    public Expressao Expressao { get; private set; }

    public override T Aceitar<T>(IVisitor<T> visitor) => visitor.VisitarExpressaoNovoVetor(this);
}