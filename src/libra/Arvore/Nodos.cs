namespace Libra.Arvore
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

    public abstract class Nodo
    {
        // cada nó retornará um tipo de valor diferente
        public abstract object Avaliar();
    }

    public class NodoPrograma : Nodo
    {
        public NodoPrograma(List<NodoInstrucao> instrucoes)
        {
            Instrucoes = instrucoes;
        }

        public List<NodoInstrucao> Instrucoes { get; private set; }

        public int CodigoSaida { get; private set; }

        public override object Avaliar()
        {
            return CodigoSaida;
        }

    }
}