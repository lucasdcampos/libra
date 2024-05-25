namespace Libra.Arvore
{
    public abstract class NodoExpressao : Nodo { } // TODO: Por que não é uma interface? Não faço ideia..

    public class NodoExpressaoTermo : NodoExpressao
    {
        public NodoExpressaoTermo(Token token)
        {
            m_token = token;
        }

        private Token m_token;

        public override object Avaliar()
        {
            switch (m_token.Tipo)
                {
                    case TokenTipo.NumeroLiteral:
                        return m_token.Valor;
                    case TokenTipo.Identificador:
                        return double.Parse(LibraHelper.Variaveis[m_token.Valor.ToString()].ToString());
                }
            
            return 0;
        }

    }

    public class NodoExpressaoBinaria : NodoExpressao
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
            var esquerda = double.Parse(m_esquerda.Avaliar().ToString());
            var direita = double.Parse(m_direita.Avaliar().ToString());

            switch(m_operador.Tipo)
            {
                case TokenTipo.OperadorSoma:
                    return esquerda + direita;
                case TokenTipo.OperadorSub:
                    return esquerda - direita;
                case TokenTipo.OperadorMult:
                    return esquerda * direita;
                case TokenTipo.OperadorDiv:
                    if(direita == 0)
                        Erro.ErroGenerico("Divisão por zero"); // TODO: Isso deveria ser checado aqui?
                    return esquerda / direita;
            }
            
            return 0;
        }
    }

    public class NodoString : Nodo
    {
        private string m_texto;

        public NodoString(NodoExpressao expressao)
        {
            m_texto = expressao.Avaliar().ToString();
        }

        public NodoString(Token str)
        {
            if(str.Tipo != TokenTipo.StringLiteral)
                Erro.ErroGenerico($"Não é possível converter {str.Tipo} para String!");

            m_texto = str.Valor.ToString();
        }

        public override object Avaliar()
        {
            return m_texto;
        }
    }
}