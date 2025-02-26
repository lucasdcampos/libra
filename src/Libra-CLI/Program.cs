using Libra;

internal static class Program
{
    private const string _ver = "1.0";

    private static readonly Dictionary<string, Action> _comandos = new()
    {
        { "sair", () => Environment.Exit(0) },
        { "limpar", Console.Clear },
        { "licenca", MostrarLicenca },
        { "creditos", MostrarCreditos },
        { "autor", MostrarCreditos },
        { "ajuda", MostrarAjuda },
        { "versao", () => Console.WriteLine(_ver) },
    };

    internal static void Main(string[] args)
    {
        if (args.Length == 1)
        {
            string arg = args[0];
            if(arg.StartsWith("--"))
                arg = arg.Replace("--", "");

            if (ExecutarComando(arg))
            {
                Interpretar(args[0], false);
            }
            
            return;
        }

        Console.WriteLine($"Bem-vindo à Libra {_ver}");
        Console.WriteLine("Digite \"ajuda\", \"licenca\" ou uma instrução.");

        while (true)
        {
            Console.Write(">>> ");
            var linha = Console.ReadLine();

            if (!string.IsNullOrWhiteSpace(linha) && ExecutarComando(linha))
            {
                new Interpretador().Interpretar(linha);
            }
        }
    }

    private static bool ExecutarComando(string comando)
    {
        if (_comandos.TryGetValue(comando, out var acao))
        {
            acao.Invoke();
            return false;
        }
        return true; // Comando não encontrado
    }

    private static void MostrarLicenca()
    {
        Console.WriteLine("MIT License - Copyright 2024 - 2025 Lucas M. Campos");
        Console.WriteLine("Acesse https://github.com/lucasdcampos/libra para mais detalhes");
    }

    private static void MostrarCreditos()
    {
        Console.WriteLine("  Creditos à Lucas Maciel de Campos, Criador da Libra.");
        Console.WriteLine("  Roberto Fernandes de Paiva: Inventor do nome Libra.");
        Console.WriteLine("  Contribuidores: Fábio de Souza Villaça Medeiros.");
    }

    private static void MostrarAjuda()
    {
        Console.WriteLine("Comandos disponíveis: " + string.Join(", ", _comandos.Keys));
    }

    private static void Interpretar(string arquivoInicial, bool incluirPadrao = false)
    {
        bool debug = true;
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        if (!File.Exists(arquivoInicial))
        {
            Console.WriteLine($"Não foi possível localizar `{arquivoInicial}`");
            return;
        }

        string codigoFonte = File.ReadAllText(arquivoInicial).ReplaceLineEndings(Environment.NewLine); // Sem isso, o Tokenizador buga

        new Interpretador().Interpretar(codigoFonte);

        stopwatch.Stop();

        if (debug)
        {
            Console.WriteLine($"Tempo de execução: {stopwatch.ElapsedMilliseconds} ms");
        }
    }
}
