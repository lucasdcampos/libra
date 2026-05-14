namespace Libra.Arvore;

public class Se : Instrucao
{
    public Expressao Condicao { get; private set; }
    public Instrucao Entao { get; private set; }
    public Instrucao? Senao { get; private set; }

    public Se(LocalFonte local, Expressao condicao, Instrucao entao, Instrucao? senao = null)
    {
        Condicao = condicao;
        Entao = entao;
        Senao = senao;
        Local = local;
    }

    public override T Aceitar<T>(IVisitor<T> visitor)
    {
        return visitor.VisitarSe(this);
    }
}