namespace Libra.Arvore
{
    // termo matemático
    public class NodoTermo : Nodo
    {
        public NodoTermo(Token token)
        {
            m_token = token;
        }

        private Token m_token;

        public override object Avaliar()
        {
            switch (m_token.Tipo)
                {
                    case TokenTipo.Numero:
                        return m_token.Valor;
                    case TokenTipo.Identificador:
                        return double.Parse(LibraHelper.Variaveis[m_token.Valor.ToString()].ToString());
                }
            
            return 0;
        }

    }

    public class NodoExpressao : Nodo
    {
        public NodoExpressao(NodoTermo termo)
        {
            Expressao = termo;
        }

        public NodoExpressao(NodoExpressaoBinaria exprBinaria)
        {
            Expressao = exprBinaria;
        }

        public object Expressao { get; private set; }

        public override object Avaliar()
        {
            if(Expressao.GetType() == typeof(NodoTermo))
            {
                var termo = (NodoTermo)Expressao;
                return termo.Avaliar();
            }

            else if (Expressao.GetType() == typeof(NodoExpressaoBinaria))
            {
                var expressao = (NodoExpressaoBinaria)Expressao;
                return expressao.Avaliar();
            }
        
            return 0;
        }
    }

    public class NodoExpressaoBinaria : Nodo 
    {
        private NodoExpressao m_esquerda;
        private Token m_operador;
        private NodoExpressao m_direita;

        public NodoExpressaoBinaria(NodoExpressao esquerda, Token operador, NodoExpressao direita)
        {
            m_esquerda = esquerda;
            m_operador = operador;
            m_direita = direita;
        }

        public override object Avaliar()
        {
            if(m_operador.Tipo == TokenTipo.OperadorSoma)
            {
                double resultado = double.Parse(m_esquerda.Avaliar().ToString()) + double.Parse(m_direita.Avaliar().ToString());
                return resultado;
            }

            if(m_operador.Tipo == TokenTipo.OperadorSub)
            {
                double resultado = double.Parse(m_esquerda.Avaliar().ToString()) - double.Parse(m_direita.Avaliar().ToString());
                return resultado;
            }

            if(m_operador.Tipo == TokenTipo.OperadorMult)
            {
                double resultado = double.Parse(m_esquerda.Avaliar().ToString()) * double.Parse(m_direita.Avaliar().ToString());
                return resultado;
            }

            if(m_operador.Tipo == TokenTipo.OperadorDiv)
            {
                double resultado = double.Parse(m_esquerda.Avaliar().ToString()) / double.Parse(m_direita.Avaliar().ToString());
                return resultado;
            }

            return 0;
        }
    }
}