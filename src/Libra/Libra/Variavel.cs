using Libra.Arvore;

namespace Libra
{
    public class Variavel
    {
        public string Identificador { get; }
        public LibraObjeto Valor {get; private set; }
        public bool Constante { get; }
        public LibraTipo Tipo { get; private set; }
        public bool TipoModificavel { get; }

        public Variavel(string ident, LibraObjeto valor, bool constante = false, LibraTipo tipo = LibraTipo.Objeto, bool tipoModificavel = true)
        {
            Identificador = ident;
            Valor = valor;
            Constante = constante;
            TipoModificavel = tipoModificavel;
            Tipo = tipo;
        }

        public void AtualizarValor(LibraObjeto novoValor)
        {
            if (Constante)
                throw new ErroModificacaoConstante(Identificador, Interpretador.LocalAtual);

            if (novoValor.Tipo != Valor.Tipo && !TipoModificavel)
            {
                novoValor = novoValor.Converter(Valor.Tipo);

                if(Valor.Tipo != novoValor.Tipo)
                    throw new ErroTipoIncompativel(Identificador, Interpretador.LocalAtual);
            }

            Valor = novoValor;
            Tipo = novoValor.Tipo;
        }

        public override string ToString()
        {
            return $"{Identificador} (Valor: {Valor}, Constante: {Constante})";
        }
    }
}