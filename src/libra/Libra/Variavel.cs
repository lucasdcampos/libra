using Libra.Arvore;

namespace Libra
{
    public class Variavel
    {
            public string Identificador;
            public string Valor;

            public Nodo Nodo { get; private set; }

            public Variavel(string identificador, Nodo nodo)
            {
                if(nodo.GetType().IsSubclassOf(typeof(NodoExpressao)) ||
                nodo.GetType() == typeof(NodoString))
                {
                    Nodo = nodo;
                }
                else
                {
                    Erro.ErroGenerico($"Não é possível atribuir {nodo.GetType()} à {identificador}");
                }

                Identificador = identificador;
                Valor = Nodo.Avaliar().ToString();
            }

    }
}