using Libra.Arvore;

namespace Libra
{
    public class Variavel
    {
        public string Identificador { get; }
        public LibraObjeto Valor {get; private set; }
        public bool Constante { get; }
        public bool TipoModificavel { get; }

        public Variavel(string ident, LibraObjeto valor, bool constante = false, bool tipoModificavel = true)
        {
            Identificador = ident;
            Valor = valor;
            Constante = constante;
            TipoModificavel = tipoModificavel;
        }

        public void AtualizarValor(LibraObjeto novoValor)
        {
            if (Constante)
                throw new ErroModificacaoConstante(Identificador, Interpretador.LocalAtual);

            if (novoValor.Tipo != Valor.Tipo && !TipoModificavel)
                throw new ErroTipoIncompativel(Identificador, Interpretador.LocalAtual);

            Valor = novoValor;
        }

        public override string ToString()
        {
            return $"{Identificador} (Valor: {Valor}, Constante: {Constante})";
        }
    }
}