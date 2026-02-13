using Libra.Arvore;

namespace Libra.Runtime;

public class Classe : LibraObjeto, IChamavel
{
    public string Identificador { get; private set; }

    public DeclaracaoVar[] Variaveis { get; private set; }
    public DefinicaoFuncao[] Funcoes { get; private set; }

    public Classe(string ident, DeclaracaoVar[] variaveis, DefinicaoFuncao[] funcoes) : base(ident, new Variavel[0])
    {
        Identificador = ident;
        Variaveis = variaveis;
        Funcoes = funcoes;
    }

    public override LibraInt Igual(LibraObjeto outro)
    {
        return new LibraInt(0);
    }
}