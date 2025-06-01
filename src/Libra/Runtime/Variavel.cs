using Libra.Arvore;

namespace Libra
{
    public class Variavel
    {
        public string Identificador { get; }
        public LibraObjeto Valor {get; private set; }
        public bool Constante { get; }
        public string Tipo { get; private set; }

        public bool Referenciada { get; internal set; }

        public Variavel(string ident, LibraObjeto valor, string tipo, bool constante)
        {
            Identificador = ident;
            Valor = valor;
            Constante = constante;
            Tipo = tipo;
            Referenciada = false;
        }

        public void AtualizarValor(LibraObjeto novoValor)
        {
            Referenciada = true;

            if (Constante)
                throw new ErroModificacaoConstante(Identificador, Interpretador.LocalAtual);

            bool tiposDiferentes = novoValor.Nome != Valor.Nome;
            bool tipoModificavel = Tipo == TiposPadrao.Objeto;

            // Tentando alterar o tipo da variável
            if (tiposDiferentes && !tipoModificavel)
            {
                // Tentando converter para o tipo base
                // Ex: Se o tipo base é Real, mas recebemos um Int,
                // então convertemos o Int para Real (O contrário não ocorre)
                novoValor= novoValor.Converter(Valor.Nome);
            }

            Valor = novoValor;
        }

        public override string ToString()
        {
            return $"{Identificador} (Valor: {Valor}, Constante: {Constante})";
        }
    }
}