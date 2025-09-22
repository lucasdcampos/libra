namespace Libra.Motor;

public sealed class LibraResultado
{
    /// <summary>
    /// O valor retornado pela execução do programa, se houver.
    /// </summary>
    public object? Valor;
    /// <summary>
    /// Conteúdo da saída do terminal.
    /// </summary>
    public string SaidaTerminal;

    public LibraResultado(object? valor, string saidaTerminal)
    {
        Valor = valor;
        SaidaTerminal = saidaTerminal;
    }
}