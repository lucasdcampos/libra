using Libra.Runtime;

namespace Libra.Arvore;

public class DeclaracaoVar : Instrucao
{
    public DeclaracaoVar(LocalFonte local, string identificador, Expressao expressao, string tipo, bool constante)
    {
        Identificador = identificador;
        Expressao = expressao;
        TipoVar = tipo;
        Local = local;
    }
    
    public Expressao Expressao { get; private set; }
    public string Identificador { get; private set; }
    internal string TipoVar;
    public bool Constante { get; private set; }

    public override object Aceitar(IVisitor visitor)
    {
        return visitor.VisitarDeclVar(this);
    }
}