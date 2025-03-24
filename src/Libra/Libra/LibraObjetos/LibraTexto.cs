namespace Libra;

public class LibraTexto : LibraObjeto
{
    public string Valor { get; private set; }

    public LibraTexto(string valor) : base("Texto", new Variavel[0], new Funcao[0])
    {
        Valor = valor;
        Metodos.Add("tamanho", new FuncaoNativa(tamanho));
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
        return new LibraInt(Valor == outro.ObterValor().ToString());
    }
}