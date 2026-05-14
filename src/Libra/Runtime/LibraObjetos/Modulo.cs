using System.Collections.Generic;

namespace Libra.Runtime;

public class Modulo : LibraObjeto
{
    public Modulo(string nome, Dictionary<string, Variavel> propriedades) : base(nome, null)
    {
        Propriedades = propriedades;
    }

    public override string Nome => "Modulo";

    public override object ObterValor()
    {
        return $"Módulo <{Nome}>";
    }

    public override string ToString()
    {
        return $"Módulo <{Nome}>";
    }
}
