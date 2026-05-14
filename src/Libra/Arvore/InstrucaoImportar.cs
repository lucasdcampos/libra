namespace Libra.Arvore;

public class InstrucaoImportar : Instrucao
{
    public string Caminho { get; private set; }
    public string? Identificador { get; private set; }

    public InstrucaoImportar(LocalFonte local, string caminho, string? identificador = null)
    {
        Local = local;
        Caminho = caminho;
        Identificador = identificador;
    }

    public override T Aceitar<T>(IVisitor<T> visitor) => visitor.VisitarImportar(this);
}
