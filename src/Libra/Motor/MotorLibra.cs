using Libra;
using Libra.Runtime;

namespace Libra.Motor;

public class MotorLibra
{
    private readonly OpcoesMotorLibra _opcoes;
    private Tokenizador _tokenizador;
    private Parser _parser;
    private Interpretador? _interpretador;

    /// <summary>
    /// Inicializa uma nova instância do MotorLibra com opções padrão.
    /// Útil para cenários onde não é necessário customizar o comportamento do motor.
    /// </summary>
    public MotorLibra()
    {
        _opcoes = new OpcoesMotorLibra();
    }

    /// <summary>
    /// Inicializa uma nova instância do MotorLibra com as opções fornecidas.
    /// Permite customizar o nível de debug e o modo de execução do motor.
    /// </summary>
    /// <param name="opcoes">Opções de configuração do motor.</param>
    public MotorLibra(OpcoesMotorLibra opcoes)
    {
        _opcoes = opcoes;
    }

    /// <summary>
    /// Executa um código em formato de string no ambiente do motor, utilizando o modo de execução configurado.
    /// Atualmente, apenas o modo de interpretação está implementado.
    /// </summary>
    /// <param name="codigo">Código a ser executado.</param>
    /// <returns>Resultado da execução, se houver; caso contrário, null.</returns>
    public LibraResultado Executar(string codigo, string arquivo="", string caminho="")
    {
        try
        {
            _tokenizador = new Tokenizador(codigo, arquivo, caminho);
            var tokens = _tokenizador.Tokenizar();
            _parser = new Parser(tokens.ToArray());
            var programa = _parser.Parse();
            
            var flags = new InterpretadorFlags(_opcoes.ModoSeguro, _opcoes.ModoEstrito, true);
            _interpretador = new Interpretador(flags);
            _interpretador.VisitarPrograma(programa);
            
        }
        catch (Erro e)
        {
            e.ExibirFormatado();

            if (_opcoes.NivelDebug > 0)
            {
                Console.WriteLine($"Pilha de Chamadas: {e.StackTrace}");
            }
        }
        catch (ExcecaoSaida)
        {
            // Não faz nada, programa foi encerrado com sucesso
        }
        catch (Exception ex)
        {
            string logsDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
            string logFile = Path.Combine(logsDir, $"erro-{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.txt");

            if (!Directory.Exists(logsDir))
            {
                Directory.CreateDirectory(logsDir);
            }

            string mensagemLog = "Ocorreu um erro interno na Libra, veja a descrição para mais detalhes:\n";
            mensagemLog += "Versão: Libra 1.0.0-Beta\n";
            mensagemLog += $"Ultima local do Script Libra executada: {_interpretador.LocalAtual}\n";
            mensagemLog += $"Problema:\n{ex.ToString()}\n";
            mensagemLog += "Por favor reportar em https://github.com/lucasdcampos/libra/issues/ (se possível incluir script que causou o problema)\n";

            File.WriteAllText(logFile, mensagemLog);

            Console.WriteLine("\nHouve um problema, mas não foi culpa sua :(");
            Console.WriteLine($"Uma descrição do erro foi salva em: {logFile}");
            Console.WriteLine("Por favor reportar em https://github.com/lucasdcampos/libra/issues/");
            Console.WriteLine($"Versão: Libra {LibraUtil.VersaoAtual()}"); // TODO: Não deixar a versão hardcoded dessa forma
            Console.WriteLine("\nImpossível continuar, encerrando a execução do programa.\n");
        }

        // TODO: ?
        return new LibraResultado(_interpretador.Saida.ObterValor(), "");
    }
}
