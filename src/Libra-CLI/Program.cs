using System.Reflection;
using System.Text;
using Libra;
using Libra.Arvore;

internal static class Program
{
    private static Tokenizador _tokenizador;
    private static Parser _parser;
    private static Interpretador _interpretador;

    private const string _ver = "1.0-PREVIEW";

    private static readonly string[] _bibliotecaPadrao = 
    {
        "Libra_CLI.so.libra",
        "Libra_CLI.matematica.libra",
        "Libra_CLI.utilidades.libra"
    };

    internal static void Main(string[] args)
    {
        _tokenizador = new Tokenizador();
        _parser = new Parser();
        _interpretador = new Interpretador();

        if (args.Length == 1)
        {
            Interpretar(args[0], true);

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

    private static string LerBibliotecaPadrao()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var sb = new StringBuilder(); 

        foreach (var str in _bibliotecaPadrao)
        {
            using (Stream stream = assembly.GetManifestResourceStream(str))
            {
                if (stream != null)
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        sb.Append(reader.ReadToEnd());
                    }
                }
                else
                {
                    Console.WriteLine($"Recurso não encontrado: {str}");
                }
            }
        }

        return sb.ToString();
    }

    
    private static Programa Interpretar(string arquivoInicial, bool incluirPadrao = false)
    {
        if (!File.Exists(arquivoInicial))
        {
            Console.WriteLine($"Não foi possível localizar `{arquivoInicial}`");
            return null;
        }

        string codigoFonte = incluirPadrao ? LerBibliotecaPadrao() : string.Empty; // carregando a biblioteca padrão no arquivo
        codigoFonte += File.ReadAllText(arquivoInicial).ReplaceLineEndings(Environment.NewLine); // Sem isso, o Tokenizador buga

        var tokens = _tokenizador.Tokenizar(codigoFonte);
        var programa = _parser.Parse(tokens);
        _interpretador.Interpretar(programa);

        return programa;
    }

}