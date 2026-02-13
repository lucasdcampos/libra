namespace Libra.Arvore;

public class ExpressaoUnaria : Expressao
{
    public Token Operador { get; private set; }
    public Expressao Operando { get; private set; }

    public ExpressaoUnaria(LocalFonte local, Token operador, Expressao operando)
    {
        Local = local;
        Operador = operador;
        Operando = operando;
    }
    public override T Aceitar<T>(IVisitor<T> visitor) => visitor.VisitarExpressaoUnaria(this);
}