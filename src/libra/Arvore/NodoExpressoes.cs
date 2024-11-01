namespace Libra.Arvore
{
    public abstract class Expressao { }

    public class ExpressaoTermo : Expressao
    {
        public ExpressaoTermo(Token token)
        {
            Token = token;
        }

        public Token Token {get; private set;}

        public string Valor => Token.Valor;        
    }

    public class ExpressaoBinaria : Expressao
    {
        public Expressao Esquerda { get; private set; }
        public Token Operador { get; private set; }
        public Expressao Direita { get; private set; }

        public ExpressaoBinaria(Expressao esquerda, Token operador, Expressao direita)
        {
            Esquerda = esquerda;
            Operador = operador;
            Direita = direita;
        }

    }
    
}