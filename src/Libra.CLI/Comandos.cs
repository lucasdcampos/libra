using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using Libra;

public static class Comandos
{
    private static readonly Dictionary<string, Action> _comandos = new()
    {
        { "sair", () => Environment.Exit(0) },
        { "limpar", Console.Clear },
        { "licenca", MostrarLicenca },
        { "creditos", MostrarCreditos },
        { "autor", MostrarCreditos },
        { "ajuda", MostrarAjuda },
        { "versao", () => Console.WriteLine(LibraUtil.VersaoAtual()) },
        { "v", () => Console.WriteLine(LibraUtil.VersaoAtual()) },
        { "novo", IniciarProjeto},
        { "rodar", RodarProjeto}
    };

    internal static bool ExecutarComando(string comando)
    {
        if (_comandos.TryGetValue(comando, out var acao))
        {
            acao.Invoke();
            return true;
        }

        return false;
    }

    internal static bool EhComandoInterno(string comando)
    {
        return _comandos.ContainsKey(comando.ToLowerInvariant());
    }

    internal static void MostrarLicenca()
    {
        Console.WriteLine("MIT License - Copyright 2024 - 2025 Lucas M. Campos");
        Console.WriteLine("Acesse https://github.com/lucasdcampos/libra para mais detalhes");
    }

    internal static void IniciarProjeto()
    {
        string nomeArquivoConfig = "libra.json";

        if (File.Exists(nomeArquivoConfig))
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"Aviso: O arquivo '{nomeArquivoConfig}' já existe no diretório atual.");
            Console.WriteLine("Nenhum arquivo novo foi criado para não sobrescrever o existente.");
            Console.ResetColor();
            return;
        }

        string nomeProjeto = "Novo Projeto";
        string versao = "0.1.0";
        string descricao = "Descrição do Projeto";
        List<string> autores = new List<string>();
        string licenca = "SEM LICENÇA";
        string codigoPrincipal = "inicio.libra";

        try
        {
            var sb = new StringBuilder();
            sb.AppendLine("{");

            sb.AppendFormat("  \"NomeProjeto\": \"{0}\",\n", nomeProjeto);
            sb.AppendFormat("  \"Versao\": \"{0}\",\n", versao);
            sb.AppendFormat("  \"Descricao\": \"{0}\",\n", descricao);

            sb.Append("  \"Autores\": [],\n");
        
            sb.AppendFormat("  \"Licenca\": \"{0}\",\n", licenca);
            sb.AppendFormat("  \"CodigoPrincipal\": \"{0}\",\n", codigoPrincipal);

            sb.AppendLine("  \"OpcoesMotor\": {");
            sb.AppendLine("    \"ModoEstrito\": true");
            sb.AppendLine("  }");

            sb.AppendLine("}");

            string conteudoJson = sb.ToString();

            File.WriteAllText(nomeArquivoConfig, conteudoJson);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Arquivo '{nomeArquivoConfig}' criado com sucesso no diretório atual!");
            Console.ResetColor();
            Console.WriteLine("Você pode editá-lo para ajustar as configurações do seu projeto Libra.");
            Console.WriteLine($"Não se esqueça de criar o arquivo principal: '{codigoPrincipal}'");

        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Ocorreu um erro ao criar o arquivo '{nomeArquivoConfig}':");
            Console.WriteLine(ex.Message);
            Console.ResetColor();
        }
    }

    internal static void RodarProjeto()
    {
        Console.WriteLine("A ser implementado..");
    }

    internal static void MostrarCreditos()
    {
        Console.WriteLine("  Creditos à Lucas Maciel de Campos, Criador da Libra.");
        Console.WriteLine("  Roberto Fernandes de Paiva: Inventor do nome Libra.");
        Console.WriteLine("  Contribuidores: Fábio de Souza Villaça Medeiros.");
    }

    internal static void MostrarAjuda()
    {
        Console.WriteLine("Libra é uma linguagem de programação, você está no modo Interativo,");
        Console.WriteLine("ou seja, pode digitar uma instrução diretamente por aqui.");
        Console.WriteLine("Tente executar uma expressão. Exemplos: `1+1`, `2^10`, `raizq(64)`. Ou digite um comando.");
        Console.WriteLine();
        Console.WriteLine("Para interpretar um arquivo, use `nomeDoArquivo.libra`");
        Console.WriteLine("Comandos disponíveis: " + string.Join(", ", _comandos.Keys));
    }
}