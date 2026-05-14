using System.Diagnostics;
using System.IO.Compression;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

internal static class GerenciadorPacotesLibra
{
    const string PastaPacotes = "pacotes";
    const string ArquivoConfig = "projeto.libra.json";
    const string ArquivoRequirements = "pacotes.txt";

    private static readonly HashSet<string> _pacotesProcessados = new();
    private static readonly HttpClient _httpClient = new();

    internal static void Instalar(List<string> args)
    {
        _pacotesProcessados.Clear();

        if (args.Count == 0)
        {
            InstalarDependencias(Directory.GetCurrentDirectory());
            return;
        }

        if (args[0] == "-r" && args.Count > 1)
        {
            string caminhoArquivo = args[1];
            InstalarDeArquivo(caminhoArquivo);
            return;
        }

        string url = NormalizarUrl(args[0]);
        InstalarPacote(url, eInstalacaoDireta: true);
    }

    private static string NormalizarUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url)) return url;

        if (url.StartsWith("http://") || url.StartsWith("https://") || url.StartsWith("git@"))
        {
            return url;
        }

        var match = Regex.Match(url, @"^([^/]+)/([^/]+)$");
        if (match.Success)
        {
            return $"https://github.com/{url}.git";
        }

        return url;
    }

    private static void InstalarDeArquivo(string caminhoArquivo)
    {
        if (!File.Exists(caminhoArquivo))
        {
            Console.WriteLine($"Erro: Arquivo '{caminhoArquivo}' não encontrado.");
            return;
        }

        string[] linhas = File.ReadAllLines(caminhoArquivo);
        foreach (string linha in linhas)
        {
            if (string.IsNullOrWhiteSpace(linha) || linha.StartsWith("#")) continue;
            string url = NormalizarUrl(linha.Trim());
            Console.WriteLine($"Instalando pacote do arquivo: {url}...");
            InstalarPacote(url, eInstalacaoDireta: true);
        }
    }

    private static void InstalarDependencias(string diretorioBase)
    {
        string caminhoConfig = Path.Combine(diretorioBase, ArquivoConfig);
        
        if (File.Exists(caminhoConfig))
        {
            try
            {
                string json = File.ReadAllText(caminhoConfig);
                using JsonDocument doc = JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("dependencias", out JsonElement deps))
                {
                    foreach (var dep in deps.EnumerateObject())
                    {
                        string urlDep = NormalizarUrl(dep.Value.GetString() ?? "");
                        if (string.IsNullOrWhiteSpace(urlDep)) continue;

                        if (_pacotesProcessados.Contains(urlDep)) continue;

                        Console.WriteLine($"Instalando dependência {dep.Name} ({urlDep})...");
                        InstalarPacote(urlDep, eInstalacaoDireta: false);
                    }
                    return;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao ler dependências de {caminhoConfig}: {ex.Message}");
            }
        }

        string caminhoRequirements = Path.Combine(diretorioBase, ArquivoRequirements);
        if (diretorioBase == Directory.GetCurrentDirectory() && File.Exists(caminhoRequirements))
        {
            string[] linhas = File.ReadAllLines(caminhoRequirements);
            foreach (string linha in linhas)
            {
                if (string.IsNullOrWhiteSpace(linha) || linha.StartsWith("#")) continue;
                string urlDep = NormalizarUrl(linha.Trim());
                if (_pacotesProcessados.Contains(urlDep)) continue;

                Console.WriteLine($"Instalando {urlDep}...");
                InstalarPacote(urlDep, eInstalacaoDireta: false);
            }
            return;
        }
    }

    private static void InstalarPacote(string url, bool eInstalacaoDireta)
    {
        if (string.IsNullOrWhiteSpace(url)) return;
        if (_pacotesProcessados.Contains(url)) return;
        _pacotesProcessados.Add(url);

        if (!Directory.Exists(PastaPacotes))
        {
            Directory.CreateDirectory(PastaPacotes);
        }

        string nomePacote = ExtrairNomePacote(url);
        string caminhoDestino = Path.Combine(PastaPacotes, nomePacote);

        bool jaExistia = Directory.Exists(caminhoDestino);
        bool sucessoInstalacao = false;

        if (jaExistia)
        {
            Console.WriteLine($"O pacote '{nomePacote}' já está instalado. Atualizando...");
            sucessoInstalacao = ExecutarComandoGit("pull", caminhoDestino);
            
            if (!sucessoInstalacao)
            {
                Console.WriteLine("Git falhou na atualização. Tentando baixar via ZIP para substituir...");
                sucessoInstalacao = BaixarEExtrairZip(url, caminhoDestino).GetAwaiter().GetResult();
            }
        }
        else
        {
            Console.WriteLine($"Tentando clonar '{url}' em '{caminhoDestino}'...");
            sucessoInstalacao = ExecutarComandoGit($"clone {url} {nomePacote}", PastaPacotes);

            if (!sucessoInstalacao)
            {
                Console.WriteLine("Git indisponível ou erro ao clonar. Tentando baixar via ZIP...");
                sucessoInstalacao = BaixarEExtrairZip(url, caminhoDestino).GetAwaiter().GetResult();
            }
        }

        if (!sucessoInstalacao)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Erro: Não foi possível instalar o pacote '{nomePacote}' via Git ou ZIP.");
            Console.ResetColor();
            return;
        }

        if (!ValidarPacote(caminhoDestino, nomePacote, out string? erro))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Erro: {erro}");
            Console.ResetColor();
            
            if (!jaExistia && Directory.Exists(caminhoDestino))
            {
                ForcarRemocaoDiretorio(caminhoDestino);
            }
            return;
        }

        if (eInstalacaoDireta)
        {
             AtualizarConfiguracoes(nomePacote, url);
        }

        if (Directory.Exists(caminhoDestino))
        {
            InstalarDependencias(caminhoDestino);
        }
    }

    private static async Task<bool> BaixarEExtrairZip(string url, string caminhoDestino)
    {
        try
        {
            string zipUrl = GerarUrlZip(url);
            if (string.IsNullOrEmpty(zipUrl)) return false;

            byte[] zipData = await _httpClient.GetByteArrayAsync(zipUrl);
            string tempFile = Path.GetTempFileName();
            await File.WriteAllBytesAsync(tempFile, zipData);

            string tempExtractPath = Path.Combine(Path.GetTempPath(), "libra_extract_" + Guid.NewGuid());
            ZipFile.ExtractToDirectory(tempFile, tempExtractPath);

            // GitHub coloca o conteúdo dentro de uma subpasta <repo>-<branch>
            string[] subpastas = Directory.GetDirectories(tempExtractPath);
            if (subpastas.Length == 1)
            {
                if (Directory.Exists(caminhoDestino)) ForcarRemocaoDiretorio(caminhoDestino);
                Directory.Move(subpastas[0], caminhoDestino);
            }
            else
            {
                if (Directory.Exists(caminhoDestino)) ForcarRemocaoDiretorio(caminhoDestino);
                Directory.Move(tempExtractPath, caminhoDestino);
            }

            File.Delete(tempFile);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao baixar/extrair ZIP: {ex.Message}");
            return false;
        }
    }

    private static string GerarUrlZip(string url)
    {
        // Se for GitHub, converte para a URL do zip da main
        if (url.Contains("github.com"))
        {
            string baseUrl = url.Replace(".git", "").TrimEnd('/');
            return $"{baseUrl}/archive/refs/heads/main.zip";
        }
        return "";
    }

    private static void ForcarRemocaoDiretorio(string caminho)
    {
        try
        {
            if (!Directory.Exists(caminho)) return;

            var directory = new DirectoryInfo(caminho) { Attributes = FileAttributes.Normal };

            foreach (var info in directory.GetFileSystemInfos("*", SearchOption.AllDirectories))
            {
                info.Attributes = FileAttributes.Normal;
            }

            directory.Delete(true);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Aviso: Não foi possível limpar a pasta: {ex.Message}");
        }
    }

    private static bool ValidarPacote(string caminho, string nome, out string? erro)
    {
        erro = null;
        if (!Directory.Exists(caminho))
        {
            erro = $"Pasta do pacote '{nome}' não encontrada após instalação.";
            return false;
        }

        string configPath = Path.Combine(caminho, ArquivoConfig);
        if (!File.Exists(configPath))
        {
            erro = $"O repositório '{nome}' não é um pacote Libra válido (falta '{ArquivoConfig}').";
            return false;
        }

        try
        {
            string json = File.ReadAllText(configPath);
            using JsonDocument doc = JsonDocument.Parse(json);
        }
        catch (Exception ex)
        {
            erro = $"Arquivo '{ArquivoConfig}' do pacote '{nome}' é inválido: {ex.Message}";
            return false;
        }

        return true;
    }

    private static string ExtrairNomePacote(string url)
    {
        var match = Regex.Match(url, @"/([^/]+?)(?:\.git)?$");
        if (match.Success)
        {
            return match.Groups[1].Value;
        }
        return "pacote_desconhecido";
    }

    private static bool ExecutarComandoGit(string argumentos, string diretorioTrabalho)
    {
        try
        {
            ProcessStartInfo psi = new ProcessStartInfo("git", argumentos)
            {
                WorkingDirectory = diretorioTrabalho,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using Process? processo = Process.Start(psi);
            if (processo == null) return false;
            
            processo.WaitForExit();
            return processo.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }

    private static void AtualizarConfiguracoes(string nome, string url)
    {
        if (!File.Exists(ArquivoConfig)) return;

        try
        {
            string jsonContent = File.ReadAllText(ArquivoConfig);
            var node = JsonNode.Parse(jsonContent);
            if (node is not JsonObject obj) return;

            if (!obj.ContainsKey("dependencias"))
            {
                obj["dependencias"] = new JsonObject();
            }

            var deps = obj["dependencias"]?.AsObject();
            if (deps != null)
            {
                deps[nome] = url;
            }

            var options = new JsonSerializerOptions { WriteIndented = true };
            File.WriteAllText(ArquivoConfig, obj.ToJsonString(options));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao atualizar {ArquivoConfig}: {ex.Message}");
        }
    }
}
