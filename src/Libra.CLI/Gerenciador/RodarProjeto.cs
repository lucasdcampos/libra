using System.IO;
using System.Text.Json;

internal static class RodarProjetoLibra
{
    internal static void Executar()
    {
        string jsonPath = Path.Combine(Directory.GetCurrentDirectory(), "projeto.libra.json");
        if (!File.Exists(jsonPath))
        {
            Console.WriteLine("Arquivo projeto.libra.json não encontrado na raiz do projeto.");
            return;
        }

        string jsonContent = File.ReadAllText(jsonPath);
        using var doc = JsonDocument.Parse(jsonContent);
        var root = doc.RootElement;

        string raiz = root.TryGetProperty("raiz", out var raizProp) ? raizProp.GetString() ?? "" : "";
        string codigoPrincipal = root.TryGetProperty("codigoPrincipal", out var codProp) ? codProp.GetString() ?? "" : "";

        if (string.IsNullOrWhiteSpace(raiz) || string.IsNullOrWhiteSpace(codigoPrincipal))
        {
            Console.WriteLine("Campos 'raiz' ou 'codigoPrincipal' não encontrados ou inválidos no projeto.libra.json.");
            return;
        }

        string caminhoCompleto = Path.GetFullPath(Path.Combine(raiz, codigoPrincipal));

        if (!File.Exists(caminhoCompleto))
        {
            Console.WriteLine($"Arquivo principal '{caminhoCompleto}' não encontrado.");
            return;
        }

        string codigo = File.ReadAllText(caminhoCompleto);

        var motor = new Libra.Motor.MotorLibra();
        motor.Executar(codigo, codigoPrincipal, Path.GetDirectoryName(caminhoCompleto) ?? "");
    }
}
