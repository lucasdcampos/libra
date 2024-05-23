internal static class Program 
{
    internal static void Main(string[] args)
    {
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
                Lexer lexer = new Lexer(line);
                
                Parser parser = new Parser(lexer.Tokenize());

                GeradorC gerador = new GeradorC(parser.Parse());

                Console.WriteLine("Código correspondente em C:");
                Console.WriteLine(gerador.Gerar());

            }
        }
    }
}