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
        return "Texto";
    }

    public override object ObterValor()
    {
        return Valor;
    }

    public override LibraInt ObterTamanhoEmBytes()
    {
        return new LibraInt(Valor.Length);
    }

    public override LibraObjeto Soma(LibraObjeto outro)
    {
        return new LibraTexto(Valor + outro.ObterValor().ToString());
    }

    public override LibraInt Igual(LibraObjeto outro)
    {
        return new LibraInt(Valor == outro.ObterValor().ToString());
    }
}