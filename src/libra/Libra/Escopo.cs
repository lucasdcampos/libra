namespace Libra;

public class Escopo
{
    private Dictionary<string, Variavel> _variaveis = new Dictionary<string, Variavel>();

    public void DefinirVariavel(string identificador, LibraObjeto valor, bool constante = false)
    {
        if(VariavelExiste(identificador))
            new ErroVariavelJaDeclarada(identificador).LancarErro();

        _variaveis[identificador] = new Variavel(identificador, valor, constante);
    }

    public Variavel? ObterVariavel(string identificador)
    {
        return _variaveis.TryGetValue(identificador, out var variavel) ? variavel : null;
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