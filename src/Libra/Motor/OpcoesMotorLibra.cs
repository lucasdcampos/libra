using Libra;

namespace Libra.Motor
{
    /// <summary>
    /// Define o nível de detalhamento para logs de depuração.
    /// </summary>
    public enum NivelDebugDetalhe
    {
        /// <summary>
        /// Nenhum log de depuração.
        /// </summary>
        Nenhum,
        /// <summary>
        /// Logs básicos sobre o início e fim da execução.
        /// </summary>
        Basico,
        /// <summary>
        /// Logs mais detalhados sobre o fluxo de execução.
        /// </summary>
        Detalhado,
        /// <summary>
        /// Logs verbosos, incluindo estado interno (pode impactar performance).
        /// </summary>
        Verboso
    }

    public class OpcoesMotorLibra
    {
        /// <summary>
        /// Define o nível de detalhamento dos logs de depuração internos do motor.
        /// O padrão é Nenhum.
        /// </summary>
        public NivelDebugDetalhe NivelDebug { get; set; } = NivelDebugDetalhe.Nenhum;

        /// <summary>
        /// Ativa o "modo estrito" da linguagem Libra.
        /// Pode habilitar verificações de tipo mais rigorosas,
        /// proibir certas construções consideradas inseguras ou obsoletas, etc.
        /// O padrão é false.
        /// </summary>
        public bool ModoEstrito { get; set; } = true;

        /// <summary>
        /// Se true, todos os avisos gerados pelo motor ou compilador serão tratados como erros,
        /// interrompendo a execução.
        /// O padrão é false.
        /// </summary>
        public bool TratarAvisosComoErros { get; set; } = false;

        /// <summary>
        /// Uma lista de códigos ou identificadores de avisos que devem ser suprimidos (ignorados)
        /// pelo motor. Permite um controle granular sobre quais avisos são reportados.
        /// O padrão é uma lista vazia (nenhum aviso desativado).
        /// </summary>
        public List<string> ListaAvisosDesativados { get; set; } = new List<string>();

        /// <summary>
        /// Permite redirecionar a saída padrão do script (ex: o que seria impresso por uma função 'imprimir')
        /// para um TextWriter customizado.
        /// Se null, o motor pode usar Console.Out como padrão.
        /// </summary>
        public TextWriter? SaidaPadrao { get; set; } = null;

        /// <summary>
        /// Permite redirecionar a saída de erro padrão do script para um TextWriter customizado.
        /// Se null, o motor pode usar Console.Error como padrão.
        /// </summary>
        public TextWriter? SaidaErro { get; set; } = null;

        /// <summary>
        /// Define um diretório base para o motor.
        /// Pode ser usado para resolver caminhos relativos ao carregar outros scripts (ex: módulos, includes)
        /// ou para operações de entrada/saída de arquivos dentro dos scripts Libra.
        /// Se null, pode usar o diretório de trabalho atual da aplicação.
        /// </summary>
        public string? DiretorioBase { get; set; } = null;

        public bool ModoSeguro { get; set; } = false;
        public bool PermitirEntrada { get; set; } = true;

        /// <summary>
        /// Logger customizado para a saída do script. Se null, usa <see cref="ConsoleLogger"/>.
        /// Embedders (ex.: playground WASM) podem passar um <see cref="SilentLogger"/> e ler
        /// a saída de <see cref="LibraResultado.SaidaTerminal"/>.
        /// </summary>
        public ILogger? Logger { get; set; } = null;

        /// <summary>
        /// Provedor de entrada para a função `entrada()`. Se null, lê do Console.
        /// </summary>
        public Func<string?>? LerLinha { get; set; } = null;

        /// <summary>
        /// Se true (padrão, CLI/REPL), erros são impressos no Console de forma formatada.
        /// Embedders que capturam a saída devem definir como false — o texto do erro é
        /// devolvido em <see cref="LibraResultado.SaidaTerminal"/>.
        /// </summary>
        public bool ExibirErrosNoConsole { get; set; } = true;

        /// <summary>
        /// Lista de caminhos adicionais onde o interpretador deve procurar por bibliotecas importadas.
        /// </summary>
        public List<string> CaminhosBiblioteca { get; set; } = new List<string>();
    }
}