using Libra.Arvore;
using System;
using System.Reflection;

namespace Libra;

public class Interpretador
{
    private NodoPrograma _programa;
    
    public void Interpretar(NodoPrograma programa)
    {
        _programa = programa;

        InterpretarInstrucoes(_programa.Escopo);
    }

    private int InterpretarInstrucoes(NodoEscopo escopo)
    {
        foreach (var instrucao in escopo.Instrucoes)
        {
            if (instrucao is NodoInstrucaoSair)
            {
                var sair = (NodoInstrucaoSair)instrucao;
                int codigoSaida = InterpretarExpressao(sair.Expressao);

                LibraBase.Sair(codigoSaida);
            }

            else if (instrucao is NodoInstrucaoVar)
            {
                var var = (NodoInstrucaoVar)instrucao;
                DefinirVariavel(var.Identificador, var.EhDeclaracao, false, var.Expressao);
            }
            else if (instrucao is NodoInstrucaoConst)
            {
                var constante = (NodoInstrucaoConst)instrucao;
                
                DefinirVariavel(constante.Identificador, true, true, constante.Expressao);
            }
            else if (instrucao is NodoInstrucaoFuncao)
            {
                var funcao = (NodoInstrucaoFuncao)instrucao;
                
                string identificador = funcao.Identificador;

                if(string.IsNullOrWhiteSpace(identificador))
                {
                    Erro.ErroGenerico("Identificador inválido!");
                }

                if(_programa.FuncaoExiste(identificador))
                {
                    Erro.ErroGenerico($"Função já declarada! {identificador}");
                }
                
                var novaFuncao = new Funcao(identificador, funcao.Escopo, funcao.Parametros);

                _programa.Funcoes[identificador] = novaFuncao;

            }
            else if (instrucao is NodoInstrucaoChamadaFuncao)
            {
                var chamada = (NodoInstrucaoChamadaFuncao)instrucao;

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
                        Erro.ErroGenerico($"Função base não encontrada {nomeFuncao}");
                    }
                }
                else
                {
                    InterpretarInstrucoes(_programa.Funcoes[chamada.Identificador].Escopo);
                }
            }

            else if (instrucao is NodoInstrucaoSe)
            {
                var se = (NodoInstrucaoSe)instrucao;

                if(InterpretarExpressao(se.Expressao) != 0)
                {
                    InterpretarInstrucoes(se.Escopo);
                }
                else
                {
                    if(se.SenaoEscopo != null)
                    {
                        InterpretarInstrucoes(se.SenaoEscopo);
                    }
                }
            }
            else if (instrucao is NodoInstrucaoEnquanto)
            {
                var enquanto = (NodoInstrucaoEnquanto)instrucao;

                while(InterpretarExpressao(enquanto.Expressao) != 0)
                {
                    if(InterpretarInstrucoes(enquanto.Escopo) == LibraHelper.ROMPER)
                        break;
                }
            }

            else if (instrucao is NodoInstrucaoRomper)
            {
                return LibraHelper.ROMPER;
            }

            else if (instrucao is NodoInstrucaoRetornar)
            {
                return LibraHelper.RETORNAR;
            }
        }

        return 0;
    }

    private int InterpretarExpressao(NodoExpressao expressao)
    {
        if(expressao is NodoExpressaoTermo)
        {
            var termo = (NodoExpressaoTermo)expressao;

            return int.Parse(ExtrairValorTermo(termo));
        }

        else if(expressao is NodoExpressaoBinaria)
        {
            var binaria = (NodoExpressaoBinaria)expressao;

            var esq = (NodoExpressaoTermo)binaria.Esquerda;
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
                        Erro.ErroDivisaoPorZero();
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

    private void DefinirVariavel(string identificador, bool declaracao, bool constante, NodoExpressao expressao)
    {
        if(string.IsNullOrWhiteSpace(identificador))
        {
            Erro.ErroGenerico("Identificador inválido!");
        }

        if(declaracao && _programa.VariavelExiste(identificador))
        {
            Erro.ErroGenerico($"Variável já declarada! {identificador}");
        }

        if(_programa.Variaveis.ContainsKey(identificador))
        {
            if(_programa.Variaveis[identificador].Constante)
                Erro.ErroGenerico($"Não é possível modificar o valor de `{identificador}` pois ela foi marcada como uma constante!");
        }
        
        var token = new Token(TokenTipo.NumeroLiteral, 0, InterpretarExpressao(expressao).ToString());

        var variavel = new Variavel(identificador, token, constante);

        _programa.Variaveis[identificador] = variavel;
    }

    private string[] ExtrairArgumentos(NodoInstrucaoChamadaFuncao chamada)
    {
        var argumentos = new List<string>();

        foreach(var argumento in chamada.Argumentos)
        {
            var valor = InterpretarExpressao(argumento).ToString();
            argumentos.Add(valor);
        }

        return argumentos.ToArray();
    }

    private string ExtrairValorTermo(NodoExpressaoTermo termo)
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