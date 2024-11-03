
namespace Libra.Arvore
{
    public abstract class Instrucao { }

    public class Escopo : Instrucao
    {
        public Escopo(List<Instrucao> instrucoes = null)
        {
            Instrucoes = instrucoes;

            if(instrucoes == null)
            {
                Instrucoes = new List<Instrucao>();
            }
        }
        
        public List<Instrucao> Instrucoes { get; internal set; }
    }

    public class InstrucaoSair : Instrucao
    {
        public InstrucaoSair(Expressao expressao)
        {
            Expressao = expressao;
        }

        public Expressao Expressao { get; private set; }

    }

    public class InstrucaoVar : Instrucao
    {
        public InstrucaoVar(string identificador, object valor, bool declaracao, TokenTipo tipo = TokenTipo.TokenInvalido, Expressao indice = null)
        {
            Valor = valor;
            Identificador = identificador;
            EhDeclaracao = declaracao;
            Tipo = tipo;   
            IndiceVetor = indice; 
        }

        public object Valor { get; private set; }
        public TokenTipo Tipo { get; private set; }
        public string Identificador {get; private set; }
        public Expressao IndiceVetor { get; private set; }
        internal bool EhDeclaracao; // usada para saber se estamos declarando uma nova variável ou só modificando uma

    }

    public class InstrucaoConst : Instrucao
    {
        public InstrucaoConst(string identificador, Expressao expressao)
        {
            Expressao = expressao;
            Identificador = identificador;
        }

        public Expressao Expressao { get; private set; }
        public string Identificador {get; private set; }
    }

    public class InstrucaoFuncao : Instrucao
    {
        public InstrucaoFuncao(string identificador, Escopo escopo, List<string> parametros = null)
        {
            Escopo = escopo;
            Identificador = identificador;
            Parametros = parametros;
        }

        public Escopo Escopo { get; private set; }
        public string Identificador {get; private set; }
        public List<string> Parametros { get; private set; }

    }

    public class InstrucaoSe : Instrucao
    {
        public InstrucaoSe(Expressao expressao, Escopo escopo, Escopo senaoEscopo = null)
        {
            Expressao = expressao;
            Escopo = escopo;
            SenaoEscopo = senaoEscopo;
        }

        public Expressao Expressao { get; private set; }
        public Escopo Escopo {get; private set; }
        public Escopo SenaoEscopo {get; private set; }

    }

    public class InstrucaoEnquanto : Instrucao
    {
        public InstrucaoEnquanto(Expressao expressao, Escopo escopo)
        {
            Expressao = expressao;
            Escopo = escopo;
        }

        public Expressao Expressao { get; private set; }
        public Escopo Escopo {get; private set; }

    }

    public class InstrucaoRomper : Instrucao
    {
        // Nada de interessante...
    }

    public class InstrucaoRetornar : Instrucao
    {
        public Expressao Expressao { get; private set; }

        public InstrucaoRetornar(Expressao expressao)
        {
            Expressao = expressao;
        }
    }

    public class InstrucaoChamadaFuncao : Instrucao
    {
        public string Identificador;
        public List<Expressao> Argumentos;

        public InstrucaoChamadaFuncao(string ident, List<Expressao> argumentos = null)
        {
            Identificador = ident;
            Argumentos = argumentos;

            if(Argumentos == null)
            {
                Argumentos = new List<Expressao>();
            }
        }
    }
}