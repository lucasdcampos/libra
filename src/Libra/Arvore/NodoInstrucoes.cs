
using System;

namespace Libra.Arvore
{
    public abstract class Instrucao 
    { 
        public TokenTipo TipoInstrucao { get; protected set; }

        public LocalToken Local {get; protected set; }
    }

    public class InstrucaoVar : Instrucao
    {
        public InstrucaoVar(LocalToken local, string identificador, Expressao expressao, bool constante, bool declaracao, bool tipoModificavel)
        {
            Identificador = identificador;
            EhDeclaracao = declaracao;
            Constante = constante;
            Expressao = expressao; 
            TipoModificavel = tipoModificavel;
            TipoInstrucao = TokenTipo.Var;
            Local = local;
        }

        public Expressao Expressao { get; private set; }
        public TokenTipo Tipo { get; private set; }
        public string Identificador {get; private set; }
        internal bool EhDeclaracao;
        internal bool TipoModificavel;
        public bool Constante { get; private set; }
    }

    public class InstrucaoFuncao : Instrucao
    {
        public InstrucaoFuncao(LocalToken local, string identificador, Instrucao[] instrucoes, List<string> parametros = null)
        {
            Instrucoes = instrucoes;
            Identificador = identificador;
            Parametros = parametros;
            TipoInstrucao = TokenTipo.Funcao;
            Local = local;
        }

        public Instrucao[] Instrucoes { get; private set; }
        public string Identificador {get; private set; }
        public List<string> Parametros { get; private set; }

    }

    public class InstrucaoSe : Instrucao
    {
        public InstrucaoSe(LocalToken local, Expressao condicao, Instrucao[] corpo, InstrucaoSenaoSe[] listaSenaoSe = null)
        {
            Condicao = condicao ?? throw new ArgumentNullException(nameof(condicao));
            Corpo = corpo ?? throw new ArgumentNullException(nameof(corpo));
            ListaSenaoSe = listaSenaoSe;
            TipoInstrucao = TokenTipo.Se;
            Local = local;
        }

        public Expressao Condicao { get; private set; }
        public IReadOnlyList<Instrucao> Corpo { get; private set; }
        public IReadOnlyList<InstrucaoSenaoSe> ListaSenaoSe { get; private set; }
    }

    public class InstrucaoSenaoSe : Instrucao
    {
        public InstrucaoSenaoSe(LocalToken local, Expressao condicao, Instrucao[] corpo)
        {
            Condicao = condicao ?? throw new ArgumentNullException(nameof(condicao));
            Corpo = corpo ?? throw new ArgumentNullException(nameof(corpo));
            TipoInstrucao = TokenTipo.SenaoSe;
            Local = local;
        }

        public Expressao Condicao { get; private set; }
        public IReadOnlyList<Instrucao> Corpo { get; private set; }
    }


    public class InstrucaoEnquanto : Instrucao
    {
        public InstrucaoEnquanto(LocalToken local, Expressao expressao, Instrucao[] instrucoes)
        {
            Expressao = expressao;
            Instrucoes = instrucoes;
            TipoInstrucao = TokenTipo.Enquanto;
            Local = local;
        }

        public Expressao Expressao { get; private set; }
        public Instrucao[] Instrucoes {get; private set; }

    }

    // Não necessitam de implementação
    public class InstrucaoRomper : Instrucao
    {
        public InstrucaoRomper(LocalToken local)
        {
            TipoInstrucao = TokenTipo.Romper;
            Local = local;
        }
    }
    public class InstrucaoContinuar : Instrucao
    {
        public InstrucaoContinuar(LocalToken local)
        {
            TipoInstrucao = TokenTipo.Continuar;
            Local = local;
        }
    }

    public class InstrucaoRetornar : Instrucao
    {
        public Expressao Expressao { get; private set; }

        public InstrucaoRetornar(LocalToken local, Expressao expressao)
        {
            Expressao = expressao;
            TipoInstrucao = TokenTipo.Retornar;
            Local = local;
        }
    }

    public class InstrucaoChamadaFuncao : Instrucao
    {
        public InstrucaoChamadaFuncao(LocalToken local, ExpressaoChamadaFuncao chamada)
        {
            Chamada = chamada;
            TipoInstrucao = TokenTipo.Identificador;
            Local = local;
        }

        public ExpressaoChamadaFuncao Chamada { get; private set; }
    }

    public class InstrucaoModificacaoVetor : Instrucao
    {
        public InstrucaoModificacaoVetor(LocalToken local, string identificador, Expressao indiceExpr, Expressao expressao)
        {
            Local = local;
            Expressao = expressao;
            Identificador = identificador;
            ExpressaoIndice = indiceExpr;
            TipoInstrucao = TokenTipo.Vetor;
        }

        public string Identificador { get; private set; }
        public Expressao ExpressaoIndice { get; private set; }
        public Expressao Expressao { get; private set; }
    }
}