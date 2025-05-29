using Libra;
using Libra.Runtime;

namespace Libra.Api;

public class MotorLibra
{
    private readonly OpcoesMotorLibra _opcoes;
    private Tokenizador _tokenizador = new Tokenizador();
    private Parser _parser = new Parser();
    private Interpretador? _interpretador;
    private Ambiente? _ambiente;

    /// <summary>
    /// Inicializa uma nova instância do MotorLibra com opções padrão.
    /// Útil para cenários onde não é necessário customizar o comportamento do motor.
    /// </summary>
    public MotorLibra()
    {
        _opcoes = new OpcoesMotorLibra
        {
            NivelDebug = 0,
            ModoExecucao = ModoExecucao.Interpretar
        };

        _ambiente = Ambiente.ConfigurarAmbiente(new ConsoleLogger(), true);
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
    /// Define uma variável global no ambiente do motor, tornando-a acessível em todos os scripts.
    /// </summary>
    /// <param name="identificador">Nome da variável global.</param>
    /// <param name="valor">Valor a ser atribuído à variável global.</param>
    public void DefinirGlobal(string identificador, object valor)
    {
        Ambiente.DefinirGlobal(identificador, valor);
    }

    /// <summary>
    /// Obtém o valor de uma variável global definida no ambiente do motor.
    /// </summary>
    /// <param name="identificador">Nome da variável global.</param>
    /// <returns>Valor da variável global, ou null se não existir.</returns>
    public object? ObterGlobal(string identificador)
    {
        return Ambiente.ObterGlobal(identificador);
    }

    /// <summary>
    /// Registra uma função nativa C# para ser chamada a partir dos scripts executados pelo motor.
    /// Permite estender as funcionalidades do ambiente de script com código C#.
    /// </summary>
    /// <param name="nomeNoScript">Nome pelo qual a função será chamada no script.</param>
    /// <param name="funcaoCSharp">Delegado da função C# a ser executada.</param>
    public void RegistrarFuncaoNativa(string nomeNoScript, Func<object?[], object?> funcaoCSharp)
    {
        if (_ambiente == null)
        {
            throw new InvalidOperationException("Ambiente não foi inicializado.");
        }
        Ambiente.RegistrarFuncaoNativa(nomeNoScript, funcaoCSharp!);
    }

    /// <summary>
    /// Executa um código em formato de string no ambiente do motor, utilizando o modo de execução configurado.
    /// Atualmente, apenas o modo de interpretação está implementado.
    /// </summary>
    /// <param name="codigo">Código a ser executado.</param>
    /// <returns>Resultado da execução, se houver; caso contrário, null.</returns>
    public object? Executar(string codigo)
    {
        if (_opcoes.ModoExecucao == ModoExecucao.Interpretar)
        {
            _interpretador = new Interpretador();
        }
        else
        {
            Console.WriteLine("Modo de execução não suportado. Apenas 'Interpretar' está implementado.");
            return null;
        }
        try
        {
            var tokens = _tokenizador.Tokenizar(codigo);
            var programa = _parser.Parse(tokens.ToArray());
            _interpretador.ExecutarPrograma(programa);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro: {ex.Message}");
        }

        return Interpretador.Saida.ObterValor();
    }
}
