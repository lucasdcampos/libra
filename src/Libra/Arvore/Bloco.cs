namespace Libra.Arvore;

public class Bloco : Instrucao
{
    public IReadOnlyList<Instrucao> Instrucoes { get; }

    public Bloco(LocalFonte local, Instrucao[] instrucoes)
    {
        Instrucoes = instrucoes;
        Local = local;
    }

    public override T Aceitar<T>(IVisitor<T> visitor)
    {
        return visitor.VisitarBloco(this);
    }
}