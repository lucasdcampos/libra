namespace Libra;

public static class LibraHelper
{
    public static Dictionary<string, object> Variaveis = new Dictionary<string, object>();
    public static int ROMPER = 1;
    public static int RETORNAR = 2;

    public static int Linha { get; set; }

    public static int BoolParaInt(bool valor)
    {
        if (valor) return 1;
        else       return 0;
    }
}