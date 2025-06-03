namespace Libra.Api;

public sealed class LibraResultado
{
    public object? Valor;
    public string Saida;

    public LibraResultado(object? valor, string saida)
    {
        Valor = valor;
        Saida = saida;
    }
}