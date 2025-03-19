namespace Libra;

public class LibraVetor : LibraObjeto
{
    public LibraObjeto[] Valor { get; private set; }

    public LibraVetor(LibraObjeto[] vetor)
    {
        Valor = vetor;
        Tipo = LibraTipo.Vetor;
    }

    public LibraVetor(int tamanho)
    {
        Valor = new LibraObjeto[tamanho];
    }

    public override string ToString()
    {
        return "Vetor";
    }

    public override object ObterValor()
    {
        return Valor;
    }

    public override LibraInt ObterTamanhoEmBytes()
    {
        // Não é possível calcular o tamanho diretamente
        return new LibraInt(-1);
    }
}