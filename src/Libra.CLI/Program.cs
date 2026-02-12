using Libra.Motor;

internal static class Program
{
    internal static void Main(string[] args)
    {
        var argumentosParaProcessar = new List<string>(args);

        var opcoesMotor = new OpcoesMotorLibra();

        if (ProcessarFlagsDeComandoGlobal(argumentosParaProcessar, ref opcoesMotor))
            return;

        if (argumentosParaProcessar.Count == 0)
            IniciarRepl(opcoesMotor);
        else
        {
            ProcessarArgumentos(argumentosParaProcessar, opcoesMotor);  
        }
    }

    private static void ProcessarArgumentos(List<string> argumentosParaProcessar, OpcoesMotorLibra opcoesMotor)
    {
        string acaoPrincipal = argumentosParaProcessar[0];
        argumentosParaProcessar.RemoveAt(0);

        switch (acaoPrincipal.ToLowerInvariant())
        {
            case "novo":
                Comandos.IniciarProjeto(argumentosParaProcessar);
                break;

            case "rodar":
                Comandos.RodarProjeto(argumentosParaProcessar);
                break;

            default:
                if (argumentosParaProcessar.Any())
                {
                    Console.WriteLine($"Erro: Argumentos inesperados '{string.Join(" ", argumentosParaProcessar)}' após o nome do arquivo '{acaoPrincipal}'.");
                    return;
                }
                var motor = new MotorLibra(opcoesMotor);
                string codigo = "";
                try
                {
                    codigo = File.ReadAllText(acaoPrincipal);
                }
                catch
                {
                    Console.WriteLine("Não foi possível carregar o arquivo " + acaoPrincipal);
                }

                string caminhoCompleto = Path.Combine(Directory.GetCurrentDirectory(), acaoPrincipal);
            
                motor.Executar(codigo, acaoPrincipal, Path.GetDirectoryName(caminhoCompleto) ?? "");
            break;
        }
    }
    
    private static bool ProcessarFlagsDeComandoGlobal(List<string> argumentos, ref OpcoesMotorLibra opcoesMotor)
    {
        // Iterar de trás para frente para poder remover itens da lista
        for (int i = argumentos.Count - 1; i >= 0; i--)
        {
            string arg = argumentos[i];
            if (arg.StartsWith("--"))
            {
                string flag = arg.TrimStart('-');
                bool flagProcessada = true;
                if (Comandos.EhComandoInterno(flag.ToLowerInvariant()))
                {
                    Comandos.ExecutarComando(flag.ToLowerInvariant());
                    return true;
                }

                switch (flag.ToLowerInvariant())
                    {
                        case "modoestrito":
                            opcoesMotor.ModoEstrito = true;
                            break;
                        case "ignoraravisos":
                            opcoesMotor.TratarAvisosComoErros = false;
                            break;
                        case "modoseguro":
                            opcoesMotor.ModoSeguro = true;
                            break;
                        default:
                            flagProcessada = false; // Não é uma flag global reconhecida
                            break;
                    }
                if (flagProcessada)
                {
                    argumentos.RemoveAt(i); // Remove a flag se foi processada
                }
            }
        }
        return false;
    }

    private static void IniciarRepl(OpcoesMotorLibra opcoesIniciais)
    {
        var repl = new Repl(opcoesIniciais);
        repl.ExecutarLoop();
    }
}