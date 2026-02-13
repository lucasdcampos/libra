using Libra.Runtime;

namespace Libra.Arvore;

public class SenaoSe : Instrucao
{
    public SenaoSe(LocalFonte local, Expressao condicao, Instrucao[] corpo)
    {
        Condicao = condicao ?? throw new ArgumentNullException(nameof(condicao));
        Corpo = corpo ?? throw new ArgumentNullException(nameof(corpo));
        Local = local;
    }

    public Expressao Condicao { get; private set; }
    public IReadOnlyList<Instrucao> Corpo { get; private set; }

    public override object Aceitar(IVisitor visitor)
    {
        return visitor.VisitarSenaoSe(this);
    }
}