using Libra.Runtime;
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
                throw new ErroModificacaoConstante(Identificador);

            bool tiposDiferentes = novoValor.Nome != Valor.Nome;
            bool tipoModificavel = Tipo == TiposPadrao.Objeto || Valor.Nome == "Nulo";

            // Tentando alterar o tipo da variável
            if (tiposDiferentes && !tipoModificavel)
            {
                // Se o tipo da variável é o nome da classe do novo objeto, permitimos
                if (Tipo == novoValor.Nome)
                {
                    Valor = novoValor;
                    return;
                }

                // Tentando converter para o tipo esperado pela variável
                novoValor = novoValor.Converter(Tipo);
            }

            Valor = novoValor;
            if (tipoModificavel && Valor.Nome != "Nulo" && Tipo == "Nulo")
            {
                // Se a variável era Nulo e agora tem um valor, ela assume o tipo do valor
                // para manter a consistência se não for um tipo genérico 'Objeto'
            }
        }

        public override string ToString()
        {
            return $"{Identificador} (Valor: {Valor}, Constante: {Constante})";
        }
    }
}