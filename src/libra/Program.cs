internal static class Program 
{
    private static Tokenizador ms_tokenizador;
    private static Parser ms_parser;
    private static GeradorC ms_gerador;
    internal static void Main(string[] args)
    {
        ms_tokenizador = new Tokenizador();
        ms_parser = new Parser();
        ms_gerador = new GeradorC();

        if(args.Length == 1)
        {
            var caminhoArquivo = args[0];
            var codigoFonte = File.ReadAllText(args[0]);
            var tokens = ms_tokenizador.Tokenizar(codigoFonte);
            var programa = ms_parser.Parse(tokens);
            var codigoFinal = ms_gerador.Gerar(programa);

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
        var tokens = ms_tokenizador.Tokenizar(codigoFonte);
        var programa = ms_parser.Parse(tokens);
        var codigoFinal = ms_gerador.Gerar(programa);

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