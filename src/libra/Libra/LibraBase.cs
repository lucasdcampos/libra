// Classe usada para ser chamada internamente pelo interpretador da Libra
// Contém a lista de funções embutidas
using Libra;
using Libra.Arvore;
using System;
using System.Reflection;
using System.Collections.Generic;
using System.IO;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

public static class LibraBase
{
    public static bool DEBUG = false;
    private static Programa _programaAtual => Ambiente.ProgramaAtual;
    public static void RegistrarFuncoesEmbutidas()
    {
        // Usados em bytes(tipo) para obter a quantidade de memória em bytes usada por esses tipos
        _programaAtual.PilhaEscopos.DefinirVariavel("int", new LibraInt(4), true);
        _programaAtual.PilhaEscopos.DefinirVariavel("real", new LibraReal(8), true);

        _programaAtual.Funcoes["sair"] = new FuncaoEmbutida(sair);
        _programaAtual.Funcoes["exibir"] = new FuncaoEmbutida(exibir);
        _programaAtual.Funcoes["tipo"] = new FuncaoEmbutida(tipo);
        _programaAtual.Funcoes["tamanho"] = new FuncaoEmbutida(tamanho);
        _programaAtual.Funcoes["ler_int"] = new FuncaoEmbutida(ler_int);
        _programaAtual.Funcoes["entrada"] = new FuncaoEmbutida(entrada);
        _programaAtual.Funcoes["concat"] = new FuncaoEmbutida(concat);
        _programaAtual.Funcoes["pausar"] = new FuncaoEmbutida(pausar);
        _programaAtual.Funcoes["aleatorio"] = new FuncaoEmbutida(aleatorio);
        _programaAtual.Funcoes["real"] = new FuncaoEmbutida(real);
        _programaAtual.Funcoes["int"] = new FuncaoEmbutida(_int);
        _programaAtual.Funcoes["texto"] = new FuncaoEmbutida(texto);
        _programaAtual.Funcoes["bytes"] = new FuncaoEmbutida(bytes);
        _programaAtual.Funcoes["erro"] = new FuncaoEmbutida(erro);
        
        // Impedir uso de funções potencialmente perigosas
        if(Ambiente.AmbienteSeguro)
            return;

        _programaAtual.Funcoes["registrarCSharp"] = new FuncaoEmbutida(registrarCSharp);
        _programaAtual.Funcoes["registrardll"] = new FuncaoEmbutida(registrardll);
        _programaAtual.Funcoes["libra"] = new FuncaoEmbutida(libra);
    }

    public static object libra(object[] args)
    {
        ChecarArgumentos(MethodBase.GetCurrentMethod().Name, 1, args.Length);

        if(args[0] is not string)
            throw new Erro("Esperado um texto");

        return new Interpretador().Interpretar(args[0].ToString());

    }
    
    public static object sair(object[] args)
    {
        int codigo = 0;

        if(args.Length > 0)
        {
            int.TryParse(args[0].ToString(), out int resultado);
            codigo = resultado;
            _programaAtual.Sair(codigo);
        }

        if(DEBUG)
        {
            Console.WriteLine($"Código de Saída: {codigo}");
        }

        Ambiente.Encerrar(codigo);

        return null;
    }

    public static object registrardll(object[] args)
    {
        ChecarArgumentos(MethodBase.GetCurrentMethod().Name, 1, args.Length);
        Assembly assembly = null;
        if(args[0] is string)
        {
            string caminhoDll = args[0].ToString();
            assembly = Assembly.LoadFrom(caminhoDll);
        }
        else if(args[0] is Assembly)
            assembly = (Assembly)args[0];
        else
            throw new Erro("Esperado um caminho para DLL");
            
        foreach (var tipo in assembly.GetTypes())
        {
            foreach (var metodo in tipo.GetMethods(BindingFlags.Public | BindingFlags.Static))
            {
                string nomeFuncao = $"{tipo.Name}_{metodo.Name}";
                Func<object[], object> funcao = args => metodo.Invoke(null, args);
                _programaAtual.Funcoes[nomeFuncao] = new FuncaoEmbutida(funcao);
            }
        }

        return null;
    }

    public static object registrarCSharp(object[] args)
    {
        ChecarArgumentos(MethodBase.GetCurrentMethod().Name, 3, args.Length);

        string classe = args[0].ToString();
        string nome = args[1].ToString();
        string codigo = args[2].ToString();

        // Compilar o código do usuário
        var assembly = CompilarCodigo(codigo);
        if (assembly == null) throw new Erro("Erro ao compilar o código.");
        registrardll(new object[] {assembly});

        return null;
    }

    public static object exibir(object[] args)
    {
        int qtd = args.Length;
        
        switch(qtd)
        {
            case 0:
                Ambiente.Msg($"{args[0]}");
                return null;
            case 1:
                args[0] = args[0] == null ? "Nulo" : args[0];
                Ambiente.Msg($"{args[0]}");
                return null;
            case 2:
                args[0] = args[0] == null ? "Nulo" : args[0];
                Ambiente.Msg(args[0].ToString(), args[1].ToString());
                return null;
        }

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

    public static object texto(object[] args)
    {
        ChecarArgumentos(MethodBase.GetCurrentMethod().Name, 1, args.Length);

        return args[0].ToString();
    }

    public static object _int(object[] args)
    {
        if(args.Length == 0)
            new Erro("Esperava 1 argumento");

        int? resultado = null;
        try
        {
            resultado = (int)double.Parse(args[0].ToString());
        }
        catch
        {
            Ambiente.Msg($"Não foi possível converter {args[0]} para Int");
        }

        return resultado;
    }

    public static object real(object[] args)
    {
        if(args.Length == 0)
            new Erro("Esperava 1 argumento");

        double? resultado = null;
        try
        {
            resultado = double.Parse(args[0].ToString());
        }
        catch
        {
            Ambiente.Msg($"Não foi possível converter {args[0]} para Real");
        }

        return resultado;
    }

    public static object bytes(object[] args)
    {
        ChecarArgumentos(MethodBase.GetCurrentMethod().Name, 1, args.Length);

        var tamanho = LibraObjeto.ParaLibraObjeto(args[0]).ObterTamanhoEmBytes();

        if(tamanho.Valor < 0)
            throw new Erro($"Não é possível calcular diretamente o tamanho de {args[0]}");
        
        return tamanho;
    }

    public static object caractere(object[] args)
    {
        ChecarArgumentos(MethodBase.GetCurrentMethod().Name, 1, args.Length);

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
        ChecarArgumentos(MethodBase.GetCurrentMethod().Name, 1, args.Length);

        if (args[0] is string str)
            return str.Length;

        if (args[0] is Array array)
            return array.Length;
            
        if(args[0] is LibraVetor vetor)
            return vetor.Valor.Length;

        throw new ArgumentException("Argumento inválido para length.");

        return null;
    }


    public static object pausar(object[] args)
    {
        ChecarArgumentos(MethodBase.GetCurrentMethod().Name, 1, args.Length);

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
        ChecarArgumentos(MethodBase.GetCurrentMethod().Name, 1, args.Length);

        return LibraObjeto.ParaLibraObjeto(args[0]).ToString();
    }

    public static object erro(object[] args)
    {
        if(args.Length == 0)
        {
            throw new Erro("Erro!");
        }
            
        throw new Erro(args[0].ToString());
    }

    private static Assembly CompilarCodigo(string codigo)
    {
        var tree = CSharpSyntaxTree.ParseText(codigo);
        var compilation = CSharpCompilation.Create(
            "LibraExterna.dll",
            new[] { tree },
            new[]
            {
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Console).Assembly.Location)
            },
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
        );

        using var ms = new MemoryStream();
        var result = compilation.Emit(ms);

        if (!result.Success)
        {
            foreach (var diagnostic in result.Diagnostics)
                Ambiente.Msg(diagnostic.ToString());
            return null;
        }

        ms.Seek(0, SeekOrigin.Begin);
        return Assembly.Load(ms.ToArray());
    }

    private static void ChecarArgumentos(string ident, int esperado, int recebido)
    {
        if(esperado != recebido)
            throw new ErroEsperadoNArgumentos(ident, esperado, recebido);
    }

}