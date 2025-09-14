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
    private string _textoSaida = "";
    public static string TextoSaida => _ambienteAtual._textoSaida;

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
        var obj = LibraObjeto.ParaLibraObjeto(valor);

        Pilha.DefinirVariavel(identificador, obj, obj.Nome);
    }

    public static object ObterGlobal(string identificador)
    {
        var variavel = Pilha.ObterVariavel(identificador);
        return variavel?.Valor ?? null;
    }

    public static void RegistrarFuncaoNativa(string nomeFuncao, Func<object[], object> funcaoCSharp)
    {
        Pilha.DefinirVariavel(nomeFuncao, new FuncaoNativa(funcaoCSharp, nomeFuncao), TiposPadrao.Func);
    }

    public static void Msg(string msg, string final = "\n")
    {
        if (_ambienteAtual == null)
            ConfigurarAmbiente(null, false);

        _ambienteAtual._textoSaida += msg + final;

        var loggerReal = Logger ?? new ConsoleLogger();
        loggerReal.Msg(msg, final);
    }

    public static void ExibirErro(Exception e)
    {
        if (e is ExcecaoSaida)
            return;
            
        if (e is Erro)
        {
            Ambiente.Msg(e.ToString());
            return;
        }
    }

    public static void Encerrar(int codigo)
    {
        throw new ExcecaoSaida(codigo);
    }
}