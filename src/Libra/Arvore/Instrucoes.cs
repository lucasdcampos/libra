
using System;
using Libra.Runtime;

namespace Libra.Arvore
{
    public enum TipoInstrucao
    {
        Expressao, // InstrucaoExpressao
        DeclVar,
        DeclFunc,
        DeclClasse,
        AtribVar,
        AtribIndice,
        AtribProp,
        Chamada,
        ChamadaMetodo,
        Se,
        SenaoSe,
        Enquanto,
        ParaCada,
        Romper,
        Continuar,
        Retornar,
        Tentar
    }

    public abstract class Instrucao
    {
        public TipoInstrucao Tipo { get; protected set; }
        public LocalToken Local { get; protected set; }
        
        public abstract object Aceitar(IVisitor visitor);
    }

    public class InstrucaoExpressao : Instrucao
    {
        public Expressao Expressao { get; private set; }

        public InstrucaoExpressao(LocalToken local, Expressao expressao)
        {
            Tipo = TipoInstrucao.Expressao;
            Expressao = expressao;
            Local = local;
        }

        public override object Aceitar(IVisitor visitor)
        {
            return Expressao.Aceitar(visitor);
        }
    }

    public class DeclaracaoVar : Instrucao
    {
        public DeclaracaoVar(LocalToken local, string identificador, Expressao expressao, string tipo, bool constante)
        {
            Tipo = TipoInstrucao.DeclVar;
            Identificador = identificador;
            Expressao = expressao;
            TipoVar = tipo;
            Local = local;
        }
        
        public Expressao Expressao { get; private set; }
        public string Identificador { get; private set; }
        internal string TipoVar;
        public bool Constante { get; private set; }

        public override object Aceitar(IVisitor visitor)
        {
            throw new NotImplementedException();
        }
    }

    public class AtribuicaoVar : Instrucao
    {
        public string Identificador { get; }
        public Expressao Expressao { get; }
        public AtribuicaoVar(LocalToken local, string identificador, Expressao expr)
        {
            Identificador = identificador;
            Expressao = expr;
            Tipo = TipoInstrucao.AtribVar;
            Local = local;
        }

        public override object Aceitar(IVisitor visitor)
        {
            throw new NotImplementedException();
        }
    }

    public class DefinicaoFuncao : Instrucao
    {
        public DefinicaoFuncao(LocalToken local, string identificador, Instrucao[] instrucoes, Parametro[] parametros = null, string tipoRetorno = "Objeto")
        {
            Tipo = TipoInstrucao.DeclFunc;
            Instrucoes = instrucoes;
            Identificador = identificador;
            Parametros = parametros;
            TipoRetorno = tipoRetorno;
            Local = local;
        }

        public Instrucao[] Instrucoes { get; private set; }
        public string Identificador {get; private set; }
        public Parametro[] Parametros { get; private set; }
        public string TipoRetorno { get; private set; }

        public override object Aceitar(IVisitor visitor)
        {
            throw new NotImplementedException();
        }
    }

    public class DefinicaoTipo : Instrucao
    {
        public DefinicaoTipo(LocalToken local, string identificador, DeclaracaoVar[] variaveis, DefinicaoFuncao[] funcoes)
        {
            Tipo = TipoInstrucao.DeclClasse;
            Variaveis = variaveis;
            Funcoes = funcoes;
            Identificador = identificador;
            Local = local;
        }

        public DeclaracaoVar[] Variaveis { get; private set; }
        public DefinicaoFuncao[] Funcoes { get; private set; }
        public string Identificador {get; private set; }

        public override object Aceitar(IVisitor visitor)
        {
            throw new NotImplementedException();
        }
    }

    public class Se : Instrucao
    {
        public Se(LocalToken local, Expressao condicao, Instrucao[] corpo, SenaoSe[] listaSenaoSe = null)
        {
            Tipo = TipoInstrucao.Se;
            Condicao = condicao ?? throw new ArgumentNullException(nameof(condicao));
            Corpo = corpo ?? throw new ArgumentNullException(nameof(corpo));
            ListaSenaoSe = listaSenaoSe;
            Local = local;
        }

        public Expressao Condicao { get; private set; }
        public IReadOnlyList<Instrucao> Corpo { get; private set; }
        public IReadOnlyList<SenaoSe> ListaSenaoSe { get; private set; }

        public override object Aceitar(IVisitor visitor)
        {
            throw new NotImplementedException();
        }
    }

    public class SenaoSe : Instrucao
    {
        public SenaoSe(LocalToken local, Expressao condicao, Instrucao[] corpo)
        {
            Tipo = TipoInstrucao.SenaoSe;
            Condicao = condicao ?? throw new ArgumentNullException(nameof(condicao));
            Corpo = corpo ?? throw new ArgumentNullException(nameof(corpo));
            Local = local;
        }

        public Expressao Condicao { get; private set; }
        public IReadOnlyList<Instrucao> Corpo { get; private set; }

        public override object Aceitar(IVisitor visitor)
        {
            throw new NotImplementedException();
        }
    }

    public class Enquanto : Instrucao
    {
        public Enquanto(LocalToken local, Expressao expressao, Instrucao[] instrucoes)
        {
            Tipo = TipoInstrucao.Enquanto;
            Expressao = expressao;
            Instrucoes = instrucoes;
            Local = local;
        }

        public Expressao Expressao { get; private set; }
        public Instrucao[] Instrucoes {get; private set; }

        public override object Aceitar(IVisitor visitor)
        {
            throw new NotImplementedException();
        }
    }

    public class ParaCada : Instrucao
    {
        public ParaCada(LocalToken local, Token ident, Expressao vetor, Instrucao[] instrucoes)
        {
            Tipo = TipoInstrucao.ParaCada;
            Identificador = ident;
            Instrucoes = instrucoes;
            Local = local;
            Vetor = vetor;
        }

        public Token Identificador { get; private set; }
        public Instrucao[] Instrucoes {get; private set; }
        public Expressao Vetor { get; private set; }

        public override object Aceitar(IVisitor visitor)
        {
            throw new NotImplementedException();
        }
    }

    public class Romper : Instrucao
    {
        public Romper(LocalToken local)
        {
            Local = local;
            Tipo = TipoInstrucao.Romper;
        }

        public override object Aceitar(IVisitor visitor)
        {
            throw new NotImplementedException();
        }
    }

    public class Continuar : Instrucao
    {
        public Continuar(LocalToken local)
        {
            Local = local;
            Tipo = TipoInstrucao.Continuar;
        }

        public override object Aceitar(IVisitor visitor)
        {
            throw new NotImplementedException();
        }
    }

    public class Retornar : Instrucao
    {
        public Expressao Expressao { get; private set; }

        public Retornar(LocalToken local, Expressao expressao)
        {
            Tipo = TipoInstrucao.Retornar;
            Expressao = expressao;
            Local = local;
        }

        public override object Aceitar(IVisitor visitor)
        {
            throw new NotImplementedException();
        }
    }

    public class Tentar : Instrucao
    {
        public Instrucao[] InstrucoesTentar { get; private set; }
        public string VariavelErro { get; private set; }
        public Instrucao[] InstrucoesCapturar { get; private set; }

        public Tentar(LocalToken local, Instrucao[] tentar, string variavelErro, Instrucao[] capturar)
        {
            Local = local;
            InstrucoesTentar = tentar;
            VariavelErro = variavelErro;
            InstrucoesCapturar = capturar;

            Tipo = TipoInstrucao.Tentar;
        }

        public override object Aceitar(IVisitor visitor)
        {
            throw new NotImplementedException();
        }
    }

    public class AtribuicaoIndice : Instrucao
    {
        public AtribuicaoIndice(LocalToken local, string identificador, Expressao indiceExpr, Expressao expressao)
        {
            Tipo = TipoInstrucao.AtribIndice;
            Expressao = expressao;
            Identificador = identificador;
            ExpressaoIndice = indiceExpr;
            Local = local;
        }

        public string Identificador { get; private set; }
        public Expressao ExpressaoIndice { get; private set; }
        public Expressao Expressao { get; private set; }

        public override object Aceitar(IVisitor visitor)
        {
            throw new NotImplementedException();
        }
    }

    public class AtribuicaoPropriedade : Instrucao
    {
        public AtribuicaoPropriedade(LocalToken local, ExpressaoPropriedade alvo, Expressao expressao)
        {
            Tipo = TipoInstrucao.AtribProp;
            Expressao = expressao;
            Alvo = alvo;
            Local = local;
        }

        public ExpressaoPropriedade Alvo { get; private set; }
        public Expressao Expressao { get; private set; }

        public override object Aceitar(IVisitor visitor)
        {
            throw new NotImplementedException();
        }
    }
}