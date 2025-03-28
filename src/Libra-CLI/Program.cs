using Libra;
using Libra.Arvore;

internal static class Program
{
    private static bool _carregarBibliotecas = true;
    private static readonly string _bibliotecas = 
@"importar matematica
importar vetores
";

    private static bool _avisos = false;
    private static bool _modoRigido = false;
    private static bool _modoSeguro = false;

    internal static void Main(string[] args)
    {
        // Processar argumentos
        foreach (var arg in args)
        {
            if (arg.StartsWith("--"))
            {
                switch (arg.TrimStart('-'))
                {
                    case "avisos":
                        _avisos = true;
                        break;

                    case "rigido":
                        _modoRigido = true;
                        break;
                    
                    case "seguro":
                        _modoSeguro = true;
                        break;

                    case "vanilla":
                        _carregarBibliotecas = false;
                        break;

                    default:
                        Console.WriteLine($"Flag desconhecida: {arg}");
                        break;
                }
            }
            else
            {
                Interpretar(arg, new InterpretadorFlags(_modoSeguro, _modoRigido, _avisos));
                return;
            }
        }

        Console.WriteLine($"Bem-vindo à Libra {LibraUtil.VersaoAtual()}");
        Console.WriteLine("Digite \"ajuda\", \"licenca\" ou uma instrução.");

        var ASTBiblioteca = new Instrucao[0];
        #if DEBUG
        #else
            if(_carregarBibliotecas)
                ASTBiblioteca = GerarAst(_bibliotecas);
        #endif

        while (true)
        {
            Console.Write(">>> ");
            string? linha = Console.ReadLine();

            if(linha == null)
            {
                Console.WriteLine("Ocorreu um erro, não foi possível ler do terminal.");
                Console.WriteLine("Dica: Se você estiver rodando via Docker, use a flag -it.");
            }
                

            if(linha.EndsWith(".libra"))
            {
                Interpretar(linha);
                continue;
            }

            if (string.IsNullOrWhiteSpace(linha) || Comandos.ExecutarComando(linha))
                continue;

            try
            {
                var instrucoes = GerarAst($"exibir({linha})").ToList<Instrucao>();
                if(ASTBiblioteca != null && ASTBiblioteca.Length != 0)
                    instrucoes.InsertRange(0, ASTBiblioteca);
                
                new Interpretador().ExecutarPrograma(
                    new Programa(instrucoes.ToArray()),
                    false,
                    new ConsoleLogger(),
                    true
                );
            }
            catch (Exception e)
            {
                Erro.MensagemBug(e);
            }
        }
    }

    private static Instrucao[] CarregarBibliotecas()
    {
        if (!_carregarBibliotecas)
            return Array.Empty<Instrucao>();

        return GerarAst(_bibliotecas);
    }

    private static Instrucao[] GerarAst(string codigo)
    {
        try
        {
            var tokenizador = new Tokenizador();
            var tokens = tokenizador.Tokenizar(codigo) ?? throw new Exception("Erro ao tokenizar código.");

            var parser = new Parser();
            var resultadoParse = parser.Parse(tokens.ToArray()) ?? throw new Exception("Erro ao criar AST.");
            return resultadoParse.Instrucoes;
        }
        catch
        {
            return new Instrucao[0];
        }
    }

    private static void Interpretar(string arquivoInicial, InterpretadorFlags flags = null)
    {
        if (!File.Exists(arquivoInicial))
        {
            Console.WriteLine($"Não foi possível localizar `{arquivoInicial}`");
            return;
        }

        var codigoFonte = File.ReadAllText(arquivoInicial);
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        new Interpretador(flags).Interpretar(codigoFonte, false, new ConsoleLogger(), false, arquivoInicial);

        stopwatch.Stop();
        Console.WriteLine($"Tempo de execução: {stopwatch.ElapsedMilliseconds} ms");
    }
}