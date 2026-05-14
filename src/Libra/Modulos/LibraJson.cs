using System.Reflection;
using System.Text.Json;
using Libra.Arvore;
using Libra.Runtime;

namespace Libra.Modulos;

public class LibraJson : IModulo
{
    public void RegistrarFuncoes(Ambiente ambiente)
    {
        ambiente.DefinirGlobal("json_ler", new FuncaoNativa(json_ler));
    }

    public static object json_ler(object[] args)
    {
        LibraUtil.ChecarArgumentos(MethodBase.GetCurrentMethod().Name, 2, args.Length);

        if (args[0] is not string json || args[1] is not string caminho)
            return false;

        try
        {
            using JsonDocument documento = JsonDocument.Parse(json);
            JsonElement atual = documento.RootElement;

            string[] partes = caminho.Split('.');

            foreach (string parte in partes)
            {
                if (!atual.TryGetProperty(parte, out atual))
                    return false;
            }

            return atual.ValueKind switch
            {
                JsonValueKind.String => atual.GetString(),
                JsonValueKind.Number => atual.GetRawText(),
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                JsonValueKind.Null => null,
                _ => atual.GetRawText()  // retorna como string JSON se for objeto ou array
            };
        }
        catch
        {
            return false;
        }
    }
}