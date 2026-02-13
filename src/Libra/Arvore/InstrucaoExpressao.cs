using Libra.Runtime;

namespace Libra.Arvore;

public class InstrucaoExpressao : Instrucao
{
    public Expressao Expressao { get; private set; }

    public InstrucaoExpressao(LocalFonte local, Expressao expressao)
    {
        Expressao = expressao;
        Local = local;
    }

    public override object Aceitar(IVisitor visitor)
    {
        return Expressao.Aceitar(visitor);
    }
}