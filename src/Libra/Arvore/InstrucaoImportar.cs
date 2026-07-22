namespace Libra.Arvore;

public class InstrucaoImportar : Instrucao
{
    public string Caminho { get; private set; }
    public string? Identificador { get; private set; }

    /// <summary>
    /// Quando o import não usa apelido (`como`), os elementos do módulo também são
    /// injetados diretamente no escopo atual — conforme documentado em docs/bibliotecas.md.
    /// Com apelido, os elementos ficam apenas sob o namespace do módulo.
    /// </summary>
    public bool InjetarNoEscopo { get; private set; }

    public InstrucaoImportar(LocalFonte local, string caminho, string? identificador = null, bool injetarNoEscopo = false)
    {
        Local = local;
        Caminho = caminho;
        Identificador = identificador;
        InjetarNoEscopo = injetarNoEscopo;
    }

    public override T Aceitar<T>(IVisitor<T> visitor) => visitor.VisitarImportar(this);
}
