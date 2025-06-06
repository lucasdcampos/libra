using System.Reflection;
using Libra.Arvore;

namespace Libra.Modulos;

public class LibraMatematica : IModulo
{
    private Random random = new Random();

    public void RegistrarFuncoes(Programa programa = null)
    {
        Ambiente.DefinirGlobal("__csraiz__", new FuncaoNativa(__csraiz__));
        Ambiente.DefinirGlobal("__csraizq__", new FuncaoNativa(__csraizq__));
        Ambiente.DefinirGlobal("__cssen__", new FuncaoNativa(__cssen__));
        Ambiente.DefinirGlobal("__cscos__", new FuncaoNativa(__cscos__));
        Ambiente.DefinirGlobal("__cstan__", new FuncaoNativa(__cstan__));
        Ambiente.DefinirGlobal("__csrand__", new FuncaoNativa(__csrand__));
        Ambiente.DefinirGlobal("__cslog__", new FuncaoNativa(__cslog__));
    }

    public static object __csraizq__(object[] args)
    {
        LibraUtil.ChecarArgumentos(MethodBase.GetCurrentMethod().Name, 1, args.Length);

        if(args[0] is not double x)
            return null;
        
        return Math.Sqrt(x);
    }

    public static object __csraiz__(object[] args)
    {
        LibraUtil.ChecarArgumentos(MethodBase.GetCurrentMethod().Name, 2, args.Length);

        if(args[0] is not double x || args[1] is not double n)
            return null;
        
        return Math.Pow(x, 1.0 / n);
    }

    public static object __cssen__(object[] args)
    {
        LibraUtil.ChecarArgumentos(MethodBase.GetCurrentMethod().Name, 1, args.Length);

        if(args[0] is not double angl)
            return null;
        
        return Math.Sin(angl);
    }

    public static object __cscos__(object[] args)
    {
        LibraUtil.ChecarArgumentos(MethodBase.GetCurrentMethod().Name, 1, args.Length);

        if(args[0] is not double angl)
            return null;
        
        return Math.Cos(angl);
    }

    public static object __cstan__(object[] args)
    {
        LibraUtil.ChecarArgumentos(MethodBase.GetCurrentMethod().Name, 1, args.Length);

        if(args[0] is not double angl)
            return null;
        
        return Math.Tan(angl);
    }

    public static object __cslog__(object[] args)
    {
        LibraUtil.ChecarArgumentos(MethodBase.GetCurrentMethod().Name, 2, args.Length);

        if(args[0] is double n && args[1] is double _base)
            return Math.Log(n, _base);

        return null;
    }

    public object __csrand__(object[] args)
    {
        LibraUtil.ChecarArgumentos(MethodBase.GetCurrentMethod().Name, 2, args.Length);

        if(args[0] is double min && args[1] is double max)
            return random.NextDouble() * (max - min) + min;

        return null;
    }
}