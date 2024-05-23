public static class Libra
{
    public static Dictionary<string, object> Variaveis = new Dictionary<string, object>();

    public static void Erro(string mensagem)
    {
        Console.WriteLine($"Erro: {mensagem}");
        Environment.Exit(1);
    }
}