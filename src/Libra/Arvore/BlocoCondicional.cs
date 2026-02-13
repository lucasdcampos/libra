namespace Libra.Arvore;

public class BlocoCondicional
{
    public Expressao Condicao { get; }
    public Instrucao[] Corpo { get; }

    public BlocoCondicional(Expressao condicao, Instrucao[] corpo)
    {
        Condicao = condicao;
        Corpo = corpo;
    }
}
