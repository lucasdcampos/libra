using Libra.Arvore;

namespace Libra
{
    public class Variavel
    {
        public string Identificador { get; }
        public Token Token { get; private set; }
        public TokenTipo Tipo => Token.Tipo;
        public object Valor => Token.Valor;
        public bool Constante { get; }

        public Variavel(string ident, Token token, bool constante = false)
        {
            Identificador = ident;
            Token = token;
            Constante = constante;
        }

        public void AtualizarValor(object novoValor)
        {
            if (Constante)
                new ErroModificacaoConstante(Identificador).LancarErro();

            if (novoValor.GetType() != Valor.GetType())
                new ErroTipoIncompativel(Identificador).LancarErro();

            Token = new Token(Token.Tipo, Token.Linha, novoValor);
        }

        public override string ToString()
        {
            return $"{Identificador} (Tipo: {Tipo}, Valor: {Valor}, Constante: {Constante})";
        }
    }

}