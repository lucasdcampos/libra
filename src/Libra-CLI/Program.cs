using Libra;
using Libra.Arvore;

internal static class Program
{
    private static Tokenizador _tokenizador;
    private static Parser _parser;
    private static Interpretador _interpretador;

    private const string _ver = "1.0-PREVIEW";

    internal static void Main(string[] args)
    {
        _tokenizador = new Tokenizador();
        _parser = new Parser();
        _interpretador = new Interpretador();

        if (args.Length == 1)
        {
            Interpretar(args[0]);

            return;
        }

        Console.WriteLine($"Libra Versão {_ver}");
        Console.WriteLine($"Digite 'ajuda', 'licenca' ou uma instrução\n");

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
                    Console.Clear();
                    break;
                case "licenca":
                    MostrarLicenca();
                    break;
                case "ajuda":
                    MostrarAjuda();
                    break;
                default:
                    _interpretador.Interpretar(_parser.Parse(_tokenizador.Tokenizar(linha)));
                    break;
            }
        }
    }

    private static void MostrarLicenca()
    {
        Console.WriteLine("MIT License - Copyright 2024 Lucas M. Campos");
        Console.WriteLine("Acesse https://github.com/lucasdcampos/libra para mais detalhes");
    }

    private static void MostrarAjuda()
    {
        Console.WriteLine("Comandos disponíveis: sair, limpar, licenca, ajuda, interpretar");
    }

    private static Programa Interpretar(string arquivoInicial)
    {
        if (!File.Exists(arquivoInicial))
        {
            Console.WriteLine($"Não foi possível localizar `{arquivoInicial}`");
            return null;
        }

        //var codigoFonte = File.ReadAllText("teste/base.libra").ReplaceLineEndings(Environment.NewLine); 
        var codigoFonte = File.ReadAllText(arquivoInicial).ReplaceLineEndings(Environment.NewLine); // Sem isso, o Tokenizador buga

        var tokens = _tokenizador.Tokenizar(codigoFonte);
        var programa = _parser.Parse(tokens);
        _interpretador.Interpretar(programa);

        return programa;
    }

}