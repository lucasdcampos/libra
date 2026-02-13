using Libra.Runtime;

namespace Libra.Arvore;

public class Continuar : Instrucao
{
    public Continuar(LocalFonte local)
    {
        Local = local;
    }

    public override object Aceitar(IVisitor visitor)
    {
        return visitor.VisitarContinuar(this);
    }
}