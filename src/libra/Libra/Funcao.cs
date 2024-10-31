using Libra.Arvore;

namespace Libra
{
    public class Funcao
    {
            public string Identificador;
            public NodoEscopo Escopo;
            public List<string> Parametros;

            public Funcao(string ident, NodoEscopo escopo, List<string> parametros)
            {
                Identificador = ident;
                Escopo = escopo;
                Parametros = parametros;
            }

    }
}