using System.Reflection;
using Libra.Arvore;

namespace Libra.Modulos;

public class LibraSistema : IModulo
{
    public void RegistrarFuncoes(Programa programa = null)
    {
        Ambiente.DefinirGlobal("criar_dir", new FuncaoNativa(criar_dir));
        Ambiente.DefinirGlobal("excluir_dir", new FuncaoNativa(excluir_dir));
        Ambiente.DefinirGlobal("eh_dir", new FuncaoNativa(eh_dir));
        Ambiente.DefinirGlobal("eh_arquivo", new FuncaoNativa(eh_arquivo));
        Ambiente.DefinirGlobal("caminho_existe", new FuncaoNativa(caminho_existe));
        Ambiente.DefinirGlobal("ler_arquivo", new FuncaoNativa(ler_arquivo));
        Ambiente.DefinirGlobal("excluir_arquivo", new FuncaoNativa(excluir_arquivo));
        Ambiente.DefinirGlobal("criar_arquivo", new FuncaoNativa(criar_arquivo));
        Ambiente.DefinirGlobal("escrever_arquivo", new FuncaoNativa(escrever_arquivo));
    }

    public static object criar_dir(object[] args)
    {
        LibraUtil.ChecarArgumentos(MethodBase.GetCurrentMethod().Name, 1, args.Length);

        if (args[0] is not string caminho)
            return false;

        if (Directory.Exists(caminho)) return false;

        Directory.CreateDirectory(caminho);

        return true;
    }

    public static object excluir_dir(object[] args)
    {
        LibraUtil.ChecarArgumentos(MethodBase.GetCurrentMethod().Name, 1, args.Length);

        if (args[0] is not string caminho)
            return false;

        if (!Directory.Exists(caminho)) return false;

        Directory.Delete(caminho);

        return true;
    }

    public static object eh_dir(object[] args)
    {
        LibraUtil.ChecarArgumentos(MethodBase.GetCurrentMethod().Name, 1, args.Length);

        if (args[0] is not string caminho)
            return false;
        return Directory.Exists(caminho);
    }

    public static object eh_arquivo(object[] args)
    {
        LibraUtil.ChecarArgumentos(MethodBase.GetCurrentMethod().Name, 1, args.Length);

        if (args[0] is not string caminho)
            return false;
        return Directory.Exists(caminho);
    }

    public static object caminho_existe(object[] args)
    {
        LibraUtil.ChecarArgumentos(MethodBase.GetCurrentMethod().Name, 1, args.Length);

        if (args[0] is not string caminho)
            return false;

        return File.Exists(caminho) || Directory.Exists(caminho);
    }

    public static object ler_arquivo(object[] args)
    {
        LibraUtil.ChecarArgumentos(MethodBase.GetCurrentMethod().Name, 1, args.Length);

        if (args[0] is not string caminho)
            return false;

        if (!File.Exists(caminho))
            throw new ErroAcessoNulo($"{caminho} não existe");

        return File.ReadAllText(caminho);
    }

    public static object excluir_arquivo(object[] args)
    {
        LibraUtil.ChecarArgumentos(MethodBase.GetCurrentMethod().Name, 1, args.Length);

        if (args[0] is not string caminho)
            return false;

        if (!File.Exists(caminho))
            throw new ErroAcessoNulo($"{caminho} não existe");

        File.Delete(caminho);

        return null;
    }

    public static object criar_arquivo(object[] args)
    {
        LibraUtil.ChecarArgumentos(MethodBase.GetCurrentMethod().Name, 1, args.Length);

        if (args[0] is not string caminho)
            return false;

        if (File.Exists(caminho))
            return false;

        File.Create(caminho);

        return true;
    }

    public static object escrever_arquivo(object[] args)
    {
        LibraUtil.ChecarArgumentos(MethodBase.GetCurrentMethod().Name, 2, args.Length);

        if (args[0] is not string caminho)
            return false;

        if (args[1] is not string conteudo)
            return false;

        if (!File.Exists(caminho))
            throw new ErroAcessoNulo($"{caminho} não existe");

        File.WriteAllText(caminho, conteudo);

        return true;
    }
}