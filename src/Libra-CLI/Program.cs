// TODO: Necessita de refatoração
// Está uma bagunça porque esse arquivo é só uma
// ferramenta que serve de cli para rodar programas Libra
// não dei muita atenção ao código
using Libra;
using Libra.Arvore;

internal static class Program
{
    private static bool _carregarBibliotecas = true;
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
        { "versao", () => Console.WriteLine(LibraUtil.VersaoAtual()) },
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

        Console.WriteLine($"Bem-vindo à Libra {LibraUtil.VersaoAtual()}");
        Console.WriteLine("Digite \"ajuda\", \"licenca\" ou uma instrução.");
        
        List<Token> bibliotecaPreTokenizada = new();
        Instrucao[] astBiblioteca = new Instrucao[0];
        if(_carregarBibliotecas)
        {
            bibliotecaPreTokenizada = new Tokenizador().Tokenizar(bibliotecas);
            astBiblioteca = new Parser().ParseInstrucoes(bibliotecaPreTokenizada.ToArray());
        }
        
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
                    tokens = new Tokenizador().Tokenizar("exibir("+linha+")");
                    instrucoes = new Parser().ParseInstrucoes(tokens.ToArray()).ToList<Instrucao>();

                    // Haverá problemas se executar pelo botão do Visual Studio, pois ele não conseguirá
                    // carregar as bibliotecas. Isso se certifica de não carregar em DEBUG mode.
                    #if DEBUG
                    #else
                        if(_carregarBibliotecas && astBiblioteca != null && instrucoes != null)
                            instrucoes.InsertRange(0, astBiblioteca);
                    #endif

                    new Interpretador().ExecutarPrograma(new Programa(instrucoes.ToArray()), false, new ConsoleLogger(), true);
                }
                catch (Exception e)
                {
                    Erro.MensagemBug(e);
                }
            }
        }
    }

    private static bool ExecutarComando(string comando)
    {
        if(comando.EndsWith(".libra"))
        {
            try
            {
                Interpretar(comando);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Algo deu errado: {e.Message}");
            }
            
            return false;
        }

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
        Console.WriteLine("Libra é uma linguagem de programação, você está no modo Interativo,");
        Console.WriteLine("ou seja, pode digitar uma instrução diretamente por aqui.");
        Console.WriteLine("Tente executar uma expressão. Exemplos: `1+1`, `2^10`, `raizq(64)`. Ou digite um comando.");
        Console.WriteLine();
        Console.WriteLine("Para interpretar um arquivo, use `nomeDoArquivo.libra`");
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

        string codigoFonte = File.ReadAllText(arquivoInicial);

        new Interpretador().Interpretar(codigoFonte, false, new ConsoleLogger(), false, arquivoInicial);

        stopwatch.Stop();

        if (debug)
        {
            Console.WriteLine($"Tempo de execução: {stopwatch.ElapsedMilliseconds} ms");
        }
    }
}
