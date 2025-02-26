using System.Reflection;
using System.Text;
using Libra;
using Libra.Arvore;

internal static class Program
{
    private const string _ver = "1.0";
    private static string programa = "";

    internal static void Main(string[] args)
    {
        if (args.Length == 1)
        {
            Interpretar(args[0], false);

            return;
        }

        Console.WriteLine($"Bem-vindo à Libra {_ver}");
        Console.WriteLine($"Digite \"ajuda\", \"licenca\" ou uma instrução.");

        while (true)
        {
            Console.Write(">>> ");
            var linha = Console.ReadLine();

            switch (linha)
            {
                case "sair":
                    Environment.Exit(0);
                    break;
                case "limpar":
                case "clear":
                    Console.Clear();
                    break;
                case "licenca":
                    MostrarLicenca();
                    break;
                case "creditos":
                case "autor":
                    MostrarCreditos();
                    break;
                case "ajuda":
                case "help":
                    MostrarAjuda();
                    break;
                default:
                    programa += "\n" + linha;
                    new Interpretador().Interpretar(linha);
                    break;
            }
        }
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
        Console.WriteLine("Comandos disponíveis: sair, limpar, licenca, creditos, ajuda, interpretar");
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