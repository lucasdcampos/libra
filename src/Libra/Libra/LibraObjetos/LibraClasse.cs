namespace Libra;

public class LibraClasse : LibraObjeto
{
    public string Nome { get; private set; }
    public Variavel[] Propriedades { get; private set; }
    public Funcao[] Metodos { get; private set; }
    public LibraClasse(string nome, Variavel[] propriedades, Funcao[] metodos)
    {
        Tipo = LibraTipo.Objeto;
        Nome = nome;
        Propriedades = propriedades;
        Metodos = metodos;
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
        foreach(var v in Propriedades)
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
        foreach(var v in Propriedades)
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