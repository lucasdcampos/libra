using Libra;
using Libra.Arvore;
using System.Diagnostics;

internal static class Program 
{
    private static Tokenizador ms_tokenizador;
    private static Parser ms_parser;
    private static Interpretador ms_interpretador;

    private const string m_ver = "1.0-PREVIEW";

    internal static void Main(string[] args)
    {
        ms_tokenizador = new Tokenizador();
        ms_parser = new Parser();
        ms_interpretador = new Interpretador();

        if(args.Length == 1)
        {
            Interpretar(args[0]);

            return;
        }

        Console.WriteLine($"Libra Versão {m_ver}");
        Console.WriteLine($"Digite 'ajuda', 'licenca' ou uma instrução\n");

        while (true) 
        {
            Console.Write(">>> ");
            var linha = Console.ReadLine();

            switch(linha)
            {
                case "sair":
                    Environment.Exit(0);
                    break;
                case "limpar":
                    Console.Clear();
                    break;
                case "licenca":
                    Console.WriteLine("MIT License - Copyright 2024 Lucas M. Campos");
                    Console.WriteLine("Acesse https://github.com/lukazof/libra para mais detalhes");
                    break;
                case "ajuda":
                    Console.WriteLine("Comandos disponíveis: sair, limpar, licenca, ajuda, interpretar");
                    break;
                default:
                    if(linha.StartsWith("interpretar"))
                    {
                        var cargs = linha.Split();
                        
                        if (cargs.Length == 2)
                        {
                            ms_interpretador.Interpretar(Interpretar(cargs[1]));
                        }
                        else
                        {
                            Console.WriteLine("Comando inválido! Use intepretar <arquivo.lb>");
                        }
                
                    }

                    else
                    {
                        ms_interpretador.Interpretar(ms_parser.Parse(ms_tokenizador.Tokenizar(linha)));
                    }
                    
                    break;
            }
        }
    }

    private static NodoPrograma Interpretar(string arquivoInicial)
    {
        if (!File.Exists(arquivoInicial))
        {
            Console.WriteLine($"Não foi possível localizar `{arquivoInicial}`");
            return null;
        }

        var codigoFonte = File.ReadAllText(arquivoInicial).ReplaceLineEndings("\n"); // Sem isso, o Tokenizador buga
        var tokens = ms_tokenizador.Tokenizar(codigoFonte);
        var programa = ms_parser.Parse(tokens);
        ms_interpretador.Interpretar(programa);
        
        return programa;
    }

}