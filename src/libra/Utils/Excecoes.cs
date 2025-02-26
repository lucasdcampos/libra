namespace Libra;

public class ExcecaoRetorno : Exception
{
    public object Valor { get; private set; }

    public ExcecaoRetorno(object valor)
    {
        Valor = valor;
    }
}

public class ExcecaoSaida : Exception
{
    public int Codigo { get; private set; }

    public ExcecaoSaida(int codigo)
    {
        Codigo = codigo;
    }
}