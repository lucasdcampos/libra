using Libra.Arvore;

namespace Libra;

public class Tipo
{
    public string Identificador { get; private set; }

    public Instrucao[] Instrucoes { get; private set; }

    public Tipo(string ident, Instrucao[] instrucoes)
    {
        Identificador = ident;
        Instrucoes = instrucoes;
    }
}