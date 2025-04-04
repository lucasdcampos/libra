using Libra.Arvore;
using Microsoft.CSharp.RuntimeBinder;
using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Libra;

public class InterpretadorFlags
{
    public bool ModoSeguro { get; private set; }
    public bool ForcarTiposEstaticos { get; private set; }
    public bool MostrarAvisos { get; private set; }

    public InterpretadorFlags(bool seguro, bool tiposEstaticos, bool avisos)
    {
        ModoSeguro = seguro;
        ForcarTiposEstaticos = tiposEstaticos;
        MostrarAvisos = avisos;
    }

    public static InterpretadorFlags Padrao()
    {
        return new InterpretadorFlags(true, false, true);
    }
}

public class Interpretador
{
    public static int NivelDebug = 0;
    public static LocalToken LocalAtual => ObterLocalAtual();
    public static InterpretadorFlags Flags => _instancia == null ? InterpretadorFlags.Padrao() : _instancia._flags;
    private InterpretadorFlags _flags;
    private Programa _programa => Ambiente.ProgramaAtual;
    private LocalToken _local = new LocalToken();
    private bool _shell = false;
    private VisitorExpressoes _visitorExpressoes;
    private static Interpretador _instancia;

    public Interpretador(InterpretadorFlags flags = null)
    {
        _instancia = this;
        _visitorExpressoes = new VisitorExpressoes(this);
        _flags = flags == null ? InterpretadorFlags.Padrao() : flags;
    }

    private static LocalToken ObterLocalAtual()
    {
        if(_instancia == null)
            return new LocalToken();
        
        return _instancia._local;
    }

    public void Resetar()
    {
        _local = new LocalToken();
    }
    
    public int Interpretar(string codigo, bool ambienteSeguro = true, ILogger logger = null, bool shell = false, string arquivo = "")
    {
        try
        {
            var tokenizador = new Tokenizador();
            var tokens = tokenizador.Tokenizar(codigo, arquivo);
            var parser = new Parser();
            var programa = parser.Parse(tokens.ToArray());

            programa.PilhaEscopos.DefinirVariavel("__ambienteSeguro__", new LibraInt(ambienteSeguro), true);

            return ExecutarPrograma(programa, ambienteSeguro, logger, shell);
        }
        catch(Exception e)
        {
            Erro.MensagemBug(e);
            return 1;
        }
    }

    public int ExecutarPrograma(Programa programa, bool ambienteSeguro = true, ILogger logger = null, bool shell = false)
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
        if(instrucao is null)
            return;

        _local = instrucao.Local;
        
        var acoes = new Dictionary<TipoInstrucao, Action>
        {
            { TipoInstrucao.DeclVar, () => InterpretarDeclVar((DeclaracaoVar)instrucao) },
            { TipoInstrucao.DeclFunc, () => InterpretarFuncao((DefinicaoFuncao)instrucao) },
            { TipoInstrucao.DeclClasse, () => InterpretarInstrucaoClasse((DefinicaoTipo)instrucao) },
            { TipoInstrucao.AtribVar, () => InterpretarAtribVar((AtribuicaoVar)instrucao) },
            { TipoInstrucao.AtribProp, () => InterpretarAtribProp((AtribuicaoPropriedade)instrucao) },
            { TipoInstrucao.Enquanto, () => InterpretarEnquanto((InstrucaoEnquanto)instrucao) },
            { TipoInstrucao.Se, () => InterpretarSe((InstrucaoSe)instrucao) },
            { TipoInstrucao.Chamada, () => InterpretarChamadaFuncao((ExpressaoChamadaFuncao)instrucao) },
            { TipoInstrucao.AtribIndice, () => InterpretarAtribIndice((AtribuicaoIndice)instrucao) },
            { TipoInstrucao.Retornar, () => InterpretarRetorno((InstrucaoRetornar)instrucao) },
            { TipoInstrucao.Romper, () => throw new ExcecaoRomper() }
        };

        // Executa a ação associada ao tipo, se existir.
        if (acoes.TryGetValue(instrucao.Tipo, out var acao))
            acao();
    }

    public void InterpretarAtribProp(AtribuicaoPropriedade instrucao)
    {
        var obj = _programa.ObterVariavel(instrucao.Identificador).Valor;
        
        obj.AtribuirPropriedade(instrucao.Propriedade, InterpretarExpressao(instrucao.Expressao));
    }

    public void InterpretarAtribIndice(AtribuicaoIndice instrucao)
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

    public void InterpretarSe(InstrucaoSe se)
    {
        if(InterpretarExpressao<LibraInt>(se.Condicao).Valor != 0)
            {
                _programa.PilhaEscopos.EmpilharEscopo();
                InterpretarInstrucoes(se.Corpo.ToArray());
                _programa.PilhaEscopos.DesempilharEscopo();
                return;
            }

        if(se.ListaSenaoSe == null || se.ListaSenaoSe.Count == 0)
            return;

        foreach(var inst in se.ListaSenaoSe)
        {
            if(InterpretarExpressao<LibraInt>(inst.Condicao).Valor != 0)
            {
                _programa.PilhaEscopos.EmpilharEscopo();
                InterpretarInstrucoes(inst.Corpo.ToArray());
                _programa.PilhaEscopos.DesempilharEscopo();
                
                return;
            }
        }
    }

    public void InterpretarEnquanto(InstrucaoEnquanto enquanto)
    {
        // TODO: Otimizar casos em que não é necessário calcular a expressão toda vez,
        // como em "enquanto 1", por exemplo.
        while(InterpretarExpressao<LibraInt>(enquanto.Expressao).Valor != 0)
        {
            _programa.PilhaEscopos.EmpilharEscopo();
            foreach(var i in enquanto.Instrucoes)
            {
                try
                {
                    InterpretarInstrucao(i);
                }
                catch (Exception e)
                {
                    if(e is ExcecaoRomper)
                    {
                        _programa.PilhaEscopos.DesempilharEscopo();
                        return;
                    }
                }
            }
            _programa.PilhaEscopos.DesempilharEscopo();
        }
    }

    // TODO: Arrumar
    public void InterpretarFuncao(DefinicaoFuncao funcao)
    {
        string identificador = funcao.Identificador;

        if(string.IsNullOrWhiteSpace(identificador))
            throw new Erro("Identificador inválido!", _local);
        
        /*if(_programa.FuncaoExiste(identificador))
            throw new ErroFuncaoJaDefinida(identificador, _local);*/
        
        var novaFuncao = new Funcao(identificador, funcao.Instrucoes, funcao.Parametros, funcao.TipoRetorno);

        _programa.PilhaEscopos.DefinirVariavel(identificador, novaFuncao, true, "Func", false);
    }

    public LibraObjeto ExecutarFuncaoEmbutida(FuncaoNativa funcao, Expressao[] argumentos) 
    {
        List<object> valoresArgumentos = new List<object>();

        for(int i = 0; i < argumentos.Length; i++)
        {
            valoresArgumentos.Add(InterpretarExpressao(argumentos[i]).ObterValor());
        }

        var resultadoFuncao = funcao.Executar(valoresArgumentos.ToArray());
        var objeto = LibraObjeto.ParaLibraObjeto(resultadoFuncao);

        return objeto;
    }

    public LibraObjeto InterpretarConstrutorClasse(string nome, Expressao[] expressoes, string quemChamou = "")
    {
        // TODO: Pode dar erro!
        Classe tipo = (Classe)_programa.ObterVariavel(nome).Valor;

        // TODO: Arrumar, nunca vi um código tão porcaria em toda a minha vida
        List<Variavel> vars = new();
        foreach(var i in tipo.Variaveis)
        {
            vars.Add(new Variavel(i.Identificador, InterpretarExpressao(i.Expressao), i.Constante, i.TipoVar, i.TipoModificavel));
        }
        foreach(var i in tipo.Funcoes)
        {
            vars.Add(new Variavel(i.Identificador, new Funcao(i.Identificador, i.Instrucoes, i.Parametros, i.TipoRetorno)));
        }

        var obj = new LibraObjeto(nome, vars.ToArray(), expressoes);
        
        return obj;
    }

    public LibraObjeto ExecutarFuncao(Funcao funcao, Expressao[] argumentos)
    {
        if(funcao is FuncaoNativa nativa)
        {
            return ExecutarFuncaoEmbutida(nativa, argumentos);
        }
        
        var qtdParametros = funcao.Parametros.Length;

        if (argumentos.Length != qtdParametros)
            throw new ErroEsperadoNArgumentos(funcao.Identificador, qtdParametros, argumentos.Length, _local);

        _programa.PilhaEscopos.EmpilharEscopo(funcao.Identificador, _local); // empurra o novo Escopo da função

        try 
        {
            // Adicionando os argumentos ao Escopo
            for (int i = 0; i < argumentos.Length; i++)
            {
                string ident = funcao.Parametros[i].Identificador;
                var obj = InterpretarExpressao(argumentos[i]);
                
                if(funcao.Parametros[i].Tipo != "Objeto" && funcao.Parametros[i].Tipo != obj.Nome)
                    obj = obj.Converter(funcao.Parametros[i].Tipo);

                _programa.PilhaEscopos.DefinirVariavel(ident, obj);
            }

            InterpretarInstrucoes(funcao.Instrucoes);
        }
        catch(ExcecaoRetorno retorno)
        {
            var resultado = LibraObjeto.ParaLibraObjeto(retorno.Valor);
            if(funcao.TipoRetorno != resultado.Nome && funcao.TipoRetorno != "Objeto")
            {
                return resultado.Converter(funcao.TipoRetorno);
            }
            
            return resultado;
        }
        finally
        {
            _programa.PilhaEscopos.DesempilharEscopo(); // Removendo o Escopo da Pilha
        }

        // Caso a função não tenha um retorno explicito
        return null;
    }

    public LibraObjeto InterpretarChamadaFuncao(ExpressaoChamadaFuncao chamada)
    {
        var argumentos = chamada.Argumentos;

        var v = _programa.ObterVariavel(chamada.Identificador);

        if(v.Valor is Classe)
            return InterpretarConstrutorClasse(chamada.Identificador, chamada.Argumentos.ToArray());

        return ExecutarFuncao((Funcao)v.Valor, chamada.Argumentos);
    }

    // TODO: É isso?
    public void InterpretarInstrucaoClasse(DefinicaoTipo i)
    {
       _programa.PilhaEscopos.DefinirVariavel(i.Identificador, new Classe(i.Identificador, i.Variaveis, i.Funcoes));
    }

    public LibraObjeto InterpretarAtribVar(AtribuicaoVar i)
    {
        if(string.IsNullOrWhiteSpace(i.Identificador))
            throw new Erro("Identificador inválido!", _local);

        LibraObjeto resultado = InterpretarExpressao(i.Expressao);

        _programa.PilhaEscopos.AtualizarVariavel(i.Identificador, resultado);

        resultado.Construtor(i.Identificador);
        
        return resultado;
    }

    public LibraObjeto InterpretarDeclVar(DeclaracaoVar i)
    {
        if(string.IsNullOrWhiteSpace(i.Identificador))
            throw new Erro("Identificador inválido!", _local);

        LibraObjeto resultado = InterpretarExpressao(i.Expressao);

        _programa.PilhaEscopos.DefinirVariavel(i.Identificador, resultado, i.Constante, i.TipoVar, i.TipoModificavel);

        resultado.Construtor(i.Identificador);

        return resultado;
    }

    public object[] InterpretarVetor(ExpressaoNovoVetor expressao)
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
        if(expressao == null)
            return new LibraNulo();

        _local = expressao.Local;
        
        return LibraObjeto.ParaLibraObjeto(expressao.Aceitar(_visitorExpressoes));
    }

    public T InterpretarExpressao<T>(Expressao expressao)
    {
        var resultado = InterpretarExpressao(expressao);

        if (resultado is T t) return t;

        throw new ErroAcessoNulo($" Expressão retornou {resultado.GetType()} ao invés do esperado", _local);
    }

}