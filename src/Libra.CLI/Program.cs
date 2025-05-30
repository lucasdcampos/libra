using System.Text.Json;
using Libra;
using Libra.Api;

internal static class Program
{
    internal static void Main(string[] args)
    {
        var argumentosParaProcessar = new List<string>(args); // Cópia para manipulação

        OpcoesMotorLibra opcoesMotor = new OpcoesMotorLibra();

        if (ProcessarFlagsDeComandoGlobal(argumentosParaProcessar, ref opcoesMotor))
        {
            return;
        }

        if (argumentosParaProcessar.Count == 0)
        {
            IniciarRepl(opcoesMotor);
        }
        else
        {
            string acaoPrincipal = argumentosParaProcessar[0];
            argumentosParaProcessar.RemoveAt(0); // Consome o argumento da ação principal

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
                            Console.WriteLine("[INFO] Flag --ignoraravisos aplicada (exemplo de lógica).");
                            break;
                        case "modoseguro":
                            Console.WriteLine("[INFO] Flag --modoseguro aplicada (exemplo de lógica).");
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

    private static void ExecutarArquivoScript(string caminhoArquivo, OpcoesMotorLibra opcoesBaseLinhaComando, List<string> argsScript)
    {
        OpcoesMotorLibra opcoesFinaisMotor = CopiarOpcoes(opcoesBaseLinhaComando);
        // Aplicar flags específicas passadas junto com o nome do arquivo
        opcoesFinaisMotor = CriarOpcoesMotorComFlags(argsScript, opcoesFinaisMotor);

        if (!File.Exists(caminhoArquivo))
        {
            Console.WriteLine($"Erro: Arquivo de codigo '{caminhoArquivo}' não encontrado.");
            return;
        }
        try
        {
            string conteudoScript = File.ReadAllText(caminhoArquivo);
            var motor = new MotorLibra(opcoesFinaisMotor); // Passa as opções configuradas
            var saida = motor.Executar(conteudoScript);
            if (saida != null)
            {
                Console.WriteLine(saida);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
    }

    private static void ExecutarComandoRodar(OpcoesMotorLibra opcoesBaseLinhaComando, List<string> argsComandoRodar)
    {
        OpcoesMotorLibra opcoesFinaisMotor = CopiarOpcoes(opcoesBaseLinhaComando);

        // Aplicar flags específicas do comando 'rodar' (ex: libra rodar --alguma-flag)
        opcoesFinaisMotor = CriarOpcoesMotorComFlags(argsComandoRodar, opcoesFinaisMotor);

        string arquivoConfig = "libra.json";

        if (File.Exists(arquivoConfig))
        {
            try
            {
                string conteudoJson = File.ReadAllText(arquivoConfig);
                var jsonOptions = new JsonSerializerOptions
                {
                    ReadCommentHandling = JsonCommentHandling.Skip,
                    PropertyNameCaseInsensitive = true
                };
                ConfiguracaoProjetoLibra? configProjeto = JsonSerializer.Deserialize<ConfiguracaoProjetoLibra>(conteudoJson, jsonOptions);

                if (configProjeto != null)
                {
                    Console.WriteLine($"Projeto: {configProjeto.NomeProjeto ?? "Sem nome"}, Versão: {configProjeto.Versao ?? "N/A"}");

                    // Sobrepõe as opções do motor com as do libra.json,
                    // mas as flags da linha de comando têm prioridade maior (já aplicadas em opcoesFinaisMotor)
                    if (configProjeto.OpcoesPadraoMotor != null)
                    {
                        opcoesFinaisMotor = MesclarOpcoes(configProjeto.OpcoesPadraoMotor, opcoesFinaisMotor);
                    }

                    if (!string.IsNullOrWhiteSpace(configProjeto.CodigoPrincipal))
                    {
                        if (File.Exists(configProjeto.CodigoPrincipal))
                        {
                            string conteudoScript = File.ReadAllText(configProjeto.CodigoPrincipal);
                            var motor = new MotorLibra(opcoesFinaisMotor);
                            var saida = motor.Executar(conteudoScript);
                            if (saida != null)
                            {
                                Console.WriteLine(saida);
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Erro: Script principal '{configProjeto.CodigoPrincipal}' definido em '{arquivoConfig}' não encontrado.");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Aviso: Nenhum 'ScriptPrincipal' definido em '{arquivoConfig}'. O comando 'rodar' não executou nada.");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao carregar ou processar '{arquivoConfig}': {ex.Message}");
                return;
            }
        }
        else
        {
            Console.WriteLine($"Erro: Arquivo '{arquivoConfig}' não encontrado no diretório atual para o comando 'rodar'.");
        }
    }

    private static bool ExtrairFlagGlobal(List<string> argumentos, string nomeFlag)
    {
        string flagCompleta = $"--{nomeFlag}";
        if (argumentos.Contains(flagCompleta))
        {
            argumentos.Remove(flagCompleta);
            return true;
        }
        return false;
    }

    private static OpcoesMotorLibra CriarOpcoesMotorComFlags(List<string> argumentos, OpcoesMotorLibra opcoesExistentes)
    {
        var novasOpcoes = CopiarOpcoes(opcoesExistentes); // Para não modificar a original diretamente

        // Iterar de trás para frente para poder remover itens da lista
        for (int i = argumentos.Count - 1; i >= 0; i--)
        {
            string arg = argumentos[i];
            if (arg.StartsWith("--"))
            {
                string flag = arg.TrimStart('-');
                bool flagProcessada = true;
                switch (flag)
                {
                    case "ignorarAvisos":
                        break;
                    case "modoEstrito":
                        novasOpcoes.ModoEstrito = true;
                        break;
                    case "modoSeguro":
                        break;
                    default:
                        flagProcessada = false;
                        break;
                }
                if (flagProcessada)
                {
                    argumentos.RemoveAt(i);
                }
            }
        }
        return novasOpcoes;
    }

    private static OpcoesMotorLibra CopiarOpcoes(OpcoesMotorLibra original)
    {
        return new OpcoesMotorLibra
        {
            NivelDebug = original.NivelDebug,
            ModoEstrito = original.ModoEstrito,
            TratarAvisosComoErros = original.TratarAvisosComoErros,
            ListaAvisosDesativados = new List<string>(original.ListaAvisosDesativados ?? new List<string>()),
            SaidaPadrao = original.SaidaPadrao,
            SaidaErro = original.SaidaErro,
            DiretorioBase = original.DiretorioBase
            // ... copiar o resto depois
        };
    }

    private static OpcoesMotorLibra MesclarOpcoes(OpcoesMotorLibra opcoesDoArquivo, OpcoesMotorLibra opcoesDaLinhaDeComando)
    {
        // Começa com as opções do arquivo como base
        var mesclado = CopiarOpcoes(opcoesDoArquivo);

        var resultado = new OpcoesMotorLibra(); // Começa com defaults do Motor

        // Aplica do arquivo de configuração
        if (opcoesDoArquivo != null)
        {
            resultado = CopiarOpcoes(opcoesDoArquivo);
        }

        var mescladoFinal = CopiarOpcoes(opcoesDoArquivo);

        var defaultsOpcoesMotor = new OpcoesMotorLibra();
        if (opcoesDaLinhaDeComando.ModoEstrito != defaultsOpcoesMotor.ModoEstrito) mescladoFinal.ModoEstrito = opcoesDaLinhaDeComando.ModoEstrito;
        if (opcoesDaLinhaDeComando.NivelDebug != defaultsOpcoesMotor.NivelDebug) mescladoFinal.NivelDebug = opcoesDaLinhaDeComando.NivelDebug;
       
        return mescladoFinal;
    }

    private static void IniciarRepl(OpcoesMotorLibra opcoesIniciais)
    {
        var repl = new Repl(opcoesIniciais);
        repl.ExecutarLoop();
    }
}