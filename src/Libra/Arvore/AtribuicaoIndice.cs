namespace Libra.Arvore;

public class AtribuicaoIndice : Instrucao
{
    public AtribuicaoIndice(LocalFonte local, string identificador, Expressao indiceExpr, Expressao expressao)
    {
        Expressao = expressao;
        Identificador = identificador;
        ExpressaoIndice = indiceExpr;
        Local = local;
    }

    public string Identificador { get; private set; }
    public Expressao ExpressaoIndice { get; private set; }
    public Expressao Expressao { get; private set; }

    public override T Aceitar<T>(IVisitor<T> visitor)
    {
        return visitor.VisitarAtribIndice(this);
    }
}