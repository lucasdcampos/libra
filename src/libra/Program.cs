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

                NodoPrograma programa = parser.Parse();

                foreach (NodoInstrucao i in programa.Instrucoes)
                {
                    Console.WriteLine(i.Saida.Expressao.Numero.Valor);
                }
               
            }
        }
    }
}