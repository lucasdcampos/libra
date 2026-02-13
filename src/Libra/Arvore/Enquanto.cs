namespace Libra.Arvore;

public class Enquanto : Instrucao
{
    public Enquanto(LocalFonte local, Expressao expressao, Instrucao[] instrucoes)
    {
        Expressao = expressao;
        Instrucoes = instrucoes;
        Local = local;
    }

    public Expressao Expressao { get; private set; }
    public Instrucao[] Instrucoes {get; private set; }

    public override T Aceitar<T>(IVisitor<T> visitor)
    {
        return visitor.VisitarEnquanto(this);
    }
}