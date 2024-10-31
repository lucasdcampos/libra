// Classe usada para ser chamada internamente pelo interpretador da Libra
// É possível chamar uma função Base usando __nomeDaFuncao__() dentro do código-fonte em Libra
public static class LibraBase
{

    public static bool DEBUG = true;

    public static void Sair(int codigo)
    {
        if(DEBUG)
        {
            Console.WriteLine($"Código de Saída: {codigo}");
        }
        Environment.Exit(codigo);
    }

    public static string Executar(string comando, string argumentos)
    {
        System.Diagnostics.Process pProcess = new System.Diagnostics.Process();

        pProcess.StartInfo.FileName = comando;
        pProcess.StartInfo.Arguments = argumentos;
        pProcess.StartInfo.UseShellExecute = false;
        pProcess.StartInfo.CreateNoWindow = true;
        pProcess.StartInfo.RedirectStandardOutput = true;   
        pProcess.Start();

        string saida = pProcess.StandardOutput.ReadToEnd();

        pProcess.WaitForExit();

        return saida;
    }

    public static void Ping()
    {
        Console.WriteLine("Pong! Função executada pelo C#");
    }

    public static void Escrever(string caractere)
    {
        var i = int.Parse(caractere);

        Console.Write((char)i);
    }

    public static void Pausar(string ms)
    {
        var i = int.Parse(ms);

        Thread.Sleep(i);
    }

}