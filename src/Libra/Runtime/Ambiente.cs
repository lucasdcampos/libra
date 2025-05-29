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

    private PilhaDeEscopos _pilha;
    public static PilhaDeEscopos Pilha => _ambienteAtual._pilha;
    private Ambiente() { }

    public static Ambiente ConfigurarAmbiente(ILogger logger, bool seguro)
    {
        _ambienteAtual = new Ambiente();
        _ambienteAtual._pilha = new PilhaDeEscopos();
        _ambienteAtual._logger = logger ?? new ConsoleLogger();

        _ambienteAtual._ambienteSeguro = seguro;

        new LibraBase().RegistrarFuncoes();
        return _ambienteAtual;
    }

    public static void DefinirGlobal(string identificador, object valor)
    {
        Pilha.DefinirVariavel(identificador, LibraObjeto.ParaLibraObjeto(valor));
    }

    public static object ObterGlobal(string identificador)
    {
        var variavel = Pilha.ObterVariavel(identificador);
        return variavel?.Valor ?? null;
    }

    public static void RegistrarFuncaoNativa(string nomeFuncao, Func<object[], object> funcaoCSharp)
    {
        Pilha.DefinirVariavel(nomeFuncao, new FuncaoNativa(funcaoCSharp, nomeFuncao));
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