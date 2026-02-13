using Libra.Runtime;

namespace Libra.Arvore;

public class Se : Instrucao
{
    public Se(LocalFonte local, Expressao condicao, Instrucao[] corpo, SenaoSe[] listaSenaoSe = null)
    {
        Condicao = condicao ?? throw new ArgumentNullException(nameof(condicao));
        Corpo = corpo ?? throw new ArgumentNullException(nameof(corpo));
        ListaSenaoSe = listaSenaoSe;
        Local = local;
    }

    public Expressao Condicao { get; private set; }
    public IReadOnlyList<Instrucao> Corpo { get; private set; }
    public IReadOnlyList<SenaoSe> ListaSenaoSe { get; private set; }

    public override object Aceitar(IVisitor visitor)
    {
        return visitor.VisitarSe(this);
    }
}