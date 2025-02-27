namespace Libra;

public class LibraTexto : LibraObjeto
{
    public string Valor { get; private set; }

    public LibraTexto(string valor)
    {
        Valor = valor;
    }

    public override string ToString()
    {
        return Valor;
    }
}