using Libra.Arvore;
using Microsoft.CSharp.RuntimeBinder;
using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Libra;

public class Interpretador
{
    public static int NivelDebug = 0;
    private Programa _programa;
    private object _ultimoRetorno = 0;
    private int _linha = 0;

    public void Interpretar(Programa programa)
    {
        LibraBase.ProgramaAtual = _programa = programa;

        LibraBase.RegistrarFuncoesEmbutidas();

        InterpretarInstrucoes(_programa.Instrucoes);
        
        LibraBase.ProgramaAtual = null; // limpar o programa depois que terminar
    }

    private object InterpretarInstrucoes(Instrucao[] instrucoes)
    {
        for(int i = 0; i < instrucoes.Length; i++)
        {
            var instrucao = InterpretarInstrucao(instrucoes[i]);

            if(instrucao is InstrucaoRetornar) 
            {
                var retorno = (InstrucaoRetornar)instrucao;
                _ultimoRetorno = InterpretarExpressao(retorno.Expressao);
                return _ultimoRetorno;
            }
        }
        
        return 0;
    }

    private Instrucao InterpretarInstrucao(Instrucao instrucao)
    {
        switch(instrucao.TipoInstrucao)
        {
            case TokenTipo.Var:
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
            case TokenTipo.Retornar:
                return instrucao;
        }

        new Erro($"Instrução Inválida: {instrucao.ToString()}");
        return null;
    }

    private void InterpretarCondicional(Instrucao instrucao)
    {
        if(instrucao is InstrucaoSe)
        {
            var instrucaoSe = (InstrucaoSe)instrucao;
            var instrucoesSe = instrucaoSe.Instrucoes;
            if((int)InterpretarExpressao(instrucaoSe.Expressao) != 0)
                InterpretarInstrucoes(instrucoesSe);
            
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
            new Erro("Identificador inválido!").LancarErro();
        
        if(_programa.FuncaoExiste(identificador))
            new ErroFuncaoJaDefinida(identificador).LancarErro();
        
        var novaFuncao = new Funcao(identificador, funcao.Instrucoes, funcao.Parametros);

        _programa.Funcoes[identificador] = novaFuncao;
    }

    private object InterpretarChamadaFuncao(ExpressaoChamadaFuncao expressaoChamadaFuncao)
    {
        return InterpretarChamadaFuncao(new InstrucaoChamadaFuncao(expressaoChamadaFuncao));
    }

    // REFATORAR!
    private HashSet<string> funcoesEmExecucao = new HashSet<string>();
    private object InterpretarChamadaFuncao(InstrucaoChamadaFuncao instrucaoChamada)
    {
        _ultimoRetorno = null;
        
        var chamada = instrucaoChamada.Chamada;
        var argumentos = chamada.Argumentos;

        if (!_programa.FuncaoExiste(chamada.Identificador))
                new ErroFuncaoNaoDefinida(chamada.Identificador).LancarErro();
                
        if(_programa.Funcoes[chamada.Identificador] is FuncaoEmbutida)
        {
            var f = (FuncaoEmbutida)_programa.Funcoes[chamada.Identificador];
            List<object> valoresArgumentos = new List<object>();
            foreach(var arg in argumentos)
            {
                valoresArgumentos.Add(InterpretarExpressao(arg));
            }

            _ultimoRetorno = f.Executar(valoresArgumentos.ToArray());
            return _ultimoRetorno;
        }
           
        // Verifica se a função já está em execução (evita recursão infinita)
        if (funcoesEmExecucao.Contains(chamada.Identificador))
        {
            new ErroTransbordoDePilha(chamada.Identificador).LancarErro();
            return null;
        }

        // Marca a função como em execução
        funcoesEmExecucao.Add(chamada.Identificador);

        try
        {
            var funcao = _programa.Funcoes[chamada.Identificador];
            var parametros = funcao.Parametros.Count;

            if (argumentos.Count != parametros)
                new Erro($"Função {chamada.Identificador}() esperava {parametros} argumento(s) e recebeu {argumentos.Count}").LancarErro();

            _programa.PilhaEscopos.EmpilharEscopo(); // empurra o novo Escopo da função

            // Adicionando os argumentos ao Escopo
            for (int i = 0; i < chamada.Argumentos.Count; i++)
            {
                string ident = funcao.Parametros[i];
                Token valor = new Token(TokenTipo.NumeroLiteral, 0, InterpretarExpressao(chamada.Argumentos[i]));
                _programa.PilhaEscopos.DefinirVariavel(ident, new Variavel(ident, valor));
            }

            object retorno = InterpretarInstrucoes(funcao.Instrucoes);

            _programa.PilhaEscopos.DesempilharEscopo(); // Removendo o Escopo da Pilha

            return _ultimoRetorno = retorno;
        }
        finally
        {
            // Marca a função como terminada
            funcoesEmExecucao.Remove(chamada.Identificador);
        }
    }

    private object InterpretarExpressao(Expressao expressao)
    {
        if(expressao == null)
            new ErroAcessoNulo(" Expressão nula");

        switch(expressao.Tipo)
        {
            case TipoExpressao.ExpressaoLiteral:
                return ((ExpressaoLiteral)expressao).Token.Valor;
            case TipoExpressao.ExpressaoVariavel:
                return _programa.PilhaEscopos.ObterVariavel(((ExpressaoVariavel)expressao).Identificador.Valor.ToString()).Valor;
            case TipoExpressao.ExpressaoChamadaFuncao:
                InterpretarChamadaFuncao((ExpressaoChamadaFuncao)expressao);
                return _ultimoRetorno;
            case TipoExpressao.ExpressaoUnaria:
                break;
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
                    case TokenTipo.OperadorMenorIgualQue:
                        return MenorIgualQue(a, b);
                }
                return 0;
        }

        new Erro("Expressão não implementada").LancarErro();

        return 0;
    }

    // TODO: Tem que ter uma forma melhor de fazer isso...
    // Será difícil manter quando tiver muitos tipos e operadores
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
        catch (DivideByZeroException)
        {
            new ErroDivisaoPorZero().LancarErro();
            return null;
        }
    }

    private object Soma(object a, object b) => Operar(a, b, (x, y) => x + y);
    private object Sub(object a, object b) => Operar(a, b, (x, y) => x - y);
    private object Mult(object a, object b) => Operar(a, b, (x, y) => x * y);
    private object Div(object a, object b) => Operar(a, b, (x, y) => y == 0 ? throw new DivideByZeroException() : x / y);

    private int Igual(object a, object b)
    {
        if(a is int)
            return (int)a > (int)b ? 1 : 0;
        
        if(a is double)
            return (double)a > (double)b ? 1 : 0;

        if(a is string)
            return (string)a == (string)b ? 1 : 0;
        
        new Erro("Operação Inválida");
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
        
        new Erro("Operação Inválida");
        return 0;
    }

    private int MaiorQue(object a, object b)
    {
        if(a is int)
            return (int)a > (int)b ? 1 : 0;
        
        if(a is double)
            return (double)a > (double)b ? 1 : 0;
        
        new Erro("Operação Inválida");
        return 0;
    }

    private int MaiorIgualQue(object a, object b)
    {
        if(a is int)
            return (int)a >= (int)b ? 1 : 0;
        
        if(a is double)
            return (double)a >= (double)b ? 1 : 0;
        
        new Erro("Operação Inválida");
        return 0;
    }

    private int MenorQue(object a, object b)
    {
        if(a is int)
            return (int)a < (int)b ? 1 : 0;
        
        if(a is double)
            return (double)a < (double)b ? 1 : 0;
        
        new Erro("Operação Inválida");
        return 0;
    }

    private int MenorIgualQue(object a, object b)
    {
        if(a is int)
            return (int)a <= (int)b ? 1 : 0;
        
        if(a is double)
            return (double)a <= (double)b ? 1 : 0;
        
        new Erro("Operação Inválida");
        return 0;
    }

    // TODO: Refatorar esse método (Desculpa, já era tarde da noite quando eu escrevi isso e deveria estar dormindo)
    private object InterpretarInstrucaoVar(InstrucaoVar i)
    {
        if(string.IsNullOrWhiteSpace(i.Identificador))
            new Erro("Identificador inválido!").LancarErro();

        object resultado = InterpretarExpressao(i.Expressao);
        var token = new Token(i.Tipo, _linha, resultado);
        var variavel = new Variavel(i.Identificador, token, i.Constante);

        if(i.EhDeclaracao)
            _programa.PilhaEscopos.DefinirVariavel(i.Identificador, variavel);
        else
            _programa.PilhaEscopos.AtualizarVariavel(i.Identificador, resultado);

        return variavel.Valor;
    }

    private object AtribuirElementoVetor(string identificador, Expressao expressaoIndice, Expressao expressao)
    {
        int indiceVetor = (int)InterpretarExpressao(expressaoIndice);
        object valor = InterpretarExpressao(expressao);

        var vetor = (Token[])_programa.PilhaEscopos.ObterVariavel(identificador).Valor;

        if(indiceVetor > vetor.Length || indiceVetor < 0)
            new ErroIndiceForaVetor().LancarErro();

        vetor[indiceVetor].Valor = valor; // TODO: Isso funciona?

        return valor;
    }

}