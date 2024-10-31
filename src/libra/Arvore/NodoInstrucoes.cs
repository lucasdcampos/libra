
namespace Libra.Arvore
{
    public abstract class NodoInstrucao : Nodo {}

    public class NodoInstrucaoSair : NodoInstrucao
    {
        public NodoInstrucaoSair(NodoExpressao expressao)
        {
            Expressao = expressao;
        }

        public NodoExpressao Expressao { get; private set; }

    }

    public class NodoInstrucaoVar : NodoInstrucao
    {
        public NodoInstrucaoVar(string identificador, NodoExpressao expressao, bool declaracao)
        {
            Expressao = expressao;
            Identificador = identificador;
            EhDeclaracao = declaracao;
        }

        public NodoExpressao Expressao { get; private set; }
        public string Identificador {get; private set; }
        internal bool EhDeclaracao; // usada para saber se estamos declarando uma nova variável ou só modificando uma

    }

    public class NodoInstrucaoConst : NodoInstrucao
    {
        public NodoInstrucaoConst(string identificador, NodoExpressao expressao)
        {
            Expressao = expressao;
            Identificador = identificador;
        }

        public NodoExpressao Expressao { get; private set; }
        public string Identificador {get; private set; }
    }

    public class NodoInstrucaoFuncao : NodoInstrucao
    {
        public NodoInstrucaoFuncao(string identificador, NodoEscopo escopo, List<string> parametros = null)
        {
            Escopo = escopo;
            Identificador = identificador;
            Parametros = parametros;
        }

        public NodoEscopo Escopo { get; private set; }
        public string Identificador {get; private set; }
        public List<string> Parametros { get; private set; }

    }

    public class NodoInstrucaoSe : NodoInstrucao
    {
        public NodoInstrucaoSe(NodoExpressao expressao, NodoEscopo escopo, NodoEscopo senaoEscopo = null)
        {
            Expressao = expressao;
            Escopo = escopo;
            SenaoEscopo = senaoEscopo;
        }

        public NodoExpressao Expressao { get; private set; }
        public NodoEscopo Escopo {get; private set; }
        public NodoEscopo SenaoEscopo {get; private set; }

    }

    public class NodoInstrucaoEnquanto : NodoInstrucao
    {
        public NodoInstrucaoEnquanto(NodoExpressao expressao, NodoEscopo escopo)
        {
            Expressao = expressao;
            Escopo = escopo;
        }

        public NodoExpressao Expressao { get; private set; }
        public NodoEscopo Escopo {get; private set; }

    }

    public class NodoInstrucaoRomper() : NodoInstrucao
    {
        // Nada de interessante...
    }

    public class NodoInstrucaoRetornar() : NodoInstrucao
    {
        // Nada de interessante...
    }

    public class NodoInstrucaoChamadaFuncao : NodoInstrucao
    {
        public string Identificador;
        public List<NodoExpressao> Argumentos;

        public NodoInstrucaoChamadaFuncao(string ident, List<NodoExpressao> argumentos = null)
        {
            Identificador = ident;
            Argumentos = argumentos;

            if(Argumentos == null)
            {
                Argumentos = new List<NodoExpressao>();
            }
        }
    }
}