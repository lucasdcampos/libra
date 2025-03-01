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
}