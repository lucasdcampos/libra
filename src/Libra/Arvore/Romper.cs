using Libra.Runtime;

namespace Libra.Arvore;

public class Romper : Instrucao
{
    public Romper(LocalFonte local)
    {
        Local = local;
    }

    public override object Aceitar(IVisitor visitor)
    {
        return visitor.VisitarRomper(this);
    }
}