namespace Libra.Arvore;

public class ExpressaoPropriedade : Expressao
{
    public Expressao Alvo { get; private set ;}
    public string Propriedade { get; private set ;}
    public ExpressaoPropriedade(LocalFonte local, Expressao alvo, string prop)
    {
        Local = local;
        Alvo = alvo;
        Propriedade = prop;
    }

    public override T Aceitar<T>(IVisitor<T> visitor) => visitor.VisitarExpressaoPropriedade(this);
}