namespace Libra.Arvore;

public class ExpressaoChamadaFuncao : Expressao
{
    public string Identificador { get; private set; }
    public Expressao[] Argumentos { get; private set; }

    public ExpressaoChamadaFuncao(LocalFonte local, string ident, Expressao[] argumentos = null)
    {
        Local = local;
        Identificador = ident;
        Argumentos = argumentos ?? new Expressao[0];
    }

    public override T Aceitar<T>(IVisitor<T> visitor) => visitor.VisitarExpressaoChamadaFuncao(this);
}