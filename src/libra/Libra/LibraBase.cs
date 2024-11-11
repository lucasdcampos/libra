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
        ProgramaAtual.Funcoes["tipo"] = new FuncaoEmbutida(tipo);
        ProgramaAtual.Funcoes["tamanho"] = new FuncaoEmbutida(tamanho);
        ProgramaAtual.Funcoes["ler_int"] = new FuncaoEmbutida(ler_int);
        ProgramaAtual.Funcoes["entrada"] = new FuncaoEmbutida(entrada);
        ProgramaAtual.Funcoes["concat"] = new FuncaoEmbutida(concat);
        ProgramaAtual.Funcoes["pausar"] = new FuncaoEmbutida(pausar);
        ProgramaAtual.Funcoes["aleatorio"] = new FuncaoEmbutida(aleatorio);
        ProgramaAtual.Funcoes["num"] = new FuncaoEmbutida(num);
        ProgramaAtual.Funcoes["int"] = new FuncaoEmbutida(_int);
        ProgramaAtual.Funcoes["caractere"] = new FuncaoEmbutida(caractere);
        ProgramaAtual.Funcoes["bytes"] = new FuncaoEmbutida(bytes);
        ProgramaAtual.Funcoes["erro"] = new FuncaoEmbutida(erro);
        ProgramaAtual.Funcoes["acessar"] = new FuncaoEmbutida(acessar);
        ProgramaAtual.Funcoes["ref"] = new FuncaoEmbutida(_ref);
    }

    public static object sair(object[] args)
    {
        int codigo = 0;

        if(args.Length > 0)
        {
            int.TryParse(args[0].ToString(), out int resultado);
            codigo = resultado;
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
        int qtd = args.Length;

        switch(qtd)
        {
            case 0:
                Console.WriteLine();
                return null;
            case 1:
                Console.WriteLine(args[0]);
                return null;
            case 2:
                Console.Write(args[0]);
                Console.Write(args[1]);
                return null;
        }

        return null;
    }

    public static object _ref(object[] args)
    {
        if(args.Length != 1)
            new Erro("Esperava 1 argumentos").LancarErro();

        string identificador = args[0].ToString();
        return ProgramaAtual.PilhaEscopos.ObterVariavel(identificador).Valor;
    }

    public static object acessar(object[] args)
    {
        if(args.Length != 2)
            new Erro("Esperava 2 argumentos").LancarErro();

        var vetor = args[0];
        int indice = (int)args[1];

        if(vetor is string)
            return vetor.ToString()[indice];
        
        // TODO: Adicionar Vetores de fato

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

    public static object _int(object[] args)
    {
        if(args.Length == 0)
            new Erro("Esperava 1 argumento");

        double n = (double)args[0];
        return (int)n;
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
        double num = 0;

        int qtd = args.Length;
        switch(qtd)
        {
            case 0:
                num = random.NextDouble();
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

    public static object tipo(object[] args)
    {
        if(args.Length != 1)
            new Erro("tipo() esperava 1 argumento").LancarErro();

        switch(args[0].GetType().ToString())
        {
            case "System.Int32":
                return "Int";
            case "System.String":
                return "Texto";
            case "System.Char":
                return "Char";
            case "System.Double":
                return "Real";
        }

        return null;
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