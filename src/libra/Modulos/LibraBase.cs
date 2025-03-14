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

namespace Libra.Modulos;

public class LibraBase : IModulo
{
    public  bool DEBUG = false;
    private Programa _programa;

    public void RegistrarFuncoes(Programa programa)
    {
        _programa = programa;

        // Usados em bytes(tipo) para obter a quantidade de memória em bytes usada por esses tipos
        _programa.PilhaEscopos.DefinirVariavel("int", new LibraInt(4), true);
        _programa.PilhaEscopos.DefinirVariavel("real", new LibraReal(8), true);

        _programa.Funcoes["__ativarmodulo__"] = new FuncaoNativa(__ativarmodulo__);

        _programa.Funcoes["sair"] = new FuncaoNativa(sair);
        _programa.Funcoes["exibir"] = new FuncaoNativa(exibir);
        _programa.Funcoes["tipo"] = new FuncaoNativa(tipo);
        _programa.Funcoes["tamanho"] = new FuncaoNativa(tamanho);
        _programa.Funcoes["entrada"] = new FuncaoNativa(entrada);
        _programa.Funcoes["concat"] = new FuncaoNativa(concat);
        _programa.Funcoes["pausar"] = new FuncaoNativa(pausar);
        _programa.Funcoes["real"] = new FuncaoNativa(real);
        _programa.Funcoes["int"] = new FuncaoNativa(_int);
        _programa.Funcoes["texto"] = new FuncaoNativa(texto);
        _programa.Funcoes["tentarReal"] = new FuncaoNativa(tentarReal);
        _programa.Funcoes["tentarInt"] = new FuncaoNativa(tentarInt);
        _programa.Funcoes["bytes"] = new FuncaoNativa(bytes);
        _programa.Funcoes["erro"] = new FuncaoNativa(erro);
        
        // Impedir uso de funções potencialmente perigosas
        if(Ambiente.AmbienteSeguro)
            return;

        _programa.Funcoes["registrarCSharp"] = new FuncaoNativa(registrarCSharp);
        _programa.Funcoes["registrardll"] = new FuncaoNativa(registrardll);
        _programa.Funcoes["libra"] = new FuncaoNativa(libra);

        _programa.PilhaEscopos.DefinirVariavel("NL", new LibraTexto("\n"), true);
        _programa.PilhaEscopos.DefinirVariavel("FDA", new LibraTexto("\0"), true);
    }

    public object __ativarmodulo__(object[] args)
    {
        LibraUtil.ChecarArgumentos(MethodBase.GetCurrentMethod().Name, 1, args.Length);

        if(args[0] is not string str)
            throw new ErroAcessoNulo(" Esperava Texto", Interpretador.LocalAtual);
        
        switch(str)
        {
            case "matematica":
                new LibraMatematica().RegistrarFuncoes(_programa);
                break;
        }

        return null;
    }

    public object libra(object[] args)
    {
        LibraUtil.ChecarArgumentos(MethodBase.GetCurrentMethod().Name, 1, args.Length);

        if(args[0] is not string)
            throw new Erro("Esperado um texto");

        return new Interpretador().Interpretar(args[0].ToString());

    }
    
    public object sair(object[] args)
    {
        int codigo = 0;

        if(args.Length > 0)
        {
            int.TryParse(args[0].ToString(), out int resultado);
            codigo = resultado;
            _programa.Sair(codigo);
        }

        Ambiente.Encerrar(codigo);

        return null;
    }

    public object registrardll(object[] args)
    {
        LibraUtil.ChecarArgumentos(MethodBase.GetCurrentMethod().Name, 1, args.Length);
        Assembly assembly = null;
        if(args[0] is string)
        {
            string caminhoDll = args[0].ToString();
            assembly = Assembly.LoadFrom(caminhoDll);
        }
        else if(args[0] is Assembly)
            assembly = (Assembly)args[0];
        else
            throw new Erro("Esperado um caminho para DLL", Interpretador.LocalAtual);
            
        foreach (var tipo in assembly.GetTypes())
        {
            foreach (var metodo in tipo.GetMethods(BindingFlags.Public | BindingFlags.Static))
            {
                string nomeFuncao = $"{tipo.Name}_{metodo.Name}";
                Func<object[], object> funcao = args => metodo.Invoke(null, args);
                _programa.Funcoes[nomeFuncao] = new FuncaoNativa(funcao);
            }
        }

        return null;
    }

    public object registrarCSharp(object[] args)
    {
        LibraUtil.ChecarArgumentos(MethodBase.GetCurrentMethod().Name, 3, args.Length);

        string classe = args[0].ToString();
        string nome = args[1].ToString();
        string codigo = args[2].ToString();

        // Compilar o código do usuário
        var assembly = CompilarCodigo(codigo);
        if (assembly == null) throw new Erro("Erro ao compilar o código.", Interpretador.LocalAtual);
        registrardll(new object[] {assembly});

        return null;
    }

    public object exibir(object[] args)
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

    public object concat(object[] args)
    {
        string final = "";
        for(int i = 0; i < args.Length; i++)
        {
            final += args[i].ToString();
        }

        return final;
    }

    public object texto(object[] args)
    {
        LibraUtil.ChecarArgumentos(MethodBase.GetCurrentMethod().Name, 1, args.Length);

        return args[0].ToString();
    }

    public object _int(object[] args)
    {
        if(args.Length == 0)
            new Erro("Esperava 1 argumento", Interpretador.LocalAtual);

        var r = (int?)tentarInt(args);
        if(r == null)
            throw new ErroAcessoNulo($" Não foi possível converter `{args[0]}` para Int");

        return r;
    }

    public object tentarInt(object[] args)
    {
        if(args.Length == 0)
            new Erro("Esperava 1 argumento", Interpretador.LocalAtual);

        int resultado;
        try
        {
            resultado = (int)double.Parse(args[0].ToString());
        }
        catch
        {
            return null;
        }

        return resultado;
    }

    public object tentarReal(object[] args)
    {
        if(args.Length == 0)
            new Erro("Esperava 1 argumento", Interpretador.LocalAtual);

        var r = (double?)tentarInt(args);
        if(r == null)
            throw new ErroAcessoNulo($" Não foi possível converter `{args[0]}` para Real", Interpretador.LocalAtual);

        return r;
    }

    public object real(object[] args)
    {
        if(args.Length == 0)
            new Erro("Esperava 1 argumento", Interpretador.LocalAtual);

        double resultado;
        try
        {
            resultado = double.Parse(args[0].ToString());
        }
        catch
        {
            return null;
        }

        return resultado;
    }

    public object bytes(object[] args)
    {
        LibraUtil.ChecarArgumentos(MethodBase.GetCurrentMethod().Name, 1, args.Length);

        var tamanho = LibraObjeto.ParaLibraObjeto(args[0]).ObterTamanhoEmBytes();

        if(tamanho.Valor < 0)
            throw new Erro($"Não é possível calcular diretamente o tamanho de {args[0]}", Interpretador.LocalAtual);
        
        return tamanho;
    }

    public object caractere(object[] args)
    {
        LibraUtil.ChecarArgumentos(MethodBase.GetCurrentMethod().Name, 1, args.Length);

        return (char)args[0];
    }

    public object entrada(object[] args)
    {
        return Console.ReadLine();
    }

    public object tamanho(object[] args)
    {
        LibraUtil.ChecarArgumentos(MethodBase.GetCurrentMethod().Name, 1, args.Length);

        if (args[0] is string str)
            return str.Length;

        if (args[0] is Array array)
            return array.Length;
            
        if(args[0] is LibraVetor vetor)
            return vetor.Valor.Length;

        throw new Erro("Argumento inválido para tamanho().", Interpretador.LocalAtual);

        return null;
    }


    public object pausar(object[] args)
    {
        LibraUtil.ChecarArgumentos(MethodBase.GetCurrentMethod().Name, 1, args.Length);

        int i = (int)args[0];

        Thread.Sleep(i);

        return null;
    }

    public object tipo(object[] args)
    {
        LibraUtil.ChecarArgumentos(MethodBase.GetCurrentMethod().Name, 1, args.Length);

        return LibraObjeto.ParaLibraObjeto(args[0]).ToString();
    }

    public object erro(object[] args)
    {
        if(args.Length == 0)
        {
            throw new Erro("Erro!");
        }
            
        throw new Erro(args[0].ToString(), Interpretador.LocalAtual);
    }

    private Assembly CompilarCodigo(string codigo)
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
                Ambiente.Msg("Falha ao compilar assembly: " + diagnostic.ToString());
            return null;
        }

        ms.Seek(0, SeekOrigin.Begin);
        return Assembly.Load(ms.ToArray());
    }
}