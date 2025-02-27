namespace Libra;

public class LibraVetor : LibraObjeto
{
    public LibraObjeto[] Valor { get; private set; }

    public LibraVetor(LibraObjeto[] vetor)
    {
        Valor = vetor;
    }

    public LibraVetor(int tamanho)
    {
        Valor = new LibraObjeto[tamanho];
    }

    public override string ToString()
    {
        return "LibraVetor";
    }
}