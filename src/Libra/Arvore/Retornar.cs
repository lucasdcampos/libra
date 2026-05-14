namespace Libra.Arvore;

public class Retornar : Instrucao
{
    public Expressao Expressao { get; private set; }

    public Retornar(LocalFonte local, Expressao expressao)
    {
        Expressao = expressao;
        Local = local;
    }

    public override T Aceitar<T>(IVisitor<T> visitor)
    {
        return visitor.VisitarRetorno(this);
    }
}