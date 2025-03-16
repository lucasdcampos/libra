using Libra.Arvore;

namespace Libra
{
    public class Variavel
    {
        public string Identificador { get; }
        public LibraObjeto Valor {get; private set; }
        public bool Constante { get; }

        public Variavel(string ident, LibraObjeto valor, bool constante = false)
        {
            Identificador = ident;
            Valor = valor;
            Constante = constante;
        }

        public void AtualizarValor(LibraObjeto novoValor)
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