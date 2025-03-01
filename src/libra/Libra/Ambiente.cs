using Libra.Arvore;
using Libra.Modulos;

namespace Libra;

public class Ambiente
{
    private static Ambiente _ambienteAtual;
    private ILogger _logger;
    public static ILogger Logger => _ambienteAtual._logger;
    public static bool AmbienteSeguro => _ambienteAtual._ambienteSeguro;
    public bool _ambienteSeguro; 
    private Programa _programaAtual;
    public static Programa ProgramaAtual => _ambienteAtual._programaAtual;

    private Ambiente() { }

    public static void ConfigurarAmbiente(ILogger logger, bool seguro)
    {
        _ambienteAtual = new Ambiente();

        if(logger == null)
            _ambienteAtual._logger = new ConsoleLogger();
        else
            _ambienteAtual._logger = logger;
        
        _ambienteAtual._ambienteSeguro = seguro;
    }

    public static void SetarPrograma(Programa programa)
    {
        _ambienteAtual._programaAtual = programa;
        new LibraBase().RegistrarFuncoes(programa);
    }

    public static void Msg(string msg, string final = "\n")
    {
        if(_ambienteAtual == null)
            ConfigurarAmbiente(null, false);

        var loggerReal = Logger == null ? new ConsoleLogger() : Logger;
        loggerReal.Msg(msg, final);
    }

    public static void Encerrar(int codigo)
    {
        throw new ExcecaoSaida(codigo);
    }
}