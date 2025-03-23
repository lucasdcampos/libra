
using System;

namespace Libra.Arvore
{
    public enum TipoInstrucao
    {
        DeclVar,
        DeclFunc,
        DeclTipo,
        AtribVar,
        Chamada,
        Se,
        SenaoSe,
        Enquanto,
        Romper,
        Continuar,
        Retornar
    }
    public abstract class Instrucao 
    { 
        public TipoInstrucao Tipo { get; protected set; }

        public LocalToken Local {get; protected set; }
    }

    public class InstrucaoVar : Instrucao
    {
        public InstrucaoVar(LocalToken local, string identificador, Expressao expressao, bool constante, bool declaracao, LibraTipo tipo,bool tipoModificavel)
        {
            Identificador = identificador;
            EhDeclaracao = declaracao;
            Constante = constante;
            Expressao = expressao; 
            TipoModificavel = tipoModificavel;
            Tipo = TipoInstrucao.DeclVar;
            Local = local;
            TipoVar = tipo;
        }

        public Expressao Expressao { get; private set; }
        public string Identificador {get; private set; }
        internal bool EhDeclaracao;
        internal LibraTipo TipoVar;
        internal bool TipoModificavel;
        public bool Constante { get; private set; }
    }

    public class AtribuicaoVariavel : Instrucao
    {
        public Expressao Identificador { get; }
        public Expressao Expressao { get; }
        public AtribuicaoVariavel(Expressao expr)
        {
            Expressao = expr;
            Tipo = TipoInstrucao.AtribVar;
        }
    }

    public class InstrucaoFuncao : Instrucao
    {
        public InstrucaoFuncao(LocalToken local, string identificador, Instrucao[] instrucoes, List<Parametro> parametros = null, LibraTipo tipoRetorno = LibraTipo.Objeto)
        {
            Tipo = TipoInstrucao.DeclFunc;
            Instrucoes = instrucoes;
            Identificador = identificador;
            Parametros = parametros;
            Local = local;
            TipoRetorno = tipoRetorno;
        }

        public Instrucao[] Instrucoes { get; private set; }
        public string Identificador {get; private set; }
        public List<Parametro> Parametros { get; private set; }
        public LibraTipo TipoRetorno { get; private set; }
    }

    public class InstrucaoTipo : Instrucao
    {
        public InstrucaoTipo(LocalToken local, string identificador, Instrucao[] instrucoes)
        {
            Tipo = TipoInstrucao.DeclTipo;
            Instrucoes = instrucoes;
            Identificador = identificador;
            Local = local;
        }

        public Instrucao[] Instrucoes { get; private set; }
        public string Identificador {get; private set; }
    }

    public class InstrucaoSe : Instrucao
    {
        public InstrucaoSe(LocalToken local, Expressao condicao, Instrucao[] corpo, InstrucaoSenaoSe[] listaSenaoSe = null)
        {
            Tipo = TipoInstrucao.Se;
            Condicao = condicao ?? throw new ArgumentNullException(nameof(condicao));
            Corpo = corpo ?? throw new ArgumentNullException(nameof(corpo));
            ListaSenaoSe = listaSenaoSe;
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
            Tipo = TipoInstrucao.SenaoSe;
            Condicao = condicao ?? throw new ArgumentNullException(nameof(condicao));
            Corpo = corpo ?? throw new ArgumentNullException(nameof(corpo));
            Local = local;
        }

        public Expressao Condicao { get; private set; }
        public IReadOnlyList<Instrucao> Corpo { get; private set; }
    }

    public class InstrucaoEnquanto : Instrucao
    {
        public InstrucaoEnquanto(LocalToken local, Expressao expressao, Instrucao[] instrucoes)
        {
            Tipo = TipoInstrucao.Enquanto;
            Expressao = expressao;
            Instrucoes = instrucoes;
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
            Tipo = TipoInstrucao.Romper;
            Local = local;
        }
    }

    public class InstrucaoContinuar : Instrucao
    {
        public InstrucaoContinuar(LocalToken local)
        {
            Tipo = TipoInstrucao.Continuar;
            Local = local;
        }
    }

    public class InstrucaoRetornar : Instrucao
    {
        public Expressao Expressao { get; private set; }

        public InstrucaoRetornar(LocalToken local, Expressao expressao)
        {
            Tipo = TipoInstrucao.Retornar;
            Expressao = expressao;
            Local = local;
        }
    }

    public class InstrucaoModificacaoVetor : Instrucao
    {
        public InstrucaoModificacaoVetor(LocalToken local, string identificador, Expressao indiceExpr, Expressao expressao)
        {
            Local = local;
            Expressao = expressao;
            Identificador = identificador;
            ExpressaoIndice = indiceExpr;
        }

        public string Identificador { get; private set; }
        public Expressao ExpressaoIndice { get; private set; }
        public Expressao Expressao { get; private set; }
    }

    public class InstrucaoModificacaoPropriedade : Instrucao
    {
        public InstrucaoModificacaoPropriedade(LocalToken local, string identificador, string propriedade, Expressao expressao)
        {
            Local = local;
            Expressao = expressao;
            Identificador = identificador;
            Propriedade = propriedade;
        }

        public string Identificador { get; private set; }
        public string Propriedade { get; private set; }
        public Expressao Expressao { get; private set; }
    }
}