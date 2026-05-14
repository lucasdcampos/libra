using System.Reflection;
using Libra.Arvore;
using Libra.Runtime;

namespace Libra.Modulos;

public class LibraTempo : IModulo
{
    public void RegistrarFuncoes(Ambiente ambiente)
    {
        ambiente.DefinirGlobal("tempo_ms", new FuncaoNativa(tempo_ms));
    }

    public static object tempo_ms(object[] args)
    {
        return (double)DateTimeOffset.Now.ToUnixTimeMilliseconds();
    }
}
