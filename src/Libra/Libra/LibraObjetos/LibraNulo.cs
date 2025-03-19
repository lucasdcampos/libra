namespace Libra;

public class LibraNulo : LibraObjeto
{
    public LibraNulo()
    {
        Tipo = LibraTipo.Nulo;
    }

    public override string ToString()
    {
        return "Nulo";
    }

    public override object ObterValor()
    {
        return null;
    }

    public override LibraInt ObterTamanhoEmBytes()
    {
        return new LibraInt(0);
    }
}