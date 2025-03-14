
using System;

namespace Libra.Arvore
{
    public abstract class Instrucao 
    { 
        public TokenTipo TipoInstrucao { get; protected set; }

        public int Linha {get; protected set; }
    }

    public class InstrucaoVar : Instrucao
    {
        public InstrucaoVar(int linha, string identificador, Expressao expressao, bool constante, bool declaracao)
        {
            Identificador = identificador;
            EhDeclaracao = declaracao;
            Constante = constante;
            Expressao = expressao; 
            TipoInstrucao = TokenTipo.Var;
            Linha = linha;
        }

        public Expressao Expressao { get; private set; }
        public TokenTipo Tipo { get; private set; }
        public string Identificador {get; private set; }
        internal bool EhDeclaracao;
        public bool Constante { get; private set; }
    }

    public class InstrucaoFuncao : Instrucao
    {
        public InstrucaoFuncao(int linha, string identificador, Instrucao[] instrucoes, List<string> parametros = null)
        {
            Instrucoes = instrucoes;
            Identificador = identificador;
            Parametros = parametros;
            TipoInstrucao = TokenTipo.Funcao;
            Linha = linha;
        }

        public Instrucao[] Instrucoes { get; private set; }
        public string Identificador {get; private set; }
        public List<string> Parametros { get; private set; }

    }

    public class InstrucaoSe : Instrucao
    {
        public InstrucaoSe(int linha, Expressao condicao, Instrucao[] corpo, InstrucaoSenaoSe[] listaSenaoSe = null)
        {
            Condicao = condicao ?? throw new ArgumentNullException(nameof(condicao));
            Corpo = corpo ?? throw new ArgumentNullException(nameof(corpo));
            ListaSenaoSe = listaSenaoSe;
            TipoInstrucao = TokenTipo.Se;
            Linha = linha;
        }

        public Expressao Condicao { get; private set; }
        public IReadOnlyList<Instrucao> Corpo { get; private set; }
        public IReadOnlyList<InstrucaoSenaoSe> ListaSenaoSe { get; private set; }
    }

    public class InstrucaoSenaoSe : Instrucao
    {
        public InstrucaoSenaoSe(int linha, Expressao condicao, Instrucao[] corpo)
        {
            Condicao = condicao ?? throw new ArgumentNullException(nameof(condicao));
            Corpo = corpo ?? throw new ArgumentNullException(nameof(corpo));
            TipoInstrucao = TokenTipo.SenaoSe;
            Linha = linha;
        }

        public Expressao Condicao { get; private set; }
        public IReadOnlyList<Instrucao> Corpo { get; private set; }
    }


    public class InstrucaoEnquanto : Instrucao
    {
        public InstrucaoEnquanto(int linha, Expressao expressao, Instrucao[] instrucoes)
        {
            Expressao = expressao;
            Instrucoes = instrucoes;
            TipoInstrucao = TokenTipo.Enquanto;
            Linha = linha;
        }

        public Expressao Expressao { get; private set; }
        public Instrucao[] Instrucoes {get; private set; }

    }

    public class InstrucaoRomper : Instrucao
    {
        // TODO: Implementar
    }

    public class InstrucaoRetornar : Instrucao
    {
        public Expressao Expressao { get; private set; }

        public InstrucaoRetornar(int linha, Expressao expressao)
        {
            Expressao = expressao;
            TipoInstrucao = TokenTipo.Retornar;
            Linha = linha;
        }
    }

    public class InstrucaoChamadaFuncao : Instrucao
    {
        public InstrucaoChamadaFuncao(int linha, ExpressaoChamadaFuncao chamada)
        {
            Chamada = chamada;
            TipoInstrucao = TokenTipo.Identificador;
            Linha = linha;
        }

        public ExpressaoChamadaFuncao Chamada { get; private set; }
    }

    // Usada na CLI da Libra. Ex:
    // >>> 1+1
    // Saída: 2
    public class InstrucaoExibirExpressao : Instrucao
    {
        public InstrucaoExibirExpressao(int linha, Expressao expressao)
        {
            Linha = linha;
            Expressao = expressao;
            TipoInstrucao = TokenTipo.TokenInvalido;
        }

        public Expressao Expressao { get; private set; }
    }

    public class InstrucaoModificacaoVetor : Instrucao
    {
        public InstrucaoModificacaoVetor(int linha, string identificador, Expressao indiceExpr, Expressao expressao)
        {
            Linha = linha;
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