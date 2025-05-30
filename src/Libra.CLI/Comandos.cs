using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using Libra;

public static class Comandos
{
    private static readonly Dictionary<string, Action> _comandos = new()
    {
        { "sair", () => Environment.Exit(0) },
        { "limpar", Console.Clear },
        { "licenca", MostrarLicenca },
        { "creditos", MostrarCreditos },
        { "autor", MostrarCreditos },
        { "ajuda", MostrarAjuda },
        { "versao", () => Console.WriteLine(LibraUtil.VersaoAtual()) },
        { "v", () => Console.WriteLine(LibraUtil.VersaoAtual()) },
        { "novo", () => IniciarProjeto(new List<string>()) },
        { "rodar", () => RodarProjeto(new List<string>()) }
    };

    internal static bool ExecutarComando(string comando)
    {
        if (_comandos.TryGetValue(comando, out var acao))
        {
            acao.Invoke();
            return true;
        }

        return false;
    }

    internal static bool EhComandoInterno(string comando)
    {
        return _comandos.ContainsKey(comando.ToLowerInvariant());
    }

    internal static void MostrarLicenca()
    {
        Console.WriteLine("MIT License - Copyright 2024 - 2025 Lucas M. Campos");
        Console.WriteLine("Acesse https://github.com/lucasdcampos/libra para mais detalhes");
    }

    internal static void IniciarProjeto(List<string> args)
    {
        string nome = args.Count > 0 ? args[0] : "ProjetoLibra";
        IniciarNovoProjeto.Executar(nome);
    }

    internal static void RodarProjeto(List<string> args)
    {
        RodarProjetoLibra.Executar();
    }

    internal static void MostrarCreditos()
    {
        Console.WriteLine("  Creditos à Lucas Maciel de Campos, Criador da Libra.");
        Console.WriteLine("  Roberto Fernandes de Paiva: Inventor do nome Libra.");
        Console.WriteLine("  Contribuidores: Fábio de Souza Villaça Medeiros.");
    }

    internal static void MostrarAjuda()
    {
        Console.WriteLine("Libra é uma linguagem de programação, você está no modo Interativo,");
        Console.WriteLine("ou seja, pode digitar uma instrução diretamente por aqui.");
        Console.WriteLine("Tente executar uma expressão. Exemplos: `1+1`, `2^10`, `raizq(64)`. Ou digite um comando.");
        Console.WriteLine();
        Console.WriteLine("Para interpretar um arquivo, use `nomeDoArquivo.libra`");
        Console.WriteLine("Comandos disponíveis: " + string.Join(", ", _comandos.Keys));
    }
}