using Libra.Arvore;

namespace Libra
{
    public class Variavel
    {
            public string Identificador;
            public Token Token {get; private set; }
            public string Valor => Token.Valor;
            public bool Constante {get; private set;}
            public Variavel(string ident, Token token, bool constante = false)
            {
                Identificador = ident;
                Token = token;
                Constante = constante;
            }

    }
}