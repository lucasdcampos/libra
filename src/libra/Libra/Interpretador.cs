using Libra.Arvore;
using Microsoft.CSharp.RuntimeBinder;
using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Libra;

public class Interpretador
{
    public static int NivelDebug = 0;
    private Programa _programa => Ambiente.ProgramaAtual;
    private int _linha = 0;

    public int Interpretar(string codigo, ILogger logger = null)
    {
        Ambiente.ConfigurarAmbiente(logger);
        try
        {
            var tokenizador = new Tokenizador();
            var tokens = tokenizador.Tokenizar(codigo);
            var parser = new Parser();
            var programa = parser.Parse(tokens);
            return Interpretar(programa);
        }
        catch
        {
            return 1;
        }
    }

    public int Interpretar(Programa programa)
    {
        try
        {
            Ambiente.SetarPrograma(programa);
            InterpretarInstrucoes(programa.Instrucoes);
            return Ambiente.ProgramaAtual.CodigoSaida;
        }
        catch
        {
            return 1;
        }
    }

    private void InterpretarInstrucoes(Instrucao[] instrucoes)
    {
        for(int i = 0; i < instrucoes.Length; i++)
        {
            InterpretarInstrucao(instrucoes[i]);
        }
    }

    private void InterpretarInstrucao(Instrucao instrucao)
    {
        switch(instrucao.TipoInstrucao)
        {
            case TokenTipo.Var:
                InterpretarInstrucaoVar((InstrucaoVar)instrucao);
                break;
            case TokenTipo.Const:
                InterpretarInstrucaoVar((InstrucaoVar)instrucao);
                break;
            case TokenTipo.Se:
                InterpretarCondicional((InstrucaoSe)instrucao);
                break;
            case TokenTipo.Enquanto:
                InterpretarCondicional((InstrucaoEnquanto)instrucao);
                break;
            case TokenTipo.Funcao:
                InterpretarFuncao((InstrucaoFuncao)instrucao);
                break;
            case TokenTipo.Identificador:
                InterpretarChamadaFuncao((InstrucaoChamadaFuncao)instrucao);
                break;
            case TokenTipo.Vetor:
                InterpretarModificacaoVetor((InstrucaoModificacaoVetor)instrucao);
                break;
            case TokenTipo.Retornar:
                InterpretarRetorno((InstrucaoRetornar)instrucao);
                break;
        }

        if(instrucao is InstrucaoExibirExpressao)
        {
            var instrucaoExpr = (InstrucaoExibirExpressao)instrucao;
            Ambiente.Msg(InterpretarExpressao(instrucaoExpr.Expressao).ToString());
        }
    }

    private void InterpretarModificacaoVetor(InstrucaoModificacaoVetor instrucao)
    {
        string identificador = instrucao.Identificador;
        int indice = (int)InterpretarExpressao(instrucao.ExpressaoIndice);
        object expressao = InterpretarExpressao(instrucao.Expressao);

        _programa.PilhaEscopos.ModificarVetor(identificador, indice, expressao);
    }

    private void InterpretarRetorno(InstrucaoRetornar instrucao)
    {
        object resultadoExpressao = InterpretarExpressao(((InstrucaoRetornar)instrucao).Expressao);
        throw new ExcecaoRetorno(resultadoExpressao);
    }

    private void InterpretarCondicional(Instrucao instrucao)
    {
        if(instrucao is InstrucaoSe)
        {
            var instrucaoSe = (InstrucaoSe)instrucao;
            var instrucoesSe = instrucaoSe.Instrucoes;
            if ((int)InterpretarExpressao(instrucaoSe.Expressao) != 0)
            {
                InterpretarInstrucoes(instrucoesSe);
                return;
            }
            
            if(instrucaoSe.SenaoInstrucoes != null)
                InterpretarInstrucoes(instrucaoSe.SenaoInstrucoes);
            
            return;
        }

        var enquanto = (InstrucaoEnquanto)instrucao;
    	var instrucoes = enquanto.Instrucoes;
        while((int)InterpretarExpressao(enquanto.Expressao) != 0)
            InterpretarInstrucoes(instrucoes);
    }

    private void InterpretarFuncao(InstrucaoFuncao funcao)
    {
        string identificador = funcao.Identificador;

        if(string.IsNullOrWhiteSpace(identificador))
            throw new Erro("Identificador inválido!", _linha);
        
        if(_programa.FuncaoExiste(identificador))
            throw new ErroFuncaoJaDefinida(identificador, _linha);
        
        var novaFuncao = new Funcao(identificador, funcao.Instrucoes, funcao.Parametros);

        _programa.Funcoes[identificador] = novaFuncao;
    }

    private object InterpretarChamadaFuncao(ExpressaoChamadaFuncao expressaoChamadaFuncao)
    {
        return InterpretarChamadaFuncao(new InstrucaoChamadaFuncao(expressaoChamadaFuncao));
    }

    private object ExecutarFuncaoEmbutida(FuncaoEmbutida funcao, ExpressaoChamadaFuncao chamada) 
    {
        var f = (FuncaoEmbutida)_programa.Funcoes[chamada.Identificador];
        List<object> valoresArgumentos = new List<object>();
        for(int i =0; i < chamada.Argumentos.Count; i++)
        {
            valoresArgumentos.Add(InterpretarExpressao(chamada.Argumentos[i]));
        }

        return f.Executar(valoresArgumentos.ToArray());
    }

    private object InterpretarChamadaFuncao(InstrucaoChamadaFuncao instrucaoChamada)
    {
        var chamada = instrucaoChamada.Chamada;
        var argumentos = chamada.Argumentos;

        if (!_programa.FuncaoExiste(chamada.Identificador))
            throw new ErroFuncaoNaoDefinida(chamada.Identificador, _linha);

        var funcao = _programa.Funcoes[chamada.Identificador];

        if(_programa.Funcoes[chamada.Identificador] is FuncaoEmbutida)
            return ExecutarFuncaoEmbutida((FuncaoEmbutida)funcao, chamada);

        var qtdParametros = funcao.Parametros.Count;

        if (argumentos.Count != qtdParametros)
            throw new ErroEsperadoNArgumentos(funcao.Identificador, qtdParametros, argumentos.Count, _linha);

        _programa.PilhaEscopos.EmpilharEscopo(); // empurra o novo Escopo da função

        try 
        {
            // Adicionando os argumentos ao Escopo
            for (int i = 0; i < chamada.Argumentos.Count; i++)
            {
                string ident = funcao.Parametros[i];
                _programa.PilhaEscopos.DefinirVariavel(ident, InterpretarExpressao(chamada.Argumentos[i]));
            }

            InterpretarInstrucoes(funcao.Instrucoes);
        }
        catch(ExcecaoRetorno retorno)
        {
            return retorno.Valor;
        }
        finally
        {
            _programa.PilhaEscopos.DesempilharEscopo(); // Removendo o Escopo da Pilha
        }

        // Caso a função não tenha um retorno explicito
        return null;
    }

    private object InterpretarAcessoVetor(ExpressaoAcessoVetor expressao)
    {
        string ident = expressao.Identificador;
        var expressaoIndice = InterpretarExpressao(expressao.Expressao);

        if(expressaoIndice is not int indice)
            throw new ErroEsperado(TokenTipo.NumeroLiteral, TokenTipo.TokenInvalido);

        var variavel = _programa.PilhaEscopos.ObterVariavel(ident);

        if (variavel.Valor is not object[] vetor)
            throw new ErroAcessoNulo();

        if (indice < 0 || indice >= vetor.Length)
            throw new ErroIndiceForaVetor();

        return vetor[indice];
    }

    private object[] InterpretarVetor(ExpressaoDeclaracaoVetor expressao)
    {
        var expressaoIndice = InterpretarExpressao(expressao.Expressao);
        
        if(expressaoIndice is not int tamanho)
            throw new Erro("Declaração de Vetor deve conter um Número Inteiro");

        return new object[tamanho];
    }

    private object[] InterpretarInicializacaoVetor(ExpressaoInicializacaoVetor expressao)
    {
        int tamanho = expressao.Expressoes.Count;
        var vetor = new object[tamanho];

        for(int i = 0; i < tamanho; i++)
        {
            vetor[i] = InterpretarExpressao(expressao.Expressoes[i]);
        }

        return vetor;
    }

    private object InterpretarExpressao(Expressao expressao)
    {
        switch(expressao.Tipo)
        {
            case TipoExpressao.ExpressaoLiteral:
                return ((ExpressaoLiteral)expressao).Token.Valor;
            case TipoExpressao.ExpressaoVariavel:
            {
                var exprVariavel = (ExpressaoVariavel)expressao;
                var variavel = _programa.PilhaEscopos.ObterVariavel(exprVariavel.Identificador.Valor.ToString());
                return variavel.Valor;
            }
            case TipoExpressao.ExpressaoChamadaFuncao:
                return InterpretarChamadaFuncao((ExpressaoChamadaFuncao)expressao);
            case TipoExpressao.ExpressaoAcessoVetor:
                return InterpretarAcessoVetor((ExpressaoAcessoVetor)expressao);
            case TipoExpressao.ExpressaoUnaria:
                var unaria = (ExpressaoUnaria)expressao;
                switch(unaria.Operador.Tipo)
                {
                    case TokenTipo.OperadorNeg:
                        return Negar(unaria.Operando);
                }
                break;
            case TipoExpressao.ExpressaoDeclaracaoVetor:
                return InterpretarVetor((ExpressaoDeclaracaoVetor)expressao);
            case TipoExpressao.ExpressaoInicializacaoVetor:
                return InterpretarInicializacaoVetor((ExpressaoInicializacaoVetor)expressao);
            case TipoExpressao.ExpressaoBinaria:
                var bin = (ExpressaoBinaria)expressao;
                var a = InterpretarExpressao(bin.Esquerda);
                var b = InterpretarExpressao(bin.Direita);

                switch(bin.Operador.Tipo)
                {
                    case TokenTipo.OperadorSoma:
                        return Soma(a, b);
                    case TokenTipo.OperadorSub:
                        return Sub(a, b);
                    case TokenTipo.OperadorMult:
                        return Mult(a, b);
                    case TokenTipo.OperadorDiv:
                        return Div(a, b);
                    case TokenTipo.OperadorPot:
                        return Pot(a,b);
                    case TokenTipo.OperadorComparacao:
                        return Igual(a, b);
                    case TokenTipo.OperadorDiferente:
                        return Diferente(a, b);
                    case TokenTipo.OperadorMaiorQue:
                        return MaiorQue(a, b);
                    case TokenTipo.OperadorMaiorIgualQue:
                        return MaiorIgualQue(a, b);
                    case TokenTipo.OperadorMenorQue:
                        return MenorQue(a, b);
                    case TokenTipo.OperadorResto:
                        return Resto(a, b);
                    case TokenTipo.OperadorMenorIgualQue:
                        return MenorIgualQue(a, b);
                    case TokenTipo.OperadorE:
                        return E(a,b);
                    case TokenTipo.OperadorOu:
                        return Ou(a,b);
                }
                break;
        }

        throw new Erro("Expressão não implementada", _linha);

        return null;
    }

    private int Negar(Expressao expressao)
    {
        int valor = (int)InterpretarExpressao(expressao);
        
        return valor == 0 ? 1 : 0;
    }

    private object Operar(dynamic a, dynamic b, Func<dynamic, dynamic, dynamic> operacao)
    {
        try
        {
            return operacao(a, b);
        }
        catch (RuntimeBinderException)
        {
            return null;
        }

        return null;
    }

    private object Soma(object a, object b) => Operar(a, b, (x, y) => x + y);
    private object Sub(object a, object b) => Operar(a, b, (x, y) => x - y);
    private object Mult(object a, object b) => Operar(a, b, (x, y) => x * y);
    private object Div(object a, object b) => Operar(a, b, (x, y) => y == 0 ? throw new ErroDivisaoPorZero(_linha) : x / y);
    private object Pot(object a, object b)
    {
        if(a is int && b is int)
            return Math.Pow((int)a,(int)b);
        if(a is double && b is double)
            return Math.Pow((double)a,(double)b);
        
        throw new Erro($"Não é possível calcular {a.ToString()}^{b.ToString()}");
    }

    private object Resto(object a, object b)
    {
        if(a is int && b is int)
            return (int)a % (int)b;
        
        throw new Erro($"Não é possível calcular {a.ToString()}^{b.ToString()}");
    }

    private int E(object a, object b)
    {
        if(a is not int || b is not int)
            throw new Erro("Esperado valor inteiro");

        return ((int)a != 0 && (int)b != 0) ? 1 : 0;

        return 0;
    }

    private int Ou(object a, object b)
    {
        if(a is not int || b is not int)
            throw new Erro("Esperado valor inteiro");

        return ((int)a != 0 || (int)b != 0) ? 1 : 0;

        return 0;
    }

    private int Igual(object a, object b)
    {
        if(a is int)
            return (int)a == (int)b ? 1 : 0;
        
        if(a is double)
            return (double)a == (double)b ? 1 : 0;

        if(a is string)
            return (string)a == (string)b ? 1 : 0;
        
        throw new Erro("Operação Inválida", _linha);
        return 0;
    }

    private int Diferente(object a, object b)
    {
        if(a is int)
            return (int)a != (int)b ? 1 : 0;
        
        if(a is double)
            return (double)a != (double)b ? 1 : 0;

        if(a is string)
            return (string)a != (string)b ? 1 : 0;
        
        throw new Erro("Operação Inválida", _linha);
        return 0;
    }

    private int MaiorQue(object a, object b)
    {
        if(a is int)
            return (int)a > (int)b ? 1 : 0;
        
        if(a is double)
            return (double)a > (double)b ? 1 : 0;
        
        throw new Erro("Operação Inválida", _linha);
        return 0;
    }

    private int MaiorIgualQue(object a, object b)
    {
        if(a is int)
            return (int)a >= (int)b ? 1 : 0;
        
        if(a is double)
            return (double)a >= (double)b ? 1 : 0;
        
        throw new Erro("Operação Inválida", _linha);
        return 0;
    }

    private int MenorQue(object a, object b)
    {
        if(a is int)
            return (int)a < (int)b ? 1 : 0;
        
        if(a is double)
            return (double)a < (double)b ? 1 : 0;
        
        throw new Erro("Operação Inválida", _linha);
        return 0;
    }

    private int MenorIgualQue(object a, object b)
    {
        if(a is int)
            return (int)a <= (int)b ? 1 : 0;
        
        if(a is double)
            return (double)a <= (double)b ? 1 : 0;
        
        throw new Erro("Operação Inválida", _linha);
        return 0;
    }

    private object InterpretarInstrucaoVar(InstrucaoVar i)
    {
        if(string.IsNullOrWhiteSpace(i.Identificador))
            throw new Erro("Identificador inválido!", _linha);

        object resultado = InterpretarExpressao(i.Expressao);
  
        if(i.EhDeclaracao)
            _programa.PilhaEscopos.DefinirVariavel(i.Identificador, resultado, i.Constante);
        else
            _programa.PilhaEscopos.AtualizarVariavel(i.Identificador, resultado);

        return resultado;
    }
}