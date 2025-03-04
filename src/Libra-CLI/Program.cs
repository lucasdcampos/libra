﻿using Libra;
using Libra.Arvore;

internal static class Program
{
    private const string _ver = "1.0.0-Beta";

    private static Tokenizador _tokenizador = new Tokenizador();
    private static Parser _parser = new Parser();
    private static Interpretador _interpretador = new Interpretador();

    // Incluindo as bibliotecas por padrão na Shell da Libra
    private static string bibliotecas = 
@"importar ""matematica.libra""
importar ""so.libra""
importar ""vetores.libra""
importar ""utilidades.libra""

";

    private static readonly Dictionary<string, Action> _comandos = new()
    {
        { "sair", () => Environment.Exit(0) },
        { "limpar", Console.Clear },
        { "licenca", MostrarLicenca },
        { "creditos", MostrarCreditos },
        { "autor", MostrarCreditos },
        { "ajuda", MostrarAjuda },
        { "versao", () => Console.WriteLine(_ver) },
    };

    internal static void Main(string[] args)
    {
        if (args.Length == 1)
        {
            string arg = args[0];
            if(arg.StartsWith("--"))
                arg = arg.Replace("--", "");

            if (ExecutarComando(arg))
            {
                Interpretar(args[0]);
            }
            
            return;
        }

        Console.WriteLine($"Bem-vindo à Libra {_ver}");
        Console.WriteLine("Digite \"ajuda\", \"licenca\" ou uma instrução.");
        
        var bibliotecaPreTokenizada = _tokenizador.Tokenizar(bibliotecas);
        var astBiblioteca = _parser.ParseInstrucoes(bibliotecaPreTokenizada);

        while (true)
        {
            Console.Write(">>> ");
            var linha = Console.ReadLine();

            if (!string.IsNullOrWhiteSpace(linha) && ExecutarComando(linha))
            {
                List<Token> tokens;
                List<Instrucao> instrucoes = null;

                try
                {
                    tokens = _tokenizador.Tokenizar(linha);
                    instrucoes = _parser.ParseInstrucoes(tokens).ToList<Instrucao>();
                }
                catch (Exception e)
                {
                    Erro.MensagemBug(e);
                }

                instrucoes.InsertRange(0, astBiblioteca);

                _interpretador.Interpretar(new Programa(instrucoes.ToArray()), false, new ConsoleLogger(), true);
            }
        }
    }

    private static bool ExecutarComando(string comando)
    {
        if (_comandos.TryGetValue(comando, out var acao))
        {
            acao.Invoke();
            return false;
        }
        return true; // Comando não encontrado
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
        Console.WriteLine("Comandos disponíveis: " + string.Join(", ", _comandos.Keys));
    }

    private static void Interpretar(string arquivoInicial)
    {
        bool debug = true;
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        if (!File.Exists(arquivoInicial))
        {
            Console.WriteLine($"Não foi possível localizar `{arquivoInicial}`");
            return;
        }

        string codigoFonte = File.ReadAllText(arquivoInicial).ReplaceLineEndings(Environment.NewLine); // Sem isso, o Tokenizador buga

        new Interpretador().Interpretar(codigoFonte, false, new ConsoleLogger());

        stopwatch.Stop();

        if (debug)
        {
            Console.WriteLine($"Tempo de execução: {stopwatch.ElapsedMilliseconds} ms");
        }
    }
}
