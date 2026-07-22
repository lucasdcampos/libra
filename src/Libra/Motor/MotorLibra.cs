using Libra;
using Libra.Runtime;

namespace Libra.Motor;

public class MotorLibra
{
    private readonly OpcoesMotorLibra _opcoes;
    private Tokenizador _tokenizador;
    private Parser _parser;
    private Interpretador _interpretador;

    /// <summary>
    /// Inicializa uma nova instância do MotorLibra com opções padrão.
    /// Útil para cenários onde não é necessário customizar o comportamento do motor.
    /// </summary>
    public MotorLibra()
    {
        _opcoes = new OpcoesMotorLibra();
        InicializarInterpretador();
    }

    /// <summary>
    /// Inicializa uma nova instância do MotorLibra com as opções fornecidas.
    /// Permite customizar o nível de debug e o modo de execução do motor.
    /// </summary>
    /// <param name="opcoes">Opções de configuração do motor.</param>
    public MotorLibra(OpcoesMotorLibra opcoes)
    {
        _opcoes = opcoes;
        InicializarInterpretador();
    }

    private void InicializarInterpretador()
    {
        var flags = new InterpretadorFlags(_opcoes.ModoSeguro, _opcoes.ModoEstrito, true);
        _interpretador = new Interpretador(flags, logger: _opcoes.Logger);

        if (_opcoes.LerLinha != null)
            _interpretador.Ambiente.LerLinha = _opcoes.LerLinha;
    }

    public void DefinirGlobal(string identificador, object valor)
    {
        _interpretador.Ambiente.DefinirGlobal(identificador, valor);
    }

    public object ObterGlobal(string identificador)
    {
        return _interpretador.Ambiente.ObterGlobal(identificador);
    }

    public void RegistrarFuncaoNativa(string nomeNoScript, Func<object[], object> funcaoCSharp)
    {
        _interpretador.Ambiente.RegistrarFuncaoNativa(nomeNoScript, funcaoCSharp);
    }

    /// <summary>
    /// Executa um código em formato de string no ambiente do motor, utilizando o modo de execução configurado.
    /// Atualmente, apenas o modo de interpretação está implementado.
    /// </summary>
    /// <param name="codigo">Código a ser executado.</param>
    /// <returns>Resultado da execução, se houver; caso contrário, null.</returns>
    public LibraResultado Executar(string codigo, string arquivo="", string caminho="")
    {
        string textoErro = "";

        try
        {
            _interpretador.LimparSaida();
            _interpretador.Ambiente.LimparTextoSaida();
            _tokenizador = new Tokenizador(codigo, arquivo, caminho, _opcoes.CaminhosBiblioteca);
            var tokens = _tokenizador.Tokenizar();
            _parser = new Parser(tokens.ToArray(), _opcoes.ModoEstrito);
            var programa = _parser.Parse();

            _interpretador.VisitarPrograma(programa);

        }
        catch (Erro e)
        {
            if (_opcoes.ExibirErrosNoConsole)
                e.ExibirFormatado();

            textoErro = e.ToString();

            if (_opcoes.NivelDebug > NivelDebugDetalhe.Nenhum)
            {
                Console.WriteLine($"[DEBUG] StackTrace: {e.StackTrace}");
            }
        }
        catch (ExcecaoSaida)
        {
            // Não faz nada, programa foi encerrado com sucesso
        }
        catch (Exception ex)
        {
            textoErro = LogarErroInterno(ex);
        }

        object? valorSaida = null;
        try {
            valorSaida = _interpretador?.Saida?.ObterValor();
        } catch {}

        string saidaTerminal = _interpretador?.Ambiente?.TextoSaida ?? "";
        if (textoErro != "")
        {
            if (saidaTerminal != "" && !saidaTerminal.EndsWith("\n"))
                saidaTerminal += "\n";
            saidaTerminal += textoErro + "\n";

            // Embedders que capturam via logger (ex.: playground WASM) não escrevem no
            // Console; roteia o erro pelo logger para que ele apareça na saída capturada.
            if (!_opcoes.ExibirErrosNoConsole)
                _opcoes.Logger?.Msg(textoErro, "\n");
        }

        return new LibraResultado(valorSaida, saidaTerminal);
    }

    private string LogarErroInterno(Exception ex)
    {
        string logFile = "";

#if !LIBRA_WASM
        string logsDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
        logFile = Path.Combine(logsDir, $"erro-interno-{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.txt");

        try
        {
            if (!Directory.Exists(logsDir))
            {
                Directory.CreateDirectory(logsDir);
            }

            string mensagemLog = "=== ERRO INTERNO DA LIBRA ===\n";
            mensagemLog += $"Data/Hora: {DateTime.Now}\n";
            mensagemLog += $"Versão Engine: {LibraUtil.VersaoAtual()}\n";
            mensagemLog += $"Último Local Conhecido: {_interpretador?.LocalAtual}\n\n";
            mensagemLog += "EXCEÇÃO:\n";
            mensagemLog += ex.ToString();
            mensagemLog += "\n\nPor favor, reporte este erro em: https://github.com/linguagem-libra/libra/issues/";
            mensagemLog += "\nSe possível, anexe o script que causou este problema.";

            File.WriteAllText(logFile, mensagemLog);
        }
        catch (Exception logEx)
        {
            Console.WriteLine($"[CRÍTICO] Falha ao salvar log de erro: {logEx.Message}");
        }
#endif

        if (_opcoes.ExibirErrosNoConsole)
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("\n[ERRO DE SISTEMA]");
            Console.ResetColor();
            Console.WriteLine("Ocorreu um problema interno no motor da Libra.");
            Console.WriteLine("Isso não é um erro no seu código, mas sim um bug na linguagem.");
            if (logFile != "")
                Console.WriteLine($"\nUm log detalhado foi salvo em: {logFile}");
            Console.WriteLine("Por favor, ajude-nos a melhorar reportando este problema no GitHub.");
            Console.WriteLine("Link: https://github.com/linguagem-libra/libra/issues/");
            Console.WriteLine($"\nVersão: {LibraUtil.VersaoAtual()}");
            Console.WriteLine("Encerrando a execução.\n");
        }

        return "[ERRO DE SISTEMA] Ocorreu um problema interno no motor da Libra. "
             + "Isso não é um erro no seu código, mas sim um bug na linguagem. "
             + $"({ex.GetType().Name}: {ex.Message})";
    }
}
