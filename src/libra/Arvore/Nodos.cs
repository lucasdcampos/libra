namespace Libra.Arvore
{
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