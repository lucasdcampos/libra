public static class LibraHelper
{
    public static Dictionary<string, object> Variaveis = new Dictionary<string, object>();
    public static int Linha = 1;

    public static void Erro(string mensagem)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"Erro: {mensagem} na linha {Linha}");
        
        string arrows = "";

        foreach(var c in mensagem)
            arrows += "^";

        Console.WriteLine($"      {arrows}");
        
        Console.ResetColor();

        Environment.Exit(1);
    }
}