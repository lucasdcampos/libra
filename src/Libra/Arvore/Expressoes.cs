using System;
using Libra.Runtime;

namespace Libra.Arvore
{
    public abstract class Expressao : Nodo<LibraObjeto> { }

    public class ExpressaoUnaria : Expressao
    {
        public Token Operador { get; private set; }
        public Expressao Operando { get; private set; }

        public ExpressaoUnaria(LocalFonte local, Token operador, Expressao operando)
        {
            Local = local;
            Operador = operador;
            Operando = operando;
        }
        public override LibraObjeto Aceitar(IVisitor visitor) => visitor.VisitarExpressaoUnaria(this);
    }

    public class ExpressaoLiteral : Expressao
    {
        public ExpressaoLiteral(LocalFonte local, LibraObjeto valor)
        {
            Local = local;
            Valor = valor;
        }

        public LibraObjeto Valor { get; private set; }

        public override LibraObjeto Aceitar(IVisitor visitor) => visitor.VisitarExpressaoLiteral(this);

        public static ExpressaoLiteral CriarInt(LocalFonte local, int valor)
        {
            return new ExpressaoLiteral(local, new LibraInt(valor));
        }

        public static ExpressaoLiteral CriarTexto(LocalFonte local, string valor)
        {
            return new ExpressaoLiteral(local, new LibraTexto(valor));
        }
    }

    public class ExpressaoVariavel : Expressao
    {
        public Token Identificador { get; private set ;}

        public ExpressaoVariavel(LocalFonte local, Token identificador)
        {
            Local = local;
            Identificador = identificador;
        }

        public override LibraObjeto Aceitar(IVisitor visitor) => visitor.VisitarExpressaoVariavel(this);
    }

    public class ExpressaoPropriedade : Expressao
    {
        public Expressao Alvo { get; private set ;}
        public string Propriedade { get; private set ;}
        public ExpressaoPropriedade(LocalFonte local, Expressao alvo, string prop)
        {
            Local = local;
            Alvo = alvo;
            Propriedade = prop;
        }

        public override LibraObjeto Aceitar(IVisitor visitor) => visitor.VisitarExpressaoPropriedade(this);
    }

    public class ExpressaoChamadaFuncao : Expressao
    {
        public string Identificador { get; private set; }
        public Expressao[] Argumentos { get; private set; }

        public ExpressaoChamadaFuncao(LocalFonte local, string ident, Expressao[] argumentos = null)
        {
            Local = local;
            Identificador = ident;
            Argumentos = argumentos;

            if(Argumentos == null)
            {
                Argumentos = new Expressao[0];
            }
        }

        public override LibraObjeto Aceitar(IVisitor visitor) => visitor.VisitarExpressaoChamadaFuncao(this);
    }

    public class ExpressaoChamadaMetodo : Expressao
    {
        public Expressao Alvo { get; private set; }
        public ExpressaoChamadaFuncao Chamada { get; private set ;}

        public ExpressaoChamadaMetodo(LocalFonte local, Expressao alvo, ExpressaoChamadaFuncao chamada)
        {
            Local = local;
            Alvo = alvo;
            Chamada = chamada;
        }

        public override LibraObjeto Aceitar(IVisitor visitor) => visitor.VisitarExpressaoChamadaMetodo(this);
    }

    public class ExpressaoAcessoVetor : Expressao
    {
        public ExpressaoAcessoVetor(LocalFonte local, string ident, Expressao expr)
        {
            Local = local;
            Identificador = ident;
            Expressao = expr;
        }
        public string Identificador {  get; private set; }
        public Expressao Expressao { get; private set; }

        public override LibraObjeto Aceitar(IVisitor visitor) => visitor.VisitarExpressaoAcessoVetor(this);
    }

    public class ExpressaoNovoVetor : Expressao
    {
        public ExpressaoNovoVetor(LocalFonte local, Expressao expr)
        {
            Local = local;
            Expressao = expr;
        }

        public Expressao Expressao { get; private set; }

        public override LibraObjeto Aceitar(IVisitor visitor) => visitor.VisitarExpressaoNovoVetor(this);
    }

    public class ExpressaoInicializacaoVetor : Expressao
    {
        public ExpressaoInicializacaoVetor(LocalFonte local, List<Expressao> expressoes)
        {
            Local = local;
            Expressoes = expressoes;
        }

        public List<Expressao> Expressoes { get; private set; }

        public override LibraObjeto Aceitar(IVisitor visitor) => visitor.VisitarExpressaoInicializacaoVetor(this);
    }

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

        public override LibraObjeto Aceitar(IVisitor visitor) => visitor.VisitarExpressaoBinaria(this);

        public override string ToString()
        {
            return $"{Esquerda.ToString()} {Token.TipoParaString(Operador.Tipo)} {Direita.ToString()}";
        }
    }
}