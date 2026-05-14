namespace Libra.Arvore;

public class ExpressaoChamadaMetodo : Expressao
{
    public Expressao Alvo { get; private set; }
    public ExpressaoChamadaFuncao Chamada { get; private set ;}

    public ExpressaoChamadaMetodo(LocalFonte local, Expressao alvo, ExpressaoChamadaFuncao chamada)
    {
        Local = local;
        Alvo = alvo;
        Chamada = chamada;
    }

    public override T Aceitar<T>(IVisitor<T> visitor) => visitor.VisitarExpressaoChamadaMetodo(this);
}