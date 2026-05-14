namespace Libra.Arvore;

public class AtribuicaoVar : Instrucao
    {
        public string Identificador { get; }
        public Expressao Expressao { get; }
        public AtribuicaoVar(LocalFonte local, string identificador, Expressao expr)
        {
            Identificador = identificador;
            Expressao = expr;
            Local = local;
        }

    public override T Aceitar<T>(IVisitor<T> visitor)
        {
            return visitor.VisitarAtribVar(this);
        }
    }