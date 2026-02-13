namespace Libra.Arvore;

public class Parametro
{
    public string Identificador;
    public string Tipo;

    public Parametro(string ident, string tipo = "Objeto")
    {
        Identificador = ident;
        Tipo = tipo;
    }
}