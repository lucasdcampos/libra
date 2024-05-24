internal static class Program 
{
    internal static void Main(string[] args)
    {
        Tokenizador tokenizador;
        Parser parser;
        GeradorC gerador;

        if(args.Length == 1)
        {
            tokenizador = new Tokenizador();
            parser = new Parser();
            gerador = new GeradorC();

            var caminhoArquivo = args[0];
            var codigoFonte = File.ReadAllText(args[0]);
            var tokens = tokenizador.Tokenizar(codigoFonte);
            var programa = parser.Parse(tokens);
            var codigoFinal = gerador.Gerar(programa);

            EscreverNoArquivo(codigoFinal, caminhoArquivo);

            return;
        }

        while (true) 
        {
            Console.Write("> ");
            var line = Console.ReadLine();

            if(line == "sair")
            {
                break;
            }

            else if(line == "limpar")
            {
                Console.Clear();
            }

            else
            {

            }
        }
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