namespace Libra.Arvore;

public class ParaCada : Instrucao
{
    public ParaCada(LocalFonte local, Token ident, Expressao vetor, Instrucao[] instrucoes)
    {
        Identificador = ident;
        Instrucoes = instrucoes;
        Local = local;
        Vetor = vetor;
    }

    public Token Identificador { get; private set; }
    public Instrucao[] Instrucoes {get; private set; }
    public Expressao Vetor { get; private set; }

    public override T Aceitar<T>(IVisitor<T> visitor)
    {
        return visitor.VisitarParaCada(this);
    }
}