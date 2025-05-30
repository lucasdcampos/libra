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

    public static void ExibirErro(Exception e)
    {
        if (e is ExcecaoSaida)
            return;
            
        if (e is Erro)
        {
            Ambiente.Msg(e.ToString());
            return;
        }

        string logsDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
        string logFile = Path.Combine(logsDir, $"erro-{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.txt");

        if (!Directory.Exists(logsDir))
        {
            Directory.CreateDirectory(logsDir);
        }

        string mensagemLog = "Ocorreu um erro interno na Libra, veja a descrição para mais detalhes:\n";
        mensagemLog += "Versão: Libra 1.0.0-Beta\n";
        mensagemLog += $"Ultima local do Script Libra executada: {Interpretador.LocalAtual}\n";
        mensagemLog += $"Problema:\n{e.ToString()}\n";
        mensagemLog += "Por favor reportar em https://github.com/lucasdcampos/libra/issues/ (se possível incluir script que causou o problema)\n";

        File.WriteAllText(logFile, mensagemLog);

        Ambiente.Msg("\nHouve um problema, mas não foi culpa sua :(");
        Ambiente.Msg($"Uma descrição do erro foi salva em: {logFile}");
        Ambiente.Msg("Por favor reportar em https://github.com/lucasdcampos/libra/issues/");
        Ambiente.Msg($"Versão: Libra {LibraUtil.VersaoAtual()}"); // TODO: Não deixar a versão hardcoded dessa forma
        Ambiente.Msg("\nImpossível continuar, encerrando a execução do programa.\n");
    }

    public static void Encerrar(int codigo)
    {
        throw new ExcecaoSaida(codigo);
    }
}