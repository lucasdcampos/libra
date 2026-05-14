namespace Libra.Arvore;

public class Programa : Nodo
{
    public Instrucao[] Instrucoes;

    public Programa(Instrucao[] instrucoes)
    {
        Instrucoes = instrucoes;
    }

    public override T Aceitar<T>(IVisitor<T> visitor) => visitor.VisitarPrograma(this);
}