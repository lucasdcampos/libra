using Libra.Arvore;
using Microsoft.CSharp.RuntimeBinder;
using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Libra;

public class Interpretador
{
    public static int NivelDebug = 0;
    public static int LinhaAtual => ObterLinhaAtual();
    private Programa _programa => Ambiente.ProgramaAtual;
    private int _linha = 0;
    private bool _shell = false;

    private VisitorExpressoes _visitorExpressoes;

    private static Interpretador _instancia;

    public Interpretador()
    {
        _instancia = this;
        _visitorExpressoes = new VisitorExpressoes(this);
    }

    private static int ObterLinhaAtual()
    {
        if(_instancia == null)
            return 0;
        return _instancia._linha;
    }

    public void Resetar()
    {
        _linha = 0;
    }
    
    public int Interpretar(string codigo, bool ambienteSeguro = true, ILogger logger = null, bool shell = false)
    {
        try
        {
            var tokenizador = new Tokenizador();
            var tokens = tokenizador.Tokenizar(codigo);
            var parser = new Parser();
            var programa = parser.Parse(tokens);

            programa.PilhaEscopos.DefinirVariavel("__ambienteSeguro__", new LibraInt(ambienteSeguro), true);

            return Interpretar(programa, ambienteSeguro, logger, shell);
        }
        catch(Exception e)
        {
            Erro.MensagemBug(e);
            return 1;
        }
    }

    public int Interpretar(Programa programa, bool ambienteSeguro = true, ILogger logger = null, bool shell = false)
    {
        Resetar();
        _shell = shell;
        
        Ambiente.ConfigurarAmbiente(logger, ambienteSeguro);
        try
        {
            Ambiente.SetarPrograma(programa);
            InterpretarInstrucoes(programa.Instrucoes);
            return Ambiente.ProgramaAtual.CodigoSaida;
        }
        catch(Exception e)
        {
            Erro.MensagemBug(e);
            return 1;
        }
    }

    public void InterpretarInstrucoes(Instrucao[] instrucoes)
    {
        for(int i = 0; i < instrucoes.Length; i++)
        {
            InterpretarInstrucao(instrucoes[i]);
        }
    }

    public void InterpretarInstrucao(Instrucao instrucao)
    {
        _linha++;

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
            Ambiente.Msg(InterpretarExpressao(instrucaoExpr.Expressao).ObterValor().ToString());
        }
    }

    public void InterpretarModificacaoVetor(InstrucaoModificacaoVetor instrucao)
    {
        string identificador = instrucao.Identificador;
        int indice = InterpretarExpressao<LibraInt>(instrucao.ExpressaoIndice).Valor;
        LibraObjeto expressao = InterpretarExpressao(instrucao.Expressao);

        _programa.PilhaEscopos.ModificarVetor(identificador, indice, expressao);
    }

    public void InterpretarRetorno(InstrucaoRetornar instrucao)
    {
        object resultadoExpressao = InterpretarExpressao(((InstrucaoRetornar)instrucao).Expressao);
        throw new ExcecaoRetorno(resultadoExpressao);
    }

    public void InterpretarCondicional(Instrucao instrucao)
    {
        if(instrucao is InstrucaoSe)
        {
            var instrucaoSe = (InstrucaoSe)instrucao;
            var instrucoesSe = instrucaoSe.Instrucoes;
            if (InterpretarExpressao<LibraInt>(instrucaoSe.Expressao).Valor != 0)
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
        while(InterpretarExpressao<LibraInt>(enquanto.Expressao).Valor != 0)
            InterpretarInstrucoes(instrucoes);
    }

    public void InterpretarFuncao(InstrucaoFuncao funcao)
    {
        string identificador = funcao.Identificador;

        if(string.IsNullOrWhiteSpace(identificador))
            throw new Erro("Identificador inválido!", _linha);
        
        if(_programa.FuncaoExiste(identificador))
            throw new ErroFuncaoJaDefinida(identificador, _linha);
        
        var novaFuncao = new Funcao(identificador, funcao.Instrucoes, funcao.Parametros);

        _programa.Funcoes[identificador] = novaFuncao;
    }

    public LibraObjeto InterpretarChamadaFuncao(ExpressaoChamadaFuncao expressaoChamadaFuncao)
    {
        return InterpretarChamadaFuncao(new InstrucaoChamadaFuncao(_linha, expressaoChamadaFuncao));
    }

    public LibraObjeto ExecutarFuncaoEmbutida(FuncaoEmbutida funcao, ExpressaoChamadaFuncao chamada) 
    {
        var f = (FuncaoEmbutida)_programa.Funcoes[chamada.Identificador];
        List<object> valoresArgumentos = new List<object>();

        for(int i = 0; i < chamada.Argumentos.Count; i++)
        {
            valoresArgumentos.Add(InterpretarExpressao(chamada.Argumentos[i]).ObterValor());
        }

        var resultadoFuncao = f.Executar(valoresArgumentos.ToArray());
        var objeto = LibraObjeto.ParaLibraObjeto(resultadoFuncao);

        if(_shell)
        {
            if(objeto.ObterValor() != null)
                Ambiente.Msg(objeto.ObterValor().ToString());
        }

        return objeto;
    }

    public LibraObjeto InterpretarChamadaFuncao(InstrucaoChamadaFuncao instrucaoChamada)
    {
        var chamada = instrucaoChamada.Chamada;
        var argumentos = chamada.Argumentos;

        if (!_programa.FuncaoExiste(chamada.Identificador))
            throw new ErroFuncaoNaoDefinida(chamada.Identificador, _linha);

        var funcao = _programa.Funcoes[chamada.Identificador];

        if(_programa.Funcoes[chamada.Identificador] is FuncaoEmbutida)
        {
            return ExecutarFuncaoEmbutida((FuncaoEmbutida)funcao, chamada);
        }
            
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
            var resultado = LibraObjeto.ParaLibraObjeto(retorno.Valor);
            if(_shell)
                Ambiente.Msg(resultado.ObterValor().ToString());

            return resultado;
        }
        finally
        {
            _programa.PilhaEscopos.DesempilharEscopo(); // Removendo o Escopo da Pilha
        }

        // Caso a função não tenha um retorno explicito
        return null;
    }

    public LibraObjeto InterpretarInstrucaoVar(InstrucaoVar i)
    {
        if(string.IsNullOrWhiteSpace(i.Identificador))
            throw new Erro("Identificador inválido!", _linha);

        LibraObjeto resultado = InterpretarExpressao(i.Expressao);
  
        if(i.EhDeclaracao)
            _programa.PilhaEscopos.DefinirVariavel(i.Identificador, resultado, i.Constante);
        else
            _programa.PilhaEscopos.AtualizarVariavel(i.Identificador, resultado);

        return resultado;
    }

    public object InterpretarAcessoVetor(ExpressaoAcessoVetor expressao)
    {
        string ident = expressao.Identificador;
        int indice = InterpretarExpressao<LibraInt>(expressao.Expressao).Valor;

        var variavel = _programa.PilhaEscopos.ObterVariavel(ident);

        if (variavel.Valor is not LibraVetor vetor)
            throw new ErroAcessoNulo(variavel.Identificador, _linha);

        if (indice < 0 || indice >= vetor.Valor.Length)
            throw new ErroIndiceForaVetor(indice.ToString(), _linha);

        return vetor.Valor[indice];
    }

    public object[] InterpretarVetor(ExpressaoDeclaracaoVetor expressao)
    {
        int indice = InterpretarExpressao<LibraInt>(expressao.Expressao).Valor;
        return new LibraObjeto[indice];
    }

    public object[] InterpretarInicializacaoVetor(ExpressaoInicializacaoVetor expressao)
    {
        int tamanho = expressao.Expressoes.Count;
        var vetor = new object[tamanho];

        for(int i = 0; i < tamanho; i++)
        {
            vetor[i] = InterpretarExpressao(expressao.Expressoes[i]);
        }

        return vetor;
    }

    public LibraObjeto InterpretarExpressao(Expressao expressao)
    {
        return LibraObjeto.ParaLibraObjeto(expressao.Aceitar(_visitorExpressoes));
    }

    public T InterpretarExpressao<T>(Expressao expressao)
    {
        var resultado = InterpretarExpressao(expressao);

        if (resultado is T t) return t;

        throw new ErroAcessoNulo($" Expressão retornou {resultado.GetType()} ao invés do esperado", _linha);
    }

}