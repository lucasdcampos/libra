public static class Erro
{
   public static void ErroEsperado(TokenTipo tipo, int linha = 0)
   {
        MostrarErro($"Esperado `{Token.TipoParaString(tipo)}`", linha);
   }

    public static void ErroGenerico(string mensagem, int linha = 0)
    {
        MostrarErro(mensagem, linha);
    }

   private static void MostrarErro(string mensagem, int linha)
    {
        Console.ForegroundColor = ConsoleColor.Red;

        if(linha == 0)
            Console.WriteLine($"Erro: {mensagem}");
        else
            Console.WriteLine($"Erro: {mensagem} na linha {linha}");
        
        string arrows = "";

        foreach(var c in mensagem)
            arrows += "^";

        Console.WriteLine($"      {arrows}");

        Console.ResetColor();

        Environment.Exit(1);
    }

}