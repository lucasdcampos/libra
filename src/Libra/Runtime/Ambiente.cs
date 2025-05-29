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

    public static Ambiente ConfigurarAmbiente(ILogger logger, bool seguro)
    {
        _ambienteAtual = new Ambiente();

        if (logger == null)
            _ambienteAtual._logger = new ConsoleLogger();
        else
            _ambienteAtual._logger = logger;

        _ambienteAtual._ambienteSeguro = seguro;

        return _ambienteAtual;
    }

    public static void SetarPrograma(Programa programa)
    {
        _ambienteAtual._programaAtual = programa;
        new LibraBase().RegistrarFuncoes(programa);
    }

    public static void DefinirGlobal(string identificador, object valor)
    {
        if (_ambienteAtual._programaAtual == null)
            ConfigurarAmbiente(null, false);

        _ambienteAtual._programaAtual.PilhaEscopos.DefinirVariavel(identificador, LibraObjeto.ParaLibraObjeto(valor));
    }

    public static object ObterGlobal(string identificador)
    {
        if (_ambienteAtual._programaAtual == null)
            ConfigurarAmbiente(null, false);

        var variavel = _ambienteAtual._programaAtual.PilhaEscopos.ObterVariavel(identificador);
        return variavel?.Valor ?? null;
    }

    public static void RegistrarFuncaoNativa(string nomeFuncao, Func<object[], object> funcaoCSharp)
    {
        if (_ambienteAtual._programaAtual == null)
            ConfigurarAmbiente(null, false);

        _ambienteAtual._programaAtual.PilhaEscopos.DefinirVariavel(nomeFuncao, new FuncaoNativa(funcaoCSharp, nomeFuncao));
    }

    public static void Msg(string msg, string final = "\n")
    {
        if (_ambienteAtual == null)
            ConfigurarAmbiente(null, false);

        var loggerReal = Logger == null ? new ConsoleLogger() : Logger;
        loggerReal.Msg(msg, final);
    }

    public static void Encerrar(int codigo)
    {
        throw new ExcecaoSaida(codigo);
    }
}