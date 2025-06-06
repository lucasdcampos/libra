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
        return new InterpretadorFlags(true, true, true);
    }
}

public class Interpretador
{
    public static int NivelDebug = 0;
    public static LocalToken LocalAtual => ObterLocalAtual();
    public static InterpretadorFlags Flags => _instancia == null ? InterpretadorFlags.Padrao() : _instancia._flags;
    private InterpretadorFlags _flags;
    private LocalToken _local = new LocalToken();
    private bool _shell = false;
    private VisitorExpressoes _visitorExpressoes;
    private static Interpretador _instancia;
    private static LibraObjeto _ultimoRetorno;
    public static LibraObjeto Saida => _ultimoRetorno ?? LibraObjeto.Inicializar("Nulo");

    public Interpretador(InterpretadorFlags flags = null)
    {
        _instancia = this;
        _visitorExpressoes = new VisitorExpressoes(this);
        _flags = flags == null ? InterpretadorFlags.Padrao() : flags;
    }

    private static LocalToken ObterLocalAtual()
    {
        if (_instancia == null)
            return new LocalToken();

        return _instancia._local;
    }

    public void Resetar()
    {
        _local = new LocalToken();
    }

    public int ExecutarPrograma(Programa programa, bool ambienteSeguro = true, ILogger logger = null, bool shell = false)
    {
        Resetar();
        _shell = shell;

        InterpretarInstrucoes(programa.Instrucoes);

        return 0;
    }

    public void InterpretarInstrucoes(Instrucao[] instrucoes)
    {
        for (int i = 0; i < instrucoes.Length; i++)
        {
            InterpretarInstrucao(instrucoes[i]);
        }
    }

    public void InterpretarInstrucao(Instrucao instrucao)
    {
        if (instrucao is null)
            return;

        _local = instrucao.Local;

        var acoes = new Dictionary<TipoInstrucao, Action>
        {
            { TipoInstrucao.Expressao, () => InterpretarInstrucaoExpressao((InstrucaoExpressao)instrucao) },
            { TipoInstrucao.DeclVar, () => InterpretarDeclVar((DeclaracaoVar)instrucao) },
            { TipoInstrucao.DeclFunc, () => InterpretarFuncao((DefinicaoFuncao)instrucao) },
            { TipoInstrucao.DeclClasse, () => InterpretarInstrucaoClasse((DefinicaoTipo)instrucao) },
            { TipoInstrucao.AtribVar, () => InterpretarAtribVar((AtribuicaoVar)instrucao) },
            { TipoInstrucao.AtribProp, () => InterpretarAtribProp((AtribuicaoPropriedade)instrucao) },
            { TipoInstrucao.Enquanto, () => InterpretarEnquanto((Enquanto)instrucao) },
            { TipoInstrucao.ParaCada, () => InterpretarParaCada((ParaCada)instrucao) },
            { TipoInstrucao.Se, () => InterpretarSe((Se)instrucao) },
            { TipoInstrucao.Chamada, () => InterpretarChamadaFuncao((ExpressaoChamadaFuncao)instrucao) },
            { TipoInstrucao.AtribIndice, () => InterpretarAtribIndice((AtribuicaoIndice)instrucao) },
            { TipoInstrucao.Retornar, () => InterpretarRetorno((Retornar)instrucao) },
            { TipoInstrucao.Romper, () => throw new ExcecaoRomper() },
            {TipoInstrucao.Tentar, () => InterpretarTentar((Tentar)instrucao)}
        };

        // Executa a ação associada ao tipo, se existir.
        if (acoes.TryGetValue(instrucao.Tipo, out var acao))
            acao();
    }

    public void InterpretarInstrucaoExpressao(InstrucaoExpressao instrucao)
    {
        if (instrucao.Expressao == null)
            return;

        _ultimoRetorno = InterpretarExpressao(instrucao.Expressao);
    }

    public void InterpretarTentar(Tentar instrucao)
    {
        try
        {
            Ambiente.Pilha.EmpilharEscopo();
            InterpretarInstrucoes(instrucao.InstrucoesTentar);
            Ambiente.Pilha.DesempilharEscopo();
        }
        catch (Erro err)
        {
            Ambiente.Pilha.DesempilharEscopo(); // Desempilhando escopo do "Tentar"

            Ambiente.Pilha.EmpilharEscopo();
            Ambiente.Pilha.DefinirVariavel(instrucao.VariavelErro, new LibraTexto(err.Mensagem), TiposPadrao.Texto, true);
            InterpretarInstrucoes(instrucao.InstrucoesCapturar);
            Ambiente.Pilha.DesempilharEscopo();
        }

    }

    public void InterpretarAtribProp(AtribuicaoPropriedade instrucao)
    {
        var obj = LibraObjeto.ParaLibraObjeto(InterpretarExpressao(instrucao.Alvo));

        obj.AtribuirPropriedade(instrucao.Alvo.Propriedade, InterpretarExpressao(instrucao.Expressao));
    }

    public void InterpretarAtribIndice(AtribuicaoIndice instrucao)
    {
        string identificador = instrucao.Identificador;
        int indice = InterpretarExpressao<LibraInt>(instrucao.ExpressaoIndice).Valor;
        LibraObjeto expressao = InterpretarExpressao(instrucao.Expressao);

        Ambiente.Pilha.ModificarVetor(identificador, indice, expressao);
    }

    public void InterpretarRetorno(Retornar instrucao)
    {
        object resultadoExpressao = InterpretarExpressao(((Retornar)instrucao).Expressao);
        _ultimoRetorno = LibraObjeto.ParaLibraObjeto(resultadoExpressao);
        throw new ExcecaoRetorno(resultadoExpressao);
    }

    public void InterpretarSe(Se se)
    {
        if (InterpretarExpressao<LibraInt>(se.Condicao).Valor != 0)
        {
            Ambiente.Pilha.EmpilharEscopo();
            InterpretarInstrucoes(se.Corpo.ToArray());
            Ambiente.Pilha.DesempilharEscopo();
            return;
        }

        if (se.ListaSenaoSe == null || se.ListaSenaoSe.Count == 0)
            return;

        foreach (var inst in se.ListaSenaoSe)
        {
            if (InterpretarExpressao<LibraInt>(inst.Condicao).Valor != 0)
            {
                Ambiente.Pilha.EmpilharEscopo();
                InterpretarInstrucoes(inst.Corpo.ToArray());
                Ambiente.Pilha.DesempilharEscopo();

                return;
            }
        }
    }

    public void InterpretarEnquanto(Enquanto enquanto)
    {
        // TODO: Otimizar casos em que não é necessário calcular a expressão toda vez,
        // como em "enquanto 1", por exemplo.
        while (InterpretarExpressao<LibraInt>(enquanto.Expressao).Valor != 0)
        {
            Ambiente.Pilha.EmpilharEscopo();
            foreach (var i in enquanto.Instrucoes)
            {
                try
                {
                    InterpretarInstrucao(i);
                }
                catch (ExcecaoRomper e)
                {
                    Ambiente.Pilha.DesempilharEscopo();
                    return;
                }
                // TODO: Adicionar 'continuar'
            }
            Ambiente.Pilha.DesempilharEscopo();
        }
    }

    public void InterpretarParaCada(ParaCada instrucao)
    {
        var expr = instrucao.Vetor;

        var vetor = InterpretarExpressao<LibraVetor>(expr);

        foreach (var item in vetor.Valor)
        {
            Ambiente.Pilha.EmpilharEscopo();
            try
            {
                Ambiente.DefinirGlobal(instrucao.Identificador.Valor.ToString(), item);
                InterpretarInstrucoes(instrucao.Instrucoes);
            }
            catch (ExcecaoRomper e)
            {
                Ambiente.Pilha.DesempilharEscopo();
                return;
            }
            // TODO: Adicionar 'continuar'
        }

        Ambiente.Pilha.DesempilharEscopo();
    }

    public void InterpretarFuncao(DefinicaoFuncao funcao)
    {
        string identificador = funcao.Identificador;

        if (string.IsNullOrWhiteSpace(identificador))
            throw new Erro("Identificador inválido!", _local);

        var novaFuncao = new Funcao(identificador, funcao.Instrucoes, funcao.Parametros, funcao.TipoRetorno);

        Ambiente.Pilha.DefinirVariavel(identificador, novaFuncao, TiposPadrao.Func, true);
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
        Classe tipo = (Classe)Ambiente.Pilha.ObterVariavel(nome).Valor;

        // TODO: Arrumar, nunca vi um código tão porcaria em toda a minha vida
        List<Variavel> vars = new();
        foreach(var i in tipo.Variaveis)
        {
            vars.Add(new Variavel(i.Identificador, InterpretarExpressao(i.Expressao), i.TipoVar, i.Constante));
        }
        foreach(var i in tipo.Funcoes)
        {
            vars.Add(new Variavel(
                i.Identificador,
                new Funcao(i.Identificador, i.Instrucoes, i.Parametros, i.TipoRetorno),
                TiposPadrao.Func,
                true
            ));
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

        Ambiente.Pilha.EmpilharEscopo(funcao.Identificador, _local); // empurra o novo Escopo da função

        try 
        {
            // Adicionando os argumentos ao Escopo
            for (int i = 0; i < argumentos.Length; i++)
            {
                string ident = funcao.Parametros[i].Identificador;
                var obj = InterpretarExpressao(argumentos[i]);
                
                if(funcao.Parametros[i].Tipo != TiposPadrao.Objeto && funcao.Parametros[i].Tipo != obj.Nome)
                    obj = obj.Converter(funcao.Parametros[i].Tipo);

                Ambiente.Pilha.DefinirVariavel(ident, obj, funcao.Parametros[i].Tipo);
            }

            InterpretarInstrucoes(funcao.Instrucoes);
        }
        catch(ExcecaoRetorno retorno)
        {
            var resultado = LibraObjeto.ParaLibraObjeto(retorno.Valor);
            if(funcao.TipoRetorno != resultado.Nome && funcao.TipoRetorno != TiposPadrao.Objeto)
            {
                return resultado.Converter(funcao.TipoRetorno);
            }
            
            return resultado;
        }
        finally
        {
            Ambiente.Pilha.DesempilharEscopo(); // Removendo o Escopo da Pilha
        }

        // Caso a função não tenha um retorno explicito
        return LibraObjeto.Inicializar(TiposPadrao.Nulo);
    }

    public LibraObjeto InterpretarChamadaFuncao(ExpressaoChamadaFuncao chamada)
    {
        var argumentos = chamada.Argumentos;

        var v = Ambiente.Pilha.ObterVariavel(chamada.Identificador);

        if(v.Valor is Classe)
            return InterpretarConstrutorClasse(chamada.Identificador, chamada.Argumentos.ToArray());

        return ExecutarFuncao((Funcao)v.Valor, chamada.Argumentos);
    }

    // TODO: É isso?
    public void InterpretarInstrucaoClasse(DefinicaoTipo i)
    {
       Ambiente.Pilha.DefinirVariavel(i.Identificador, new Classe(i.Identificador, i.Variaveis, i.Funcoes), i.Identificador);
    }

    public LibraObjeto InterpretarAtribVar(AtribuicaoVar i)
    {
        if(string.IsNullOrWhiteSpace(i.Identificador))
            throw new Erro("Identificador inválido!", _local);

        LibraObjeto resultado = InterpretarExpressao(i.Expressao);

        Ambiente.Pilha.AtualizarVariavel(i.Identificador, resultado);

        resultado.Construtor(i.Identificador);
        
        return resultado;
    }

    public LibraObjeto InterpretarDeclVar(DeclaracaoVar i)
    {
        if(string.IsNullOrWhiteSpace(i.Identificador))
            throw new Erro("Identificador inválido!", _local);

        LibraObjeto resultado = InterpretarExpressao(i.Expressao);

        Ambiente.Pilha.DefinirVariavel(i.Identificador, resultado, i.TipoVar, i.Constante);

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