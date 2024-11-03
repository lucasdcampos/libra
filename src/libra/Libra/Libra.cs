// Classe usada para ser chamada internamente pelo interpretador da Libra
// É possível chamar uma função Base usando __nomeDaFuncao__() dentro do código-fonte em Libra
using Libra;
using Libra.Arvore;

public static class LibraBase
{

    public static bool DEBUG = true;

    public static Programa ProgramaAtual;

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

    public static void EscreverInt(string i)
    {
        Console.WriteLine(int.Parse(i));
    }

    public static void Pausar(string ms)
    {
        var i = int.Parse(ms);

        Thread.Sleep(i);
    }

    public static void Erro(string codigo)
    {
        new Erro(codigo).LancarErro();
    }

    // TODO: Só uma estimativa por enquanto, o programa pega muito mais memória que isso
    // fora que, salvar variáveis como strings não é uma boa opção.
    public static void AnaliseMem()
    {
        int mem = 0;
        foreach(var v in ProgramaAtual.Variaveis.Keys)
        {
            object valor = ProgramaAtual.Variaveis[v].Valor;

            mem += sizeof(int);
        }

        LibraLogger.Log($"Memória:\nQuantidade de variáveis: {ProgramaAtual.Variaveis.Keys.Count}\nMemória Ocupada: {mem} bytes");
    }

}