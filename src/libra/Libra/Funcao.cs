using Libra.Arvore;

namespace Libra
{
    public class Funcao
    {
            public string Identificador;
            public Escopo Escopo;
            public List<string> Parametros;

            public Funcao(string ident, Escopo escopo, List<string> parametros)
            {
                Identificador = ident;
                Escopo = escopo;
                Parametros = parametros;
            }

    }
}