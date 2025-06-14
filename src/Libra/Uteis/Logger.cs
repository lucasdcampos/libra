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
            if(string.IsNullOrEmpty(mensagem))
                mensagem = "Nulo";
            
            if(mensagem.StartsWith("Aviso: "))
                Console.ForegroundColor = ConsoleColor.Yellow;
                
            Console.Write(mensagem); // TODO: Arrumar
            Console.ResetColor();
        }
    }

}
