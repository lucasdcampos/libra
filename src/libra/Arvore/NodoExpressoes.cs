namespace Libra.Arvore
{
    public abstract class NodoExpressao : Nodo { }

    public class NodoExpressaoTermo : NodoExpressao
    {
        public NodoExpressaoTermo(Token token)
        {
            Token = token;
        }

        public Token Token {get; private set;}

        public string Valor => Token.Valor;        
    }

    public class NodoExpressaoBinaria : NodoExpressao
    {
        public NodoExpressao Esquerda { get; private set; }
        public Token Operador { get; private set; }
        public NodoExpressao Direita { get; private set; }

        public NodoExpressaoBinaria(NodoExpressao esquerda, Token operador, NodoExpressao direita)
        {
            Esquerda = esquerda;
            Operador = operador;
            Direita = direita;
        }

    }
    
}