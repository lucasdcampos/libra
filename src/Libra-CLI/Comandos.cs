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

    private static void MostrarLicenca()
    {
        Console.WriteLine("MIT License - Copyright 2024 - 2025 Lucas M. Campos");
        Console.WriteLine("Acesse https://github.com/lucasdcampos/libra para mais detalhes");
    }

    private static void MostrarCreditos()
    {
        Console.WriteLine("  Creditos à Lucas Maciel de Campos, Criador da Libra.");
        Console.WriteLine("  Roberto Fernandes de Paiva: Inventor do nome Libra.");
        Console.WriteLine("  Contribuidores: Fábio de Souza Villaça Medeiros.");
    }

    private static void MostrarAjuda()
    {
        Console.WriteLine("Libra é uma linguagem de programação, você está no modo Interativo,");
        Console.WriteLine("ou seja, pode digitar uma instrução diretamente por aqui.");
        Console.WriteLine("Tente executar uma expressão. Exemplos: `1+1`, `2^10`, `raizq(64)`. Ou digite um comando.");
        Console.WriteLine();
        Console.WriteLine("Para interpretar um arquivo, use `nomeDoArquivo.libra`");
        Console.WriteLine("Comandos disponíveis: " + string.Join(", ", _comandos.Keys));
    }
}