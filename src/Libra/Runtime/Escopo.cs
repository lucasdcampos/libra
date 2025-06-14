namespace Libra;

public class Escopo
{
    internal Dictionary<string, Variavel> _variaveis = new Dictionary<string, Variavel>();
    public string Nome { get; private set; }
    public LocalFonte Local { get; private set; }

    public Escopo(string nome, LocalFonte local)
    {
        Nome = nome;
        Local = local;
    }

    public void DefinirVariavel(string identificador, LibraObjeto valor, string tipo, bool constante = false)
    {
        if (VariavelExiste(identificador))
            throw new ErroVariavelJaDeclarada(identificador);

        if(valor == null)
        {
            valor = LibraObjeto.Inicializar(tipo);
        }

        if (tipo == TiposPadrao.Indefinido)
        {
            tipo = valor.Nome;    
        }
        
        if (tipo != TiposPadrao.Objeto && valor.Nome != tipo)
        {
            valor = valor.Converter(tipo);
        }
        // TODO: Se for uma classe, o Tipo tem que ser o Nome da classe!
        _variaveis[identificador] = new Variavel(identificador, valor, tipo, constante);
    }

    public Variavel? ObterVariavel(string identificador)
    {
        var var = _variaveis.TryGetValue(identificador, out var variavel) ? variavel : null;
        var.Referenciada = true;
        return var;
    }

    public int? ObterIndiceVariavel(string identificador)
    {
        if(!VariavelExiste(identificador)) return -1;

        return _variaveis.Keys.ToList().IndexOf(identificador);
    }

    public bool VariavelExiste(string identificador)
    {
        return _variaveis.ContainsKey(identificador);
    }

    public void AtualizarVariavel(string identificador, LibraObjeto novoValor)
    {
        if (_variaveis.TryGetValue(identificador, out var variavel))
        {
            variavel.AtualizarValor(novoValor);
        }
        else
        {
            throw new ErroVariavelNaoDeclarada(identificador);
        }
    }

    public void ModificarVetor(string identificador, int indice, LibraObjeto novoValor)
    {
        if (_variaveis.TryGetValue(identificador, out var variavel))
        {
            LibraVetor vetor = (LibraVetor)variavel.Valor;
            vetor.Valor[indice] = novoValor;
            variavel.AtualizarValor(vetor);
        }
        else
        {
            throw new ErroVariavelNaoDeclarada(identificador);
        }
    }
}