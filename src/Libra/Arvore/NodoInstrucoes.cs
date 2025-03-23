
using System;

namespace Libra.Arvore
{
    public enum TipoInstrucao
    {
        DeclVar,
        DeclFunc,
        DeclClasse,
        AtribVar,
        AtribIndice,
        AtribProp,
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

    public class DeclaracaoConst : Instrucao
    {
        public DeclaracaoConst(string identificador, Expressao expressao)
        {
            Identificador = identificador;
            Expressao = expressao;
        }
        
        public string Identificador { get; }
        public Expressao Expressao { get; }
    }
    public class DeclaracaoVar : Instrucao
    {
        public DeclaracaoVar(string identificador, Expressao expressao, LibraTipo tipo, bool tipoModificavel, bool constante)
        {
            Tipo = TipoInstrucao.DeclVar;
            Identificador = identificador;
            Expressao = expressao; 
            TipoModificavel = tipoModificavel;
            TipoVar = tipo;
        }

        public Expressao Expressao { get; private set; }
        public string Identificador {get; private set; }
        internal LibraTipo TipoVar;
        internal bool TipoModificavel;
        public bool Constante { get; private set; }
    }

    public class AtribuicaoVar : Instrucao
    {
        public string Identificador { get; }
        public Expressao Expressao { get; }
        public AtribuicaoVar(string identificador, Expressao expr)
        {
            Identificador = identificador;
            Expressao = expr;
            Tipo = TipoInstrucao.AtribVar;
        }
    }

    public class DefinicaoFuncao : Instrucao
    {
        public DefinicaoFuncao(string identificador, Instrucao[] instrucoes, Parametro[] parametros = null, LibraTipo tipoRetorno = LibraTipo.Objeto)
        {
            Tipo = TipoInstrucao.DeclFunc;
            Instrucoes = instrucoes;
            Identificador = identificador;
            Parametros = parametros;
            TipoRetorno = tipoRetorno;
        }

        public Instrucao[] Instrucoes { get; private set; }
        public string Identificador {get; private set; }
        public Parametro[] Parametros { get; private set; }
        public LibraTipo TipoRetorno { get; private set; }
    }

    public class DefinicaoTipo : Instrucao
    {
        public DefinicaoTipo(string identificador, DeclaracaoVar[] variaveis, DefinicaoFuncao[] funcoes)
        {
            Tipo = TipoInstrucao.DeclClasse;
            Variaveis = variaveis;
            Funcoes = funcoes;
            Identificador = identificador;
        }

        public DeclaracaoVar[] Variaveis { get; private set; }
        public DefinicaoFuncao[] Funcoes { get; private set; }
        public string Identificador {get; private set; }
    }

    public class InstrucaoSe : Instrucao
    {
        public InstrucaoSe(Expressao condicao, Instrucao[] corpo, InstrucaoSenaoSe[] listaSenaoSe = null)
        {
            Tipo = TipoInstrucao.Se;
            Condicao = condicao ?? throw new ArgumentNullException(nameof(condicao));
            Corpo = corpo ?? throw new ArgumentNullException(nameof(corpo));
            ListaSenaoSe = listaSenaoSe;
        }

        public Expressao Condicao { get; private set; }
        public IReadOnlyList<Instrucao> Corpo { get; private set; }
        public IReadOnlyList<InstrucaoSenaoSe> ListaSenaoSe { get; private set; }
    }

    public class InstrucaoSenaoSe : Instrucao
    {
        public InstrucaoSenaoSe(Expressao condicao, Instrucao[] corpo)
        {
            Tipo = TipoInstrucao.SenaoSe;
            Condicao = condicao ?? throw new ArgumentNullException(nameof(condicao));
            Corpo = corpo ?? throw new ArgumentNullException(nameof(corpo));
            
        }

        public Expressao Condicao { get; private set; }
        public IReadOnlyList<Instrucao> Corpo { get; private set; }
    }

    public class InstrucaoEnquanto : Instrucao
    {
        public InstrucaoEnquanto( Expressao expressao, Instrucao[] instrucoes)
        {
            Tipo = TipoInstrucao.Enquanto;
            Expressao = expressao;
            Instrucoes = instrucoes;
            
        }

        public Expressao Expressao { get; private set; }
        public Instrucao[] Instrucoes {get; private set; }

    }

    // Não necessitam de implementação
    public class InstrucaoRomper : Instrucao
    {
        public InstrucaoRomper()
        {
            Tipo = TipoInstrucao.Romper;
        }
    }

    public class InstrucaoContinuar : Instrucao
    {
        public InstrucaoContinuar()
        {
            Tipo = TipoInstrucao.Continuar;
        }
    }

    public class InstrucaoRetornar : Instrucao
    {
        public Expressao Expressao { get; private set; }

        public InstrucaoRetornar( Expressao expressao)
        {
            Tipo = TipoInstrucao.Retornar;
            Expressao = expressao;
            
        }
    }

    public class AtribuicaoIndice : Instrucao
    {
        public AtribuicaoIndice(string identificador, Expressao indiceExpr, Expressao expressao)
        {
            Tipo = TipoInstrucao.AtribIndice;
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
        public InstrucaoModificacaoPropriedade(string identificador, string propriedade, Expressao expressao)
        {
            Expressao = expressao;
            Identificador = identificador;
            Propriedade = propriedade;
        }

        public string Identificador { get; private set; }
        public string Propriedade { get; private set; }
        public Expressao Expressao { get; private set; }
    }
}