using Libra;
using Libra.Api;

public class Repl
{
    private readonly OpcoesMotorLibra _opcoesMotorBase;
    public Repl(OpcoesMotorLibra opcoesMotor)
    {
        _opcoesMotorBase = opcoesMotor;
    }

    public void ExecutarLoop()
    {
        Console.WriteLine($"Bem-vindo à Libra {LibraUtil.VersaoAtual()}");
        Console.WriteLine("Digite \"ajuda\", \"licenca\", \"sair\" ou uma instrução.");

        while (true)
        {
            Console.Write("> ");
            string? linha = Console.ReadLine();

            if (linha == null)
            {
                Console.WriteLine();
                break; 
            }

            string linhaProcessada = linha.Trim();

            if (linhaProcessada.Equals("sair", StringComparison.OrdinalIgnoreCase))
            {
                break;
            }

            if (string.IsNullOrWhiteSpace(linhaProcessada))
            {
                continue;
            }

            if (Comandos.ExecutarComando(linhaProcessada))
            {
                continue;
            }

            // Se não for um comando interno, tenta executar como código Libra
            try
            {
                var motor = new MotorLibra(_opcoesMotorBase);
                var saida = motor.Executar(linha); 
                
                if (saida != null)
                {
                    Console.WriteLine(saida.ToString());
                }
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.Error.WriteLine("Ocorreu um erro inesperado:");
                Console.Error.WriteLine(e.ToString());
                Console.ResetColor();
            }
        }
    }
}