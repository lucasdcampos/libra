namespace Libra.Arvore;

public class ExpressaoBinaria : Expressao
    {
        public Expressao Esquerda { get; private set; }
        public Token Operador { get; private set; }
        public Expressao Direita { get; private set; }

        public ExpressaoBinaria(LocalFonte local, Expressao esquerda, Token operador, Expressao direita)
        {
            Local = local;
            Esquerda = esquerda;
            Operador = operador;
            Direita = direita;
        }

        public override T Aceitar<T>(IVisitor<T> visitor) => visitor.VisitarExpressaoBinaria(this);

        public override string ToString()
        {
            return $"{Esquerda.ToString()} {Token.TipoParaString(Operador.Tipo)} {Direita.ToString()}";
        }
    }