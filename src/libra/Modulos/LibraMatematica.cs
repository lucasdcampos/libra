using System.Reflection;
using Libra.Arvore;

namespace Libra.Modulos;

public class LibraMatematica : IModulo
{
    private Programa _programa;
    private Random random = new Random();

    public void RegistrarFuncoes(Programa programa)
    {
        _programa = programa;

        _programa.Funcoes["__mathsin__"] = new FuncaoNativa(__mathsin__);
        _programa.Funcoes["__mathcos__"] = new FuncaoNativa(__mathcos__);
        _programa.Funcoes["__mathtan__"] = new FuncaoNativa(__mathtan__);
        _programa.Funcoes["__mathrand__"] = new FuncaoNativa(__mathsin__);
    }

    public static object __mathsin__(object[] args)
    {
        LibraUtil.ChecarArgumentos(MethodBase.GetCurrentMethod().Name, 1, args.Length);

        if(args[0] is not double angl)
            return null;
        
        return Math.Sin(angl);
    }

    public static object __mathcos__(object[] args)
    {
        LibraUtil.ChecarArgumentos(MethodBase.GetCurrentMethod().Name, 1, args.Length);

        if(args[0] is not double angl)
            return null;
        
        return Math.Cos(angl);
    }

    public static object __mathtan__(object[] args)
    {
        LibraUtil.ChecarArgumentos(MethodBase.GetCurrentMethod().Name, 1, args.Length);

        if(args[0] is not double angl)
            return null;
        
        return Math.Tan(angl);
    }

    public object __mathrand__(object[] args)
    {
        LibraUtil.ChecarArgumentos(MethodBase.GetCurrentMethod().Name, 0, args.Length);

        return random.Next();
    }
}