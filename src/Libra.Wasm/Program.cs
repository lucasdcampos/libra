using System.Runtime.InteropServices.JavaScript;
using Libra;
using Libra.Motor;

// Ponto de entrada exigido para um app WebAssembly (OutputType=Exe).
// A aplicação React carrega o runtime e chama diretamente os métodos [JSExport] abaixo,
// portanto o Main apenas inicializa e não faz nada.
return;

/// <summary>
/// Superfície de interop exposta ao JavaScript do playground.
/// </summary>
public partial class LibraInterop
{
    /// <summary>
    /// Executa um trecho de código Libra e devolve o texto do terminal
    /// (saída do script + eventuais mensagens de erro já formatadas).
    /// </summary>
    /// <param name="codigo">Código-fonte Libra.</param>
    /// <param name="stdin">Conteúdo da "entrada padrão", consumido linha a linha por `entrada()`.</param>
    /// <param name="modoEstrito">
    /// Ativa o modo estrito (tipagem estática mais rígida no parser/interpretador).
    /// Controlado pelo toggle de opções do motor no playground.
    /// </param>
    [JSExport]
    internal static string Executar(string codigo, string stdin, bool modoEstrito)
    {
        var linhas = new Queue<string>(
            (stdin ?? "").Replace("\r\n", "\n").Replace("\r", "\n").Split('\n'));

        // Logger que acumula a saída (inclusive de módulos importados, que rodam em
        // interpretadores próprios). Escrever no Console quebra no browser WASM.
        var logger = new CapturingLogger();

        var opcoes = new OpcoesMotorLibra
        {
            Logger = logger,
            ExibirErrosNoConsole = false,
            // Bloqueia registrarCSharp/registrardll e afins — sandbox do playground.
            ModoSeguro = true,
            // O padrão do motor é estrito; no playground quem manda é o toggle da UI.
            ModoEstrito = modoEstrito,
            LerLinha = () => linhas.Count > 0 ? linhas.Dequeue() : "",
        };

        var motor = new MotorLibra(opcoes);
        motor.Executar(codigo, "playground.libra");
        return logger.Texto;
    }

    /// <summary>Versão do motor da Libra, para exibição no playground.</summary>
    [JSExport]
    internal static string Versao() => LibraUtil.VersaoAtual();
}
