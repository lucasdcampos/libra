namespace Libra.Arvore;

public class Tentar : Instrucao
{
    public Instrucao[] InstrucoesTentar { get; private set; }
    public string VariavelErro { get; private set; }
    public Instrucao[] InstrucoesCapturar { get; private set; }

    public Tentar(LocalFonte local, Instrucao[] tentar, string variavelErro, Instrucao[] capturar)
    {
        Local = local;
        InstrucoesTentar = tentar;
        VariavelErro = variavelErro;
        InstrucoesCapturar = capturar;
    }

    public override T Aceitar<T>(IVisitor<T> visitor)
    {
        return visitor.VisitarTentar(this);
    }
}