
using System;
using Libra.Runtime;

namespace Libra.Arvore
{
    public abstract class Instrucao : Nodo<object> { }

    public class InstrucaoExpressao : Instrucao
    {
        public Expressao Expressao { get; private set; }

        public InstrucaoExpressao(LocalFonte local, Expressao expressao)
        {
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
        public DeclaracaoVar(LocalFonte local, string identificador, Expressao expressao, string tipo, bool constante)
        {
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
            return visitor.VisitarDeclVar(this);
        }
    }

    public class AtribuicaoVar : Instrucao
    {
        public string Identificador { get; }
        public Expressao Expressao { get; }
        public AtribuicaoVar(LocalFonte local, string identificador, Expressao expr)
        {
            Identificador = identificador;
            Expressao = expr;
            Local = local;
        }

        public override object Aceitar(IVisitor visitor)
        {
            return visitor.VisitarAtribVar(this);
        }
    }

    public class DefinicaoFuncao : Instrucao
    {
        public DefinicaoFuncao(LocalFonte local, string identificador, Instrucao[] instrucoes, Parametro[] parametros = null, string tipoRetorno = "Objeto")
        {
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
            return visitor.VisitarFuncao(this);
        }
    }

    public class DefinicaoTipo : Instrucao
    {
        public DefinicaoTipo(LocalFonte local, string identificador, DeclaracaoVar[] variaveis, DefinicaoFuncao[] funcoes)
        {
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
            return visitor.VisitarClasse(this);
        }
    }

    public class Se : Instrucao
    {
        public Se(LocalFonte local, Expressao condicao, Instrucao[] corpo, SenaoSe[] listaSenaoSe = null)
        {
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
            return visitor.VisitarSe(this);
        }
    }

    // TODO: Isso é necessário?
    public class SenaoSe : Instrucao
    {
        public SenaoSe(LocalFonte local, Expressao condicao, Instrucao[] corpo)
        {
            Condicao = condicao ?? throw new ArgumentNullException(nameof(condicao));
            Corpo = corpo ?? throw new ArgumentNullException(nameof(corpo));
            Local = local;
        }

        public Expressao Condicao { get; private set; }
        public IReadOnlyList<Instrucao> Corpo { get; private set; }

        public override object Aceitar(IVisitor visitor)
        {
            return visitor.VisitarSenaoSe(this);
        }
    }

    public class Enquanto : Instrucao
    {
        public Enquanto(LocalFonte local, Expressao expressao, Instrucao[] instrucoes)
        {
            Expressao = expressao;
            Instrucoes = instrucoes;
            Local = local;
        }

        public Expressao Expressao { get; private set; }
        public Instrucao[] Instrucoes {get; private set; }

        public override object Aceitar(IVisitor visitor)
        {
            return visitor.VisitarEnquanto(this);
        }
    }

    public class ParaCada : Instrucao
    {
        public ParaCada(LocalFonte local, Token ident, Expressao vetor, Instrucao[] instrucoes)
        {
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
            return visitor.VisitarParaCada(this);
        }
    }

    public class Romper : Instrucao
    {
        public Romper(LocalFonte local)
        {
            Local = local;
        }

        public override object Aceitar(IVisitor visitor)
        {
            return visitor.VisitarRomper(this);
        }
    }

    public class Continuar : Instrucao
    {
        public Continuar(LocalFonte local)
        {
            Local = local;
        }

        public override object Aceitar(IVisitor visitor)
        {
            return visitor.VisitarContinuar(this);
        }
    }

    public class Retornar : Instrucao
    {
        public Expressao Expressao { get; private set; }

        public Retornar(LocalFonte local, Expressao expressao)
        {
            Expressao = expressao;
            Local = local;
        }

        public override object Aceitar(IVisitor visitor)
        {
            return visitor.VisitarRetorno(this);
        }
    }

    public class Tentar : Instrucao
    {
        public Instrucao[] InstrucoesTentar { get; private set; }
        public string VariavelErro { get; private set; }
        public Instrucao[] InstrucoesCapturar { get; private set; }

        public Tentar(LocalFonte local, Instrucao[] tentar, string variavelErro, Instrucao[] capturar)
        {
            Local = local;
            InstrucoesTentar = tentar;
            VariavelErro = variavelErro;
            InstrucoesCapturar = capturar;
        }

        public override object Aceitar(IVisitor visitor)
        {
            return visitor.VisitarTentar(this);
        }
    }

    public class AtribuicaoIndice : Instrucao
    {
        public AtribuicaoIndice(LocalFonte local, string identificador, Expressao indiceExpr, Expressao expressao)
        {
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
            return visitor.VisitarAtribIndice(this);
        }
    }

    public class AtribuicaoPropriedade : Instrucao
    {
        public AtribuicaoPropriedade(LocalFonte local, ExpressaoPropriedade alvo, Expressao expressao)
        {
            Expressao = expressao;
            Alvo = alvo;
            Local = local;
        }

        public ExpressaoPropriedade Alvo { get; private set; }
        public Expressao Expressao { get; private set; }

        public override object Aceitar(IVisitor visitor)
        {
            return visitor.VisitarAtribProp(this);
        }
    }
}