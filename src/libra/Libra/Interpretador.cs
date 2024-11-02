using Libra.Arvore;
using System;
using System.Reflection;

namespace Libra;

public class Interpretador
{
    public static int NivelDebug = 0;
    private Programa _programa;
    private int _enderecoInicialEscopo;
    private List<int> _enderecosIniciaisEscopos = new List<int>();

    public void Interpretar(Programa programa)
    {
        _programa = programa;
        
        LibraBase.ProgramaAtual = _programa;

        for(int i = 0; i < _programa.Instrucoes.Count; i++)
        {
            InterpretarInstrucao(_programa.Instrucoes[i]);
        }

        LibraBase.ProgramaAtual = null;
    }

    private int InterpretarInstrucao(Instrucao instrucao)
    {
        if (instrucao is InstrucaoSair)
        {
            var sair = (InstrucaoSair)instrucao;
            int codigoSaida = InterpretarExpressao(sair.Expressao);

            LibraBase.Sair(codigoSaida);
        }

        else if (instrucao is InstrucaoVar)
        {
            var var = (InstrucaoVar)instrucao;
            DefinirVariavel(var.Identificador, var.EhDeclaracao, false, var.Expressao);
        }
        else if (instrucao is InstrucaoConst)
        {
            var constante = (InstrucaoConst)instrucao;
            
            DefinirVariavel(constante.Identificador, true, true, constante.Expressao);
        }
        else if (instrucao is InstrucaoFuncao)
        {
            var funcao = (InstrucaoFuncao)instrucao;
            
            string identificador = funcao.Identificador;

            if(string.IsNullOrWhiteSpace(identificador))
            {
                Erros.LancarErro(new Erro("Identificador inválido!"));
            }

            if(_programa.FuncaoExiste(identificador))
            {
                Erros.LancarErro(new ErroFuncaoJaDefinida(identificador));
            }
            
            var novaFuncao = new Funcao(identificador, funcao.Escopo, funcao.Parametros);

            _programa.Funcoes[identificador] = novaFuncao;

        }
        else if (instrucao is InstrucaoChamadaFuncao)
        {
            var chamada = (InstrucaoChamadaFuncao)instrucao;
            
            if(chamada.Identificador.StartsWith("__") && chamada.Identificador.EndsWith("__"))
            {
                string nomeFuncao = chamada.Identificador.Replace("__", "");

                MethodInfo funcao = typeof(LibraBase).GetMethod(nomeFuncao, BindingFlags.Static | BindingFlags.Public);

                if (funcao != null)
                {
                    funcao.Invoke(null, ExtrairArgumentos(chamada));
                }
                else
                {
                    Erros.LancarErro(new ErroFuncaoNaoDefinida(nomeFuncao));
                }
            }
            else
            {
                InterpretarEscopo(_programa.Funcoes[chamada.Identificador].Escopo);
            }
        }

        else if (instrucao is InstrucaoSe)
        {
            var se = (InstrucaoSe)instrucao;

            if(InterpretarExpressao(se.Expressao) != 0)
            {
                InterpretarEscopo(se.Escopo);
            }
            else
            {
                if(se.SenaoEscopo != null)
                {
                    InterpretarEscopo(se.SenaoEscopo);
                }
            }
        }
        else if (instrucao is InstrucaoEnquanto)
        {
            var enquanto = (InstrucaoEnquanto)instrucao;

            while(InterpretarExpressao(enquanto.Expressao) != 0)
            {
                InterpretarEscopo(enquanto.Escopo);
            }
        }

        else if (instrucao is InstrucaoRomper)
        {
            return LibraHelper.ROMPER;
        }

        else if (instrucao is InstrucaoRetornar)
        {
            return LibraHelper.RETORNAR;
        }

        return 0;
    }

    private int InterpretarEscopo(Escopo escopo)
    {
        _enderecosIniciaisEscopos.Add(_programa.Variaveis.Count -1);

        for(int i = 0; i < escopo.Instrucoes.Count; i++)
        {
            InterpretarInstrucao(escopo.Instrucoes[i]);
        }

        int enderecoInicialUltimoEscopo = _enderecosIniciaisEscopos.Last();

        // limpando a memória depois da finalização do escopo
        for (int i = _programa.Variaveis.Count - 1; i > enderecoInicialUltimoEscopo; i--)
        {
            var variavelAtual = _programa.Variaveis.ElementAt(i).Key;
            _programa.Variaveis.Remove(variavelAtual);
        }

        _enderecosIniciaisEscopos.RemoveAt(_enderecosIniciaisEscopos.Count - 1);
        
        return 0;
    }

    private int InterpretarExpressao(Expressao expressao)
    {
        if(expressao is ExpressaoTermo)
        {
            var termo = (ExpressaoTermo)expressao;

            return int.Parse(ExtrairValorTermo(termo));
        }

        else if(expressao is ExpressaoBinaria)
        {
            var binaria = (ExpressaoBinaria)expressao;

            var esq = (ExpressaoTermo)binaria.Esquerda;
            var dir = binaria.Direita;

            int a, b = 0;

            a = int.Parse(ExtrairValorTermo(esq));

            b = InterpretarExpressao(dir);
            
            switch(binaria.Operador.Tipo)
            {
                case TokenTipo.OperadorSoma: return a+b;
                case TokenTipo.OperadorSub: return a-b;
                case TokenTipo.OperadorMult: return a*b;
                case TokenTipo.OperadorDiv:
                    if(b == 0)
                        Erros.LancarErro(new ErroDivisaoPorZero());
                    return a/b;
                case TokenTipo.OperadorMaiorQue: return LibraHelper.BoolParaInt(a>b);
                case TokenTipo.OperadorMaiorIgualQue: return LibraHelper.BoolParaInt(a>=b);
                case TokenTipo.OperadorMenorQue: return LibraHelper.BoolParaInt(a<b);
                case TokenTipo.OperadorMenorIgualQue: return LibraHelper.BoolParaInt(a<=b);
                case TokenTipo.OperadorOu: return LibraHelper.BoolParaInt(a!= 0 || b != 0);
                case TokenTipo.OperadorE: return LibraHelper.BoolParaInt(a!=0 && b != 0);
                case TokenTipo.OperadorComparacao: return LibraHelper.BoolParaInt(a==b);
                case TokenTipo.OperadorDiferente: return LibraHelper.BoolParaInt(a!=b);
            }
        }
        return 0;
    }

    private void DefinirVariavel(string identificador, bool declaracao, bool constante, Expressao expressao)
    {
        if(string.IsNullOrWhiteSpace(identificador))
        {
            Erros.LancarErro(new Erro("Identificador inválido!"));
        }

        if(declaracao && _programa.VariavelExiste(identificador))
        {
            Erros.LancarErro(new ErroVariavelJaDeclarada(identificador));
        }

        if(_programa.Variaveis.ContainsKey(identificador))
        {
            if(_programa.Variaveis[identificador].Constante)
                Erros.LancarErro(new ErroModificacaoConstante(identificador));
        }
        
        var token = new Token(TokenTipo.NumeroLiteral, 0, InterpretarExpressao(expressao).ToString());

        var variavel = new Variavel(identificador, token, constante);

        _programa.Variaveis[identificador] = variavel;
    }

    private string[] ExtrairArgumentos(InstrucaoChamadaFuncao chamada)
    {
        var argumentos = new List<string>();

        foreach(var argumento in chamada.Argumentos)
        {
            var valor = InterpretarExpressao(argumento).ToString();
            argumentos.Add(valor);
        }

        return argumentos.ToArray();
    }

    private string ExtrairValorTermo(ExpressaoTermo termo)
    {
        if(termo.Token.Tipo == TokenTipo.Identificador)
        {
            if(!_programa.VariavelExiste(termo.Valor)) return "0";

            return _programa.Variaveis[termo.Valor].Valor;
        }

        if(termo.Valor == null) return "0"; 
        return termo.Valor;
    }

}