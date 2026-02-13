// TODO: Necessita refatoração

using Libra.Arvore;
using Libra.Modulos;

namespace Libra.Runtime;

public class Ambiente
{
    private ILogger _logger;
    public ILogger Logger => _logger;
    public bool AmbienteSeguro => _ambienteSeguro;
    public bool _ambienteSeguro; 

    private PilhaDeEscopos _pilha;
    public PilhaDeEscopos Pilha => _pilha;
    private string _textoSaida = "";
    public string TextoSaida => _textoSaida;


    public Ambiente(ILogger logger, bool seguro)
    {
        _pilha = new PilhaDeEscopos();
        _logger = logger ?? new ConsoleLogger();

        _ambienteSeguro = seguro;

        new LibraBase().RegistrarFuncoes(this);
    }

    public void DefinirGlobal(string identificador, object valor)
    {
        var obj = LibraObjeto.ParaLibraObjeto(valor);

        Pilha.DefinirVariavel(identificador, obj, obj.Nome);
    }

    public object ObterGlobal(string identificador)
    {
        var variavel = Pilha.ObterVariavel(identificador);
        return variavel?.Valor ?? null;
    }

    public void RegistrarFuncaoNativa(string nomeFuncao, Func<object[], object> funcaoCSharp)
    {
        Pilha.DefinirVariavel(nomeFuncao, new FuncaoNativa(funcaoCSharp, nomeFuncao), TiposPadrao.Func);
    }

    public void Msg(string msg, string final = "\n")
    {
        _textoSaida += msg + final;

        var loggerReal = _logger ?? new ConsoleLogger();
        loggerReal.Msg(msg, final);
    }

    public void ExibirErro(Exception e)
    {
        if (e is ExcecaoSaida)
            return;
            
        if (e is Erro)
            Msg(e.ToString());
    }

    public void Encerrar(int codigo)
    {
        throw new ExcecaoSaida(codigo);
    }
}