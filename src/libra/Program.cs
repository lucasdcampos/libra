using Libra.Arvore;

internal static class Program 
{
    private static Tokenizador ms_tokenizador;
    private static Parser ms_parser;
    private static GeradorC ms_gerador;
    private static Interpretador ms_interpretador;

    private const string m_ver = "1.0-PREVIEW";

    internal static void Main(string[] args)
    {
        ms_tokenizador = new Tokenizador();
        ms_parser = new Parser();
        ms_gerador = new GeradorC();
        ms_interpretador = new Interpretador();

        if(args.Length == 1)
        {
            Compilar(args[0], args[0]);
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
                    Console.WriteLine("Comandos disponíveis: sair, limpar, licenca, ajuda");
                    break;
                default:
                    if(linha.StartsWith("compilar"))
                    {
                        var cargs = linha.Split();
                        
                        if(cargs.Length == 3)
                        {
                            Compilar(cargs[1], cargs[2]);
                        }
                        else if (cargs.Length == 2)
                        {
                            Compilar(cargs[1], cargs[1]);
                        }
                        else
                        {
                            Console.WriteLine("Comando inválido! Use compilar {arquivoInicial} {arquivoFinal}");
                        }

                    }
                    else if(linha.StartsWith("interpretar"))
                    {
                        var cargs = linha.Split();
                        
                        if (cargs.Length == 2)
                        {
                            ms_interpretador.Interpretar(Interpretar(cargs[1]));
                        }
                        else
                        {
                            Console.WriteLine("Comando inválido! Use inntepretar {arquivoInicial} {arquivoFinal}");
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
        var codigoFonte = File.ReadAllText(arquivoInicial).ReplaceLineEndings(); // Sem isso, o Tokenizador buga
        var tokens = ms_tokenizador.Tokenizar(codigoFonte);
        var programa = ms_parser.Parse(tokens);
        
        return programa;
    }

    private static void Compilar(string arquivoInicial, string arquivoFinal)
    {
        var programa = Interpretar(arquivoInicial);
        var codigoFinal = ms_gerador.Gerar(programa);

        EscreverNoArquivo(codigoFinal, arquivoFinal + ".c");
    }

    private static void EscreverNoArquivo(string texto, string arquivo)
    {
        try
        {
            using (FileStream fs = new FileStream(arquivo, FileMode.Create))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.Write(texto);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
    }


}