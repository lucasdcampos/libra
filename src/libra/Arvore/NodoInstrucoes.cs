namespace Libra.Arvore
{
    public class NodoInstrucao : Nodo
    {
        public NodoInstrucao(NodoInstrucaoSair saida)
        {
            Instrucao = saida;
        }

        public NodoInstrucao(NodoInstrucaoVar var)
        {
            Instrucao = var;
        }

        public NodoInstrucao(NodoInstrucaoImprimir imprimir)
        {
            Instrucao = imprimir;
        }

        public object Instrucao { get; private set; }

        public override object Avaliar()
        {
            if(Instrucao.GetType() == typeof(NodoInstrucaoSair))
            {
                var sair = (NodoInstrucaoSair)Instrucao;

                return sair.Avaliar();
            }

            if(Instrucao.GetType() == typeof(NodoInstrucaoVar))
            {
                var var = (NodoInstrucaoVar)Instrucao;

                return var.Avaliar();
            }

            return 0;
        }
    }

    public class NodoInstrucaoSair : Nodo
    {
        public NodoInstrucaoSair(NodoExpressao expressao)
        {
            Expressao = expressao;
        }

        public NodoExpressao Expressao { get; private set; }

        public override object Avaliar()
        {
            return Expressao.Avaliar();
        }
    }

    public class NodoInstrucaoVar : Nodo
    {
        public NodoInstrucaoVar(Var var)
        {
            m_var = var;
        }

        private Var m_var;

        public override object Avaliar()
        {
            return m_var;
        }
    }

    public class NodoInstrucaoImprimir: Nodo
    {
        public NodoInstrucaoImprimir(NodoExpressao expressao)
        {
            m_expressao = expressao;
        }

        private NodoExpressao m_expressao;

        public override object Avaliar()
        {
            return m_expressao;
        }
    }
}