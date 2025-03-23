using Libra.Arvore;

namespace Libra;

public class Classe
{
    public string Identificador { get; private set; }

    public DeclaracaoVar[] Variaveis { get; private set; }
    public DefinicaoFuncao[] Funcoes { get; private set; }

    public Classe(string ident, DeclaracaoVar[] variaveis, DefinicaoFuncao[] funcoes)
    {
        Identificador = ident;
        Variaveis = variaveis;
        Funcoes = funcoes;
    }
}