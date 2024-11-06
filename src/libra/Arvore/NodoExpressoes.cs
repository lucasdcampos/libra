namespace Libra.Arvore
{
    public abstract class Expressao { }

    // TODO: melhorar isso!
    public class ExpressaoUnaria : Expressao
    {
        public ExpressaoUnaria(Token token)
        {
            Token = token;
        }

        public ExpressaoUnaria(ExpressaoChamadaFuncao chamada)
        {
            ChamadaFuncao = chamada;
        }

        public ExpressaoUnaria(ExpressaoAcessarVetor vetor)
        {
            AcessoVetor = vetor;
        }

        public ExpressaoChamadaFuncao ChamadaFuncao { get; private set; }
        public ExpressaoAcessarVetor AcessoVetor { get; private set; }

        public Token Token { get; private set; }

        public object Valor => Token.Valor;

        public override string ToString()
        {
            if(Token != null)
                return Token.Valor.ToString();
            
            return ChamadaFuncao.Identificador + "()"; // TODO: Printar os argumentos
        }
    }

    public class ExpressaoChamadaFuncao : Expressao
    {
        public string Identificador;
        public List<Expressao> Argumentos;

        public ExpressaoChamadaFuncao(string ident, List<Expressao> argumentos = null)
        {
            Identificador = ident;
            Argumentos = argumentos;

            if(Argumentos == null)
            {
                Argumentos = new List<Expressao>();
            }
        }
    }

    public class ExpressaoAcessarVetor : Expressao
    {
        public ExpressaoAcessarVetor(string ident, Expressao expr)
        {
            Identificador = ident;
            Expressao = expr;
        }
        public string Identificador {  get; private set; }
        public Expressao Expressao { get; private set; }
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

        public override string ToString()
        {
            return $"{Esquerda.ToString()} {Token.TipoParaString(Operador.Tipo)} {Direita.ToString()}";
        }

    }
    
}