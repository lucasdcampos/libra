namespace Libra.Arvore;

public class InstrucaoExpressao : Instrucao
{
    public Expressao Expressao { get; private set; }

    public InstrucaoExpressao(LocalFonte local, Expressao expressao)
    {
        Expressao = expressao;
        Local = local;
    }

    public override T Aceitar<T>(IVisitor<T> visitor)
    {
        return Expressao.Aceitar(visitor);
    }
}