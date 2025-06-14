namespace Libra;

public class InterpretadorFlags
{
    public bool ModoSeguro { get; private set; }
    public bool ForcarTiposEstaticos { get; private set; }
    public bool MostrarAvisos { get; private set; }

    public InterpretadorFlags(bool seguro, bool tiposEstaticos, bool avisos)
    {
        ModoSeguro = seguro;
        ForcarTiposEstaticos = tiposEstaticos;
        MostrarAvisos = avisos;
    }

    public static InterpretadorFlags Padrao()
    {
        return new InterpretadorFlags(true, true, true);
    }
}