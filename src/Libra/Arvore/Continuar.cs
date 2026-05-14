namespace Libra.Arvore;

public class Continuar : Instrucao
{
    public Continuar(LocalFonte local)
    {
        Local = local;
    }

    public override T Aceitar<T>(IVisitor<T> visitor)
    {
        return visitor.VisitarContinuar(this);
    }
}