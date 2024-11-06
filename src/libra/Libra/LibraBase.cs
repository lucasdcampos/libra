// Classe usada para ser chamada internamente pelo interpretador da Libra
// Contém a lista de funções embutidas
using Libra;
using Libra.Arvore;

public static class LibraBase
{
    public static bool DEBUG = true;

    public static Programa ProgramaAtual;

    public static void RegistrarFuncoesEmbutidas()
    {
        ProgramaAtual.Funcoes["ping"] = new FuncaoEmbutida(ping);
        ProgramaAtual.Funcoes["sair"] = new FuncaoEmbutida(sair);
        ProgramaAtual.Funcoes["exibir"] = new FuncaoEmbutida(exibir);
        ProgramaAtual.Funcoes["exibirln"] = new FuncaoEmbutida(exibirln);
        ProgramaAtual.Funcoes["tamanho"] = new FuncaoEmbutida(tamanho);
        ProgramaAtual.Funcoes["ler_int"] = new FuncaoEmbutida(ler_int);
        ProgramaAtual.Funcoes["entrada"] = new FuncaoEmbutida(entrada);
        ProgramaAtual.Funcoes["concat"] = new FuncaoEmbutida(concat);
        ProgramaAtual.Funcoes["pausar"] = new FuncaoEmbutida(pausar);
        ProgramaAtual.Funcoes["aleatorio"] = new FuncaoEmbutida(aleatorio);
        ProgramaAtual.Funcoes["num"] = new FuncaoEmbutida(num);
        ProgramaAtual.Funcoes["caractere"] = new FuncaoEmbutida(caractere);
        ProgramaAtual.Funcoes["bytes"] = new FuncaoEmbutida(bytes);
        ProgramaAtual.Funcoes["erro"] = new FuncaoEmbutida(erro);
    }

    public static object sair(object[] args)
    {
        int codigo = 0;

        if(args.Length > 0)
        {
            codigo = (int)args[0];
        }

        if(DEBUG)
        {
            Console.WriteLine($"Código de Saída: {codigo}");
        }
        Environment.Exit(codigo);

        return null;
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

    // Apenas para testar
    public static object[] ping(object[] args)
    {
        Console.WriteLine("Pong! Função executada pelo C#.");

        if(args.Length > 0)
        {
            Console.Write("Seus argumentos: ");
            foreach(var arg in args)
                Console.Write(arg.ToString());
        }
        return null;
    }

    public static object exibir(object[] args)
    {
        for(int i = 0; i < args.Length; i++)
        {
            Console.Write(args[i].ToString());
        }

        return null;
    }

    public static object exibirln(object[] args)
    {
        for(int i = 0; i < args.Length; i++)
        {
            Console.Write(args[i].ToString());
        }

        Console.WriteLine();
        return null;
    }

    public static object concat(object[] args)
    {
        string final = "";
        for(int i = 0; i < args.Length; i++)
        {
            final += args[i].ToString();
        }

        return final;
    }

    public static object num(object[] args)
    {
        if(args.Length == 0)
            new Erro("Esperava 1 argumento");

        return int.Parse(args[0].ToString());
    }

    public static object bytes(object[] args)
    {
        if(args.Length == 0)
            new Erro("Esperava 1 argumento");

        var objeto = args[0];

        if (objeto is int)
            return sizeof(int);
        
        if (objeto is string)
            return sizeof(char) * objeto.ToString().Length;

        return 0;
    }

    public static object caractere(object[] args)
    {
        if(args.Length == 0)
            new Erro("Esperava 1 argumento");

        return (char)args[0];
    }

    public static object ler_int(object[] args)
    {
        return int.Parse(Console.ReadLine());
    }

    public static object entrada(object[] args)
    {
        return Console.ReadLine();
    }

    public static object tamanho(object[] args)
    {
        if(args.Length == 0)
            return null;

        if (args.Length != 1)
                throw new ArgumentException("length espera 1 argumento.");

        if (args[0] is string str)
            return str.Length;

        if (args[0] is Array array)
            return array.Length;

        throw new ArgumentException("Argumento inválido para length.");

        return null;
    }


    public static object pausar(object[] args)
    {
        if(args.Length == 0)
            new Erro("Argumentos insuficientes");

        int i = (int)args[0];

        Thread.Sleep(i);

        return null;
    }

    public static object aleatorio(object[] args)
    {
        Random random = new Random();
        int num = 0;

        int qtd = args.Length;
        switch(qtd)
        {
            case 0:
                num = random.Next();
                break;
            case 1:
                num = random.Next((int)args[0]);
                break;
            case 2:
                num = random.Next((int)args[0], (int)args[1]);
                break;
        }
        
        return num;
    }

    public static void tipo(string identificador)
    {
        Console.WriteLine(identificador);

        var tipo = ProgramaAtual.Variaveis[identificador].Tipo;

        switch (tipo)
        {
            case TokenTipo.NumeroLiteral: Console.WriteLine("Numero"); break;
            case TokenTipo.CaractereLiteral: Console.WriteLine("Caractere"); break;
            case TokenTipo.Vetor: Console.WriteLine("Vetor"); break;
        }
    }

    public static object erro(object[] args)
    {
        if(args.Length == 0)
        {
            new Erro("Erro!").LancarErro();
        }
            
        new Erro(args[0].ToString()).LancarErro();

        return null;
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