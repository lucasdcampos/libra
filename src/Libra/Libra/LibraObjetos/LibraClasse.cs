namespace Libra;

public class LibraClasse : LibraObjeto
{
    public string Nome { get; private set; }

    public Variavel[] Variaveis { get; private set; }
    public LibraClasse(string nome, Variavel[] variavels)
    {
        Nome = nome;
        Tipo = LibraTipo.Objeto;
        Variaveis = variavels;
    }

    public override string ToString()
    {
        return Nome;
    }

    public override object ObterValor()
    {
        return Nome;
    }

    public override LibraObjeto AcessarPropriedade(string prop)
    {
        foreach(var v in Variaveis)
        {
            if(v.Identificador == prop)
            {
                return v.Valor;
            }
        }

        throw new ErroVariavelNaoDeclarada(prop);
    }

    public void ModificarVariavel(string ident, LibraObjeto novoValor)
    {
        foreach(var v in Variaveis)
        {
            if(v.Identificador == ident)
            {
                v.AtualizarValor(novoValor);
                return;
            }
        }

        throw new ErroVariavelNaoDeclarada(ident);
    }
    
    public override LibraInt ObterTamanhoEmBytes()
    {
        throw new NotImplementedException();
    }
}