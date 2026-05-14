using Libra;
using Libra.Motor;

public class Repl
{
    private readonly OpcoesMotorLibra _opcoesMotorBase;
    private readonly MotorLibra _motor;

    public Repl(OpcoesMotorLibra opcoesMotor)
    {
        _opcoesMotorBase = opcoesMotor;
        _motor = new MotorLibra(_opcoesMotorBase);
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
                // Tenta executar a linha e captura o resultado
                var resultado = _motor.Executar(linhaProcessada);
                
                // Se o resultado tiver um valor (não for uma instrução pura como 'var')
                // e não for Nulo, exibe para o usuário
                if (resultado != null && resultado.Valor != null)
                {
                    Console.WriteLine(resultado.Valor);
                }
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.Error.WriteLine("Ocorreu um erro ao processar a instrução:");
                Console.Error.WriteLine(e.Message);
                Console.ResetColor();
            }
        }
    }
}