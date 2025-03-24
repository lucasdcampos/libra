namespace Libra.Arvore
{
    public enum TipoExpressao
    {
        ExpressaoLiteral,
        ExpressaoVariavel,
        ExpressaoPropriedade,
        ExpressaoChamadaFuncao,
        ExpressaoChamadaMetodo,
        ExpressaoNovoVetor,
        ExpressaoInicializacaoVetor,
        ExpressaoAcessoVetor,
        ExpressaoUnaria,
        ExpressaoBinaria
    }

    public abstract class Expressao : Instrucao
    {
        public TipoExpressao TipoExpr { get; protected set ;}
        public abstract object Aceitar(IVisitor visitor);
    }

    public class ExpressaoUnaria : Expressao
    {
        public Token Operador { get; private set; }
        public Expressao Operando { get; private set; }

        public ExpressaoUnaria(Token operador, Expressao operando)
        {
            Operador = operador;
            Operando = operando;

            TipoExpr = TipoExpressao.ExpressaoUnaria;
        }

        public override object Aceitar(IVisitor visitor) => visitor.VisitarExpressaoUnaria(this);

    }

    public class ExpressaoLiteral : Expressao
    {
        public ExpressaoLiteral(Token token)
        {
            Token = token;
            TipoExpr = TipoExpressao.ExpressaoLiteral;
        }

        public Token Token { get; private set; }
        public object Valor => Token.Valor;

        public override object Aceitar(IVisitor visitor) => visitor.VisitarExpressaoLiteral(this);

        public static ExpressaoLiteral CriarInt(LocalToken local, int valor)
        {
            return new ExpressaoLiteral(new Token(TokenTipo.NumeroLiteral, local, valor));
        }
    }

    public class ExpressaoVariavel : Expressao
    {
        public Token Identificador { get; private set ;}

        public ExpressaoVariavel(Token identificador)
        {
            Identificador = identificador;
            TipoExpr = TipoExpressao.ExpressaoVariavel;
        }

        public override object Aceitar(IVisitor visitor) => visitor.VisitarExpressaoVariavel(this);
    }

    public class ExpressaoPropriedade : Expressao
    {
        public string Identificador { get; private set ;}
        public string Propriedade { get; private set ;}
        public ExpressaoPropriedade(string identificador, string prop)
        {
            Identificador = identificador;
            Propriedade = prop;
            TipoExpr = TipoExpressao.ExpressaoPropriedade;
        }

        public override object Aceitar(IVisitor visitor) => visitor.VisitarExpressaoPropriedade(this);
    }

    public class ExpressaoChamadaFuncao : Expressao
    {
        public string Identificador { get; private set; }
        public Expressao[] Argumentos { get; private set; }

        public ExpressaoChamadaFuncao(string ident, Expressao[] argumentos = null)
        {
            Tipo = TipoInstrucao.Chamada;
            Identificador = ident;
            Argumentos = argumentos;

            if(Argumentos == null)
            {
                Argumentos = new Expressao[0];
            }

            TipoExpr = TipoExpressao.ExpressaoChamadaFuncao;
        }

        public override object Aceitar(IVisitor visitor) => visitor.VisitarExpressaoChamadaFuncao(this);
    }

    public class ExpressaoChamadaMetodo : Expressao
    {
        public string Identificador { get; private set; }
        public ExpressaoChamadaFuncao Chamada { get; private set ;}

        public ExpressaoChamadaMetodo(string ident, ExpressaoChamadaFuncao chamada)
        {
            Tipo = TipoInstrucao.ChamadaMetodo;
            TipoExpr = TipoExpressao.ExpressaoChamadaMetodo;
            Identificador = ident;
            Chamada = chamada;
        }

        public override object Aceitar(IVisitor visitor) => visitor.VisitarExpressaoChamadaMetodo(this);
    }

    public class ExpressaoAcessoVetor : Expressao
    {
        public ExpressaoAcessoVetor(string ident, Expressao expr)
        {
            Identificador = ident;
            Expressao = expr;
            TipoExpr = TipoExpressao.ExpressaoAcessoVetor;
        }
        public string Identificador {  get; private set; }
        public Expressao Expressao { get; private set; }

        public override object Aceitar(IVisitor visitor) => visitor.VisitarExpressaoAcessoVetor(this);
    }

    public class ExpressaoNovoVetor : Expressao
    {
        public ExpressaoNovoVetor(Expressao expr)
        {
            Expressao = expr;
            TipoExpr = TipoExpressao.ExpressaoNovoVetor;
        }

        public Expressao Expressao { get; private set; }

        public override object Aceitar(IVisitor visitor) => visitor.VisitarExpressaoNovoVetor(this);
    }

    public class ExpressaoInicializacaoVetor : Expressao
    {
        public ExpressaoInicializacaoVetor(List<Expressao> expressoes)
        {
            Expressoes = expressoes;
            TipoExpr = TipoExpressao.ExpressaoInicializacaoVetor;
        }

        public List<Expressao> Expressoes { get; private set; }

        public override object Aceitar(IVisitor visitor) => visitor.VisitarExpressaoInicializacaoVetor(this);
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

            TipoExpr = TipoExpressao.ExpressaoBinaria;
        }

        public override object Aceitar(IVisitor visitor) => visitor.VisitarExpressaoBinaria(this);

        public override string ToString()
        {
            return $"{Esquerda.ToString()} {Token.TipoParaString(Operador.Tipo)} {Direita.ToString()}";
        }
    }
}