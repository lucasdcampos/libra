using Libra.Runtime;

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

        public override object Aceitar(IVisitor visitor)
        {
            return visitor.VisitarAtribVar(this);
        }
    }