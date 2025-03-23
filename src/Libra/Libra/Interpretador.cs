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

        if(instrucao is InstrucaoModificacaoPropriedade ip)
        {
            InterpretarModificacaoPropriedade(ip);
        }
    }

    public void InterpretarModificacaoPropriedade(InstrucaoModificacaoPropriedade instrucao)
    {
        var obj = _programa.ObterVariavel(instrucao.Identificador).Valor;

        if(obj is not LibraClasse classe)
            throw new ErroAcessoNulo(instrucao.Identificador, _local);
        
        classe.ModificarVariavel(instrucao.Propriedade, InterpretarExpressao(instrucao.Expressao));
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

    public void InterpretarFuncao(DefinicaoFuncao funcao)
    {
        string identificador = funcao.Identificador;

        if(string.IsNullOrWhiteSpace(identificador))
            throw new Erro("Identificador inválido!", _local);
        
        if(_programa.FuncaoExiste(identificador))
            throw new ErroFuncaoJaDefinida(identificador, _local);
        
        var novaFuncao = new Funcao(identificador, funcao.Instrucoes, funcao.Parametros, funcao.TipoRetorno);

        _programa.Funcoes[identificador] = novaFuncao;
    }

    public LibraObjeto ExecutarFuncaoEmbutida(FuncaoNativa funcao, ExpressaoChamadaFuncao chamada) 
    {
        var f = (FuncaoNativa)_programa.Funcoes[chamada.Identificador];
        List<object> valoresArgumentos = new List<object>();

        for(int i = 0; i < chamada.Argumentos.Count; i++)
        {
            valoresArgumentos.Add(InterpretarExpressao(chamada.Argumentos[i]).ObterValor());
        }

        var resultadoFuncao = f.Executar(valoresArgumentos.ToArray());
        var objeto = LibraObjeto.ParaLibraObjeto(resultadoFuncao);

        return objeto;
    }

    public LibraObjeto InterpretarConstrutorClasse(string nome, Expressao[] expressoes)
    {
        if(!_programa.ClasseExiste(nome))
            throw new ErroFuncaoNaoDefinida(nome, _local);

        var tipo = _programa.Classes[nome];
        
        if(tipo.Variaveis.Length != expressoes.Length && expressoes.Length > 0)
            throw new ErroEsperadoNArgumentos(nome, tipo.Variaveis.Length, expressoes.Length, _local);

        List<Variavel> vars = new();
        foreach(var i in tipo.Variaveis)
        {
            vars.Add(new Variavel(i.Identificador, InterpretarExpressao(i.Expressao), i.Constante, i.TipoVar, i.TipoModificavel));
        }
        List<Funcao> funcs = new();
        foreach(var i in tipo.Funcoes)
        {
            funcs.Add(new Funcao(i.Identificador, i.Instrucoes, i.Parametros, i.TipoRetorno));
        }

        return new LibraClasse(nome, vars.ToArray(), funcs.ToArray());
    }

    public LibraObjeto InterpretarChamadaFuncao(ExpressaoChamadaFuncao chamada)
    {
        var argumentos = chamada.Argumentos;

        // Verificando se estamos chamando uma nova instancia de uma classe
        if(_programa.ClasseExiste(chamada.Identificador))
        {
            return InterpretarConstrutorClasse(chamada.Identificador, chamada.Argumentos.ToArray());
        }

        if (!_programa.FuncaoExiste(chamada.Identificador))
        {
            throw new ErroFuncaoNaoDefinida(chamada.Identificador, _local);
        }

        var funcao = _programa.Funcoes[chamada.Identificador];

        if(_programa.Funcoes[chamada.Identificador] is FuncaoNativa nativa)
        {
            return ExecutarFuncaoEmbutida(nativa, chamada);
        }
            
        var qtdParametros = funcao.Parametros.Length;

        if (argumentos.Count != qtdParametros)
            throw new ErroEsperadoNArgumentos(funcao.Identificador, qtdParametros, argumentos.Count, _local);

        _programa.PilhaEscopos.EmpilharEscopo(funcao.Identificador, _local); // empurra o novo Escopo da função

        try 
        {
            // Adicionando os argumentos ao Escopo
            for (int i = 0; i < chamada.Argumentos.Count; i++)
            {
                string ident = funcao.Parametros[i].Identificador;
                var obj = InterpretarExpressao(chamada.Argumentos[i]);
                
                if(funcao.Parametros[i].Tipo != LibraTipo.Objeto && funcao.Parametros[i].Tipo != obj.Tipo)
                    obj = obj.Converter(funcao.Parametros[i].Tipo);

                _programa.PilhaEscopos.DefinirVariavel(ident, obj);
            }

            InterpretarInstrucoes(funcao.Instrucoes);
        }
        catch(ExcecaoRetorno retorno)
        {
            var resultado = LibraObjeto.ParaLibraObjeto(retorno.Valor);
            if(funcao.TipoRetorno != resultado.Tipo && funcao.TipoRetorno != LibraTipo.Objeto)
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

    public void InterpretarInstrucaoClasse(DefinicaoTipo i)
    {
        if(_programa.ClasseExiste(i.Identificador))
            throw new Erro($"Classe já existe: {i.Identificador}", _local);

       _programa.Classes.Add(i.Identificador, new Classe(i.Identificador, i.Variaveis, i.Funcoes));
    }

    public LibraObjeto InterpretarAtribVar(AtribuicaoVar i)
    {
        if(string.IsNullOrWhiteSpace(i.Identificador))
            throw new Erro("Identificador inválido!", _local);

        LibraObjeto resultado = InterpretarExpressao(i.Expressao);

        _programa.PilhaEscopos.AtualizarVariavel(i.Identificador, resultado);

        return resultado;
    }

    public LibraObjeto InterpretarDeclVar(DeclaracaoVar i)
    {
        if(string.IsNullOrWhiteSpace(i.Identificador))
            throw new Erro("Identificador inválido!", _local);

        LibraObjeto resultado = InterpretarExpressao(i.Expressao);

        _programa.PilhaEscopos.DefinirVariavel(i.Identificador, resultado, i.Constante, i.TipoVar, i.TipoModificavel);

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

        return LibraObjeto.ParaLibraObjeto(expressao.Aceitar(_visitorExpressoes));
    }

    public T InterpretarExpressao<T>(Expressao expressao)
    {
        var resultado = InterpretarExpressao(expressao);

        if (resultado is T t) return t;

        throw new ErroAcessoNulo($" Expressão retornou {resultado.GetType()} ao invés do esperado", _local);
    }

}