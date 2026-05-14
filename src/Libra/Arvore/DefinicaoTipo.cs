namespace Libra.Arvore;

public class DefinicaoTipo : Instrucao
{
    public DefinicaoTipo(LocalFonte local, string identificador, DeclaracaoVar[] variaveis, DefinicaoFuncao[] funcoes)
    {
        Variaveis = variaveis;
        Funcoes = funcoes;
        Identificador = identificador;
        Local = local;
    }

    public DeclaracaoVar[] Variaveis { get; private set; }
    public DefinicaoFuncao[] Funcoes { get; private set; }
    public string Identificador {get; private set; }

    public override T Aceitar<T>(IVisitor<T> visitor)
    {
        return visitor.VisitarClasse(this);
    }
}