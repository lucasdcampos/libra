namespace Libra;

public class LibraTexto : LibraObjeto
{
    public string Valor { get; private set; }

    public LibraTexto(string valor) : base("Texto", new Variavel[0])
    {
        Valor = valor;
        Propriedades.Add("tamanho", new Variavel("tamanho", new FuncaoNativa(tamanho)));
    }

    public object tamanho(object[] args)
    {
        return Valor.Length;
    }

    public override object ObterValor()
    {
        return Valor;
    }

    public override LibraInt ObterTamanhoEmBytes()
    {
        return new LibraInt(Valor.Length);
    }

    public override LibraObjeto Soma(LibraObjeto outro)
    {
        if(outro.ObterValor() == null)
            return new LibraTexto(Valor + "Nulo");
        return new LibraTexto(Valor + outro.ObterValor().ToString());
    }

    public override LibraInt Igual(LibraObjeto outro)
    {
        if (outro is LibraNulo) return new LibraInt(0);
        
        return new LibraInt(Valor == outro.ObterValor().ToString());
    }
}