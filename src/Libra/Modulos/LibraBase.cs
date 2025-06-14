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

    public void RegistrarFuncoes(Programa programa = null)
    {
        Ambiente.DefinirGlobal("Nulo", new LibraNulo());

        Ambiente.DefinirGlobal("__ativarmodulo__", new FuncaoNativa(__ativarmodulo__));

        Ambiente.DefinirGlobal("sair", new FuncaoNativa(sair));
        Ambiente.DefinirGlobal("exibir", new FuncaoNativa(exibir));
        Ambiente.DefinirGlobal("tipo", new FuncaoNativa(tipo));
        Ambiente.DefinirGlobal("garantir", new FuncaoNativa(garantir));
        Ambiente.DefinirGlobal("tamanho", new FuncaoNativa(tamanho));
        Ambiente.DefinirGlobal("concat", new FuncaoNativa(concat));
        Ambiente.DefinirGlobal("pausar", new FuncaoNativa(pausar));
        Ambiente.DefinirGlobal("real", new FuncaoNativa(real));
        Ambiente.DefinirGlobal("int", new FuncaoNativa(_int));
        Ambiente.DefinirGlobal("texto", new FuncaoNativa(texto));
        Ambiente.DefinirGlobal("tentarReal", new FuncaoNativa(tentarReal));
        Ambiente.DefinirGlobal("tentarInt", new FuncaoNativa(tentarInt));
        Ambiente.DefinirGlobal("bytes", new FuncaoNativa(bytes));
        Ambiente.DefinirGlobal("erro", new FuncaoNativa(erro));

        // Impedir uso de funções potencialmente perigosas
        if (Ambiente.AmbienteSeguro /* TODO: Arrumar! || Interpretador.Flags.ModoSeguro*/)
            return;

        Ambiente.DefinirGlobal("entrada", new FuncaoNativa(entrada));
        Ambiente.DefinirGlobal("registrarCSharp", new FuncaoNativa(registrarCSharp));
        Ambiente.DefinirGlobal("registrardll", new FuncaoNativa(registrardll));
        Ambiente.DefinirGlobal("libra", new FuncaoNativa(libra));

        Ambiente.DefinirGlobal("NL", new LibraTexto("\n"));
        Ambiente.DefinirGlobal("FDA", new LibraTexto("\0"));
    }

    public object __ativarmodulo__(object[] args)
    {
        LibraUtil.ChecarArgumentos(MethodBase.GetCurrentMethod().Name, 1, args.Length);

        if (args[0] is not string str)
            throw new ErroAcessoNulo(" Esperava Texto", new LocalFonte() /* TODO: Arrumar! */);

        switch (str)
        {
            case "matematica":
                new LibraMatematica().RegistrarFuncoes(_programa);
                break;
            case "sistema":
                new LibraSistema().RegistrarFuncoes(_programa);
                break;
            case "json":
                new LibraJson().RegistrarFuncoes(_programa);
                break;
        }

        return null;
    }

    // TODO: Implementar
    public object libra(object[] args)
    {
        LibraUtil.ChecarArgumentos(MethodBase.GetCurrentMethod().Name, 1, args.Length);

        if (args[0] is not string)
            throw new Erro("Esperado um texto");

        //return new Interpretador().Interpretar(args[0].ToString());

        return null;
    }

    public object garantir(object[] args)
    {
        if(args[0] is not int valor)
            throw new Erro("Esperado uma condição");

        string msgErro = "";

        if(args.Length > 1)
            if(args[1] is string mensagem)
                msgErro = mensagem;

        if(valor == 0)
            throw new Erro(msgErro);
        
        return null;
    }
    
    public object sair(object[] args)
    {
        int codigo = 0;

        if(args.Length > 0)
        {
            int.TryParse(args[0].ToString(), out int resultado);
            codigo = resultado;
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
            throw new Erro("Esperado um caminho para DLL", new LocalFonte() /* TODO: Arrumar! */);
            
        foreach (var tipo in assembly.GetTypes())
        {
            foreach (var metodo in tipo.GetMethods(BindingFlags.Public | BindingFlags.Static))
            {
                string nomeFuncao = $"{tipo.Name}_{metodo.Name}";
                Func<object[], object> funcao = args => metodo.Invoke(null, args);
                Ambiente.DefinirGlobal(nomeFuncao, new FuncaoNativa(funcao));
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
        if (assembly == null) throw new Erro("Erro ao compilar o código.", new LocalFonte() /* TODO: Arrumar! */);
        registrardll(new object[] {assembly});

        return null;
    }

    public object exibir(object[] args)
    {
        int qtd = args.Length;
        
        for(int i = 0; i < args.Length; i++)
        {
            if(args[i] == null)
                return null;
        }

        switch(qtd)
        {
            case 0:
                Ambiente.Msg($"{args[0]}");
                break;
            case 1:
                Ambiente.Msg($"{args[0]}");
                break;
            case 2:
                Ambiente.Msg(args[0].ToString(), args[1].ToString());
                break;
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
            new Erro("Esperava 1 argumento", new LocalFonte() /* TODO: Arrumar! */);

        var r = (int?)tentarInt(args);
        if(r == null)
            throw new ErroAcessoNulo($" Não foi possível converter `{args[0]}` para Int");

        return r;
    }

    public object tentarInt(object[] args)
    {
        if(args.Length == 0)
            new Erro("Esperava 1 argumento", new LocalFonte() /* TODO: Arrumar! */);

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
            new Erro("Esperava 1 argumento", new LocalFonte() /* TODO: Arrumar! */);

        var r = (double?)tentarInt(args);
        if(r == null)
            throw new ErroAcessoNulo($" Não foi possível converter `{args[0]}` para Real", new LocalFonte() /* TODO: Arrumar! */);

        return r;
    }

    public object real(object[] args)
    {
        if(args.Length == 0)
            new Erro("Esperava 1 argumento", new LocalFonte() /* TODO: Arrumar! */);

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
            throw new Erro($"Não é possível calcular diretamente o tamanho de {args[0]}", new LocalFonte() /* TODO: Arrumar! */);
        
        return tamanho;
    }

    public object caractere(object[] args)
    {
        LibraUtil.ChecarArgumentos(MethodBase.GetCurrentMethod().Name, 1, args.Length);

        return (char)args[0];
    }

    public object entrada(object[] args)
    {
        if (true /*TODO: Arrumar! Interpretador.Flags.ModoSeguro*/)
        {
            throw new ErroAcessoNulo(" entrada não disponível nesse modo");
        }

        exibir(args);
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

        throw new Erro("Argumento inválido para tamanho().", new LocalFonte() /* TODO: Arrumar! */);

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

        var nome = LibraObjeto.ParaLibraObjeto(args[0]).Nome;

        if (nome == TiposPadrao.Nulo)
        {
            throw new ErroAcessoNulo();
        }

        return nome;
    }

    public object erro(object[] args)
    {
        if(args.Length == 0)
        {
            throw new Erro("Erro!");
        }
        throw new Erro(args[0].ToString(), new LocalFonte() /* TODO: Arrumar! */);
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