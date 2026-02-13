namespace Libra.Arvore;

public class Enquanto : Instrucao
{
    public Enquanto(LocalFonte local, Expressao expressao, Instrucao corpo)
    {
        Expressao = expressao;
        Corpo = corpo;
        Local = local;
    }

    public Expressao Expressao { get; private set; }
    public Instrucao Corpo {get; private set; }

    public override T Aceitar<T>(IVisitor<T> visitor)
    {
        return visitor.VisitarEnquanto(this);
    }
}