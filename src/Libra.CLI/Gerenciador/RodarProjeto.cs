using System.IO;
using System.Text.Json;

internal static class RodarProjetoLibra
{
    internal static void Executar()
    {
        string jsonPath = Path.Combine(Directory.GetCurrentDirectory(), "libra.json");
        if (!File.Exists(jsonPath))
        {
            Console.WriteLine("Arquivo libra.json não encontrado na raiz do projeto.");
            return;
        }

        string jsonContent = File.ReadAllText(jsonPath);
        using var doc = JsonDocument.Parse(jsonContent);
        var root = doc.RootElement;

        string raiz = root.TryGetProperty("Raiz", out var raizProp) ? raizProp.GetString() ?? "" : "";
        string codigoPrincipal = root.TryGetProperty("CodigoPrincipal", out var codProp) ? codProp.GetString() ?? "" : "";

        if (string.IsNullOrWhiteSpace(raiz) || string.IsNullOrWhiteSpace(codigoPrincipal))
        {
            Console.WriteLine("Campos 'Raiz' ou 'CodigoPrincipal' não encontrados ou inválidos no libra.json.");
            return;
        }

        string caminho = Path.Combine(raiz, codigoPrincipal);

        if (!File.Exists(caminho))
        {
            Console.WriteLine($"Arquivo principal '{caminho}' não encontrado.");
            return;
        }

        string codigo = File.ReadAllText(caminho);

        var motor = new Libra.Api.MotorLibra();
        motor.Executar(codigo, codigoPrincipal, caminho);
    }
}