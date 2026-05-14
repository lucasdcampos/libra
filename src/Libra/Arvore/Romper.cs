namespace Libra.Arvore;

public class Romper : Instrucao
{
    public Romper(LocalFonte local)
    {
        Local = local;
    }

    public override T Aceitar<T>(IVisitor<T> visitor)
    {
        return visitor.VisitarRomper(this);
    }
}