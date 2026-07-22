using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Libra
{
    public interface ILogger
    {
        public void Msg(string mensagem, string final = "\n");
    }

    public class ConsoleLogger : ILogger
    {
        public void Msg(string mensagem, string final = "\n")
        {
            if (string.IsNullOrEmpty(mensagem))
                mensagem = "Nulo";

            if (mensagem.StartsWith("Aviso: "))
                Console.ForegroundColor = ConsoleColor.Yellow;

            // Escreve a mensagem + final de forma imediata
            Console.Write(mensagem + final);
            Console.ResetColor();

            // Força envio imediato para stdout
            Console.Out.Flush();
        }
    }

    /// <summary>
    /// Logger que descarta a saída. Usado por embedders (ex.: playground WASM)
    /// que capturam o texto diretamente de <c>Ambiente.TextoSaida</c> e não querem
    /// escrever no Console (evita chamadas de cor não suportadas no browser).
    /// </summary>
    public class SilentLogger : ILogger
    {
        public void Msg(string mensagem, string final = "\n") { }
    }

    /// <summary>
    /// Logger que acumula toda a saída em memória, sem tocar no Console. Como a mesma
    /// instância pode ser compartilhada entre o interpretador principal e os
    /// interpretadores de módulos importados, a saída de funções de biblioteca
    /// (ex.: <c>mostrarVetor</c>) é capturada na ordem correta. Usado pelo playground WASM.
    /// </summary>
    public class CapturingLogger : ILogger
    {
        private readonly System.Text.StringBuilder _sb = new();

        public void Msg(string mensagem, string final = "\n")
        {
            _sb.Append(mensagem ?? "");
            _sb.Append(final ?? "");
        }

        public string Texto => _sb.ToString();
    }
}
