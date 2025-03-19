using System.Reflection;

namespace Libra;

public static class LibraUtil
{
    public static int BoolParaInt(bool valor)
    {
        if (valor) return 1;
        else       return 0;
    }

    public static int NegarInteiroLogico(int valor)
    {
        return valor != 0 ? 0 : 1;
    }

    internal static void ChecarArgumentos(string ident, int esperado, int recebido)
    {
        if(esperado != recebido)
            throw new ErroEsperadoNArgumentos(ident, esperado, recebido);
    }

    public static string VersaoAtual()
    {
        var assembly = Assembly.GetExecutingAssembly();

        return assembly.GetName().Version.ToString();
    }

    public static LibraTipo PegarTipo(string tipo)
    {
        switch(tipo)
        {
            case "Int": return LibraTipo.Int;
            case "Real": return LibraTipo.Real;
            case "Texto": return LibraTipo.Texto;
            case "Vetor": return LibraTipo.Vetor;
            case "Objeto": return LibraTipo.Objeto;
            default:
                throw new Erro($"Tipo desconhecido `{tipo}`", Interpretador.LocalAtual);
        }
        return LibraTipo.Nulo;
    }
}