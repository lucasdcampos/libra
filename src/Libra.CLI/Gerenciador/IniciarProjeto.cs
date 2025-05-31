using System.Text;

internal static class IniciarNovoProjeto
{
    static string _nomePadraoProjeto = "ProjetoLibra/";
    const string _nomeConfig = "libra.json";
    internal static void Executar(string nome)
    {
        _nomePadraoProjeto = nome;
        string codigoPath = Path.Combine(nome, "codigo");
        string arquivoInicialPath = Path.Combine(codigoPath, "inicio.libra");
        string conteudoInicial = "//Para mais informações, visite: https://docs.linguagem-libra.github.io\nexibir(\"Olá, Mundo!\")";

        // Cria o diretório principal e o subdiretório "codigo"
        Directory.CreateDirectory(codigoPath);

        // Cria o arquivo "inicio.libra" com o conteúdo inicial
        File.WriteAllText(arquivoInicialPath, conteudoInicial);

        CriarJson();
    }

    static void CriarJson()
    {
        string nomeArquivoConfig = Path.Combine(_nomePadraoProjeto, _nomeConfig);

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
             sb.AppendFormat("  \"Raiz\": \"{0}\",\n", "codigo/");
            sb.AppendFormat("  \"CodigoPrincipal\": \"{0}\",\n", codigoPrincipal);

            sb.AppendLine("  \"OpcoesMotor\": {");
            sb.AppendLine("    \"ModoEstrito\": true");
            sb.AppendLine("  }");

            sb.AppendLine("}");

            string conteudoJson = sb.ToString();

            File.WriteAllText(nomeArquivoConfig, conteudoJson);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Arquivo '{_nomePadraoProjeto}/{nomeArquivoConfig}' criado com sucesso!");
            Console.ResetColor();
            Console.WriteLine($"Use 'cd {_nomePadraoProjeto}' e depois 'libra rodar' para executar seu código!");

        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Ocorreu um erro ao criar o arquivo '{nomeArquivoConfig}':");
            Console.WriteLine(ex.Message);
            Console.ResetColor();
        }
    }
}