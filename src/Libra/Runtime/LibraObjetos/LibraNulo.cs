namespace Libra.Runtime;

public class LibraNulo : LibraObjeto
{
    public LibraNulo() : base("Nulo", new Variavel[0])
    {
        
    }

    public override object ObterValor()
    {
        return null;
    }

    public override LibraInt Igual(LibraObjeto outro)
    {
        return new LibraInt(outro is LibraNulo);
    }
    
    public override LibraObjeto Converter(string novoTipo)
    {
        return LibraObjeto.Inicializar(novoTipo);
    }
    
    public override LibraInt ObterTamanhoEmBytes()
    {
        return new LibraInt(0);
    }
}