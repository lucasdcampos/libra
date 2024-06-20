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
                        if(!LibraHelper.Variaveis.ContainsKey(m_token.Valor.ToString()))
                            Erro.ErroGenerico($"Variável não declarada: `{m_token.Valor.ToString()}`", m_token.Linha);
                        return double.Parse(LibraHelper.Variaveis[m_token.Valor.ToString()].ToString());

                }
            
            return 0;
        }

    }

    public class NodoTermoBooleano : Nodo
    {
        private Token m_bool = null;
        public NodoTermoBooleano(Token token)
        {
            if (token.Tipo != TokenTipo.BoolLiteral)
                Erro.ErroGenerico($"{token.Tipo} não é válido para uma expressão booleana");

            m_bool = token;
        }

        public override object Avaliar()
        {
            if ((bool)m_bool.Valor == true)
            {
                return "Verdadeiro";
            }

            return "Falso";
        }
    }

    public class NodoExpressaoBooleana : Nodo
    {
        private NodoTermoBooleano termo;

        public NodoExpressaoBooleana(Token token)
        {
            termo = new NodoTermoBooleano(token);
        }

        public NodoExpressaoBooleana(NodoExpressao esq, Token opr, NodoExpressao dir)
        {
            if(opr.Tipo == TokenTipo.OperadorMaiorQue)
            {
                var val = double.Parse(esq.Avaliar().ToString()) > double.Parse(dir.Avaliar().ToString());

                termo = new NodoTermoBooleano(new Token(TokenTipo.BoolLiteral, val == true ? 1 : 0));
                
            }

            termo = new NodoTermoBooleano(new Token(TokenTipo.BoolLiteral, 0));
        }

        public override object Avaliar()
        {
            return termo.Avaliar();
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

        public NodoString(NodoExpressaoBooleana booleana)
        {
            m_texto = booleana.Avaliar().ToString();
        }

        public NodoString(NodoInstrucaoTipo tipo)
        {
            m_texto = tipo.Avaliar().ToString();
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