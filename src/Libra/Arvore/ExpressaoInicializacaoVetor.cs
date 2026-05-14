namespace Libra.Arvore;

public class ExpressaoInicializacaoVetor : Expressao
{
    public ExpressaoInicializacaoVetor(LocalFonte local, List<Expressao> expressoes)
    {
        Local = local;
        Expressoes = expressoes;
    }

    public List<Expressao> Expressoes { get; private set; }

    public override T Aceitar<T>(IVisitor<T> visitor) => visitor.VisitarExpressaoInicializacaoVetor(this);
}