using System.Reflection;

namespace Libra;

/// <summary>
/// Acesso à biblioteca padrão da Libra (vetores, matematica, etc.) embutida no
/// assembly como recurso incorporado.
///
/// Serve de fallback quando não há sistema de arquivos para resolver os módulos
/// — por exemplo, no playground que roda em WebAssembly no navegador. Os arquivos
/// são incluídos via &lt;EmbeddedResource&gt; em Libra.csproj com o prefixo lógico
/// "Biblioteca/".
/// </summary>
public static class BibliotecaEmbutida
{
    private static readonly Assembly _assembly = typeof(BibliotecaEmbutida).Assembly;
    private const string Prefixo = "Biblioteca/";

    /// <summary>
    /// Tenta ler o código-fonte de um módulo da biblioteca padrão embutida.
    /// Aceita o nome com ou sem a extensão .libra (ex.: "vetores" ou "vetores.libra").
    /// Só resolve nomes simples (sem separador de diretório), que é o formato dos
    /// módulos padrão — caminhos de pacotes/relativos continuam sendo do filesystem.
    /// </summary>
    /// <returns><c>true</c> se o módulo existe embutido; nesse caso preenche os parâmetros de saída.</returns>
    public static bool TryLer(string nomeLogico, out string codigo, out string nomeArquivo)
    {
        codigo = null;
        nomeArquivo = null;

        if (string.IsNullOrEmpty(nomeLogico) || nomeLogico.IndexOfAny(new[] { '/', '\\' }) >= 0)
            return false;

        string arquivo = nomeLogico.EndsWith(".libra") ? nomeLogico : nomeLogico + ".libra";

        using var stream = _assembly.GetManifestResourceStream(Prefixo + arquivo);
        if (stream == null)
            return false;

        using var reader = new StreamReader(stream);
        codigo = reader.ReadToEnd().ReplaceLineEndings("\n");
        nomeArquivo = arquivo;
        return true;
    }
}
