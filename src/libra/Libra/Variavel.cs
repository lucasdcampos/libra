using Libra.Arvore;

namespace Libra
{
    public class Variavel
    {
        public string Identificador { get; }
        public object Valor {get; private set; }
        public bool Constante { get; }

        public Variavel(string ident, object valor, bool constante = false)
        {
            Identificador = ident;
            Valor = valor;
            Constante = constante;
        }

        public void AtualizarValor(object novoValor)
        {
            if (Constante)
                throw new ErroModificacaoConstante(Identificador);

            Valor = novoValor;
        }

        public override string ToString()
        {
            return $"{Identificador} (Valor: {Valor}, Constante: {Constante})";
        }
    }
}