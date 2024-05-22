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

            else
            {
                Lexer lexer = new Lexer(line);
                
                Parser parser = new Parser(lexer.Tokenize());

                NodoPrograma programa = parser.Parse();

                Console.WriteLine(programa.ExpressaoSaida.Numero.Valor);
            }
        }
    }
}