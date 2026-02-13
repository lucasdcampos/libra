namespace Libra.Arvore;

public class AtribuicaoPropriedade : Instrucao
{
    public AtribuicaoPropriedade(LocalFonte local, ExpressaoPropriedade alvo, Expressao expressao)
    {
        Expressao = expressao;
        Alvo = alvo;
        Local = local;
    }

    public ExpressaoPropriedade Alvo { get; private set; }
    public Expressao Expressao { get; private set; }

    public override T Aceitar<T>(IVisitor<T> visitor)
    {
        return visitor.VisitarAtribProp(this);
    }
}