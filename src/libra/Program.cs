internal static class Program 
{
        static Tokenizador s_tokenizador;
        static Parser s_parser;
        static GeradorC s_gerador;
    internal static void Main(string[] args)
    {
        s_tokenizador = new Tokenizador();
        s_parser = new Parser();
        s_gerador = new GeradorC();

        if(args.Length == 1)
        {


            var caminhoArquivo = args[0];
            var codigoFonte = File.ReadAllText(args[0]);
            var tokens = s_tokenizador.Tokenizar(codigoFonte);
            var programa = s_parser.Parse(tokens);
            var codigoFinal = s_gerador.Gerar(programa);

            EscreverNoArquivo(codigoFinal, caminhoArquivo);

            return;
        }

        while (true) 
        {
            Console.Write("> ");
            var linha = Console.ReadLine();

            if(linha == "sair")
            {
                break;
            }

            else if(linha == "limpar")
            {
                Console.Clear();
            }

            else
            {
                ExecutarModoImperativo(linha);
            }
        }
    }

    private static void ExecutarModoImperativo(string codigoFonte)
    {
        var tokens = s_tokenizador.Tokenizar(codigoFonte);
        var programa = s_parser.Parse(tokens);
        var codigoFinal = s_gerador.Gerar(programa);

        Console.WriteLine("Código correspondente em C:\n");
        Console.WriteLine(codigoFinal);
    }
    private static void EscreverNoArquivo(string texto, string arquivo)
    {
        try
        {
            if (File.Exists(arquivo))
            {
                File.WriteAllText(arquivo, texto);
            }
            else
            {
                using (FileStream fs = File.Create(arquivo))
                {
                    File.WriteAllText(arquivo, texto);
                }

            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
    }
}