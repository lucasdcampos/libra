namespace Libra;

public class Tipo
{
    public string Identificador { get; private set; }

    public Variavel[] Variaveis { get; private set; }

    public Tipo(string ident, Variavel[] variaveis)
    {
        Identificador = ident;
        Variaveis = variaveis;
    }
}