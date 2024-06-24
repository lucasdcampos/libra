namespace Libra.Arvore
{
    public abstract class NodoExpressao : Nodo { } // TODO: Por que não é uma interface? Não faço ideia..

    public class NodoExpressaoTermo : NodoExpressao
    {
        public NodoExpressaoTermo(Token token)
        {
            _token = token;
        }

        private Token _token;

        public override object Avaliar()
        {
            switch (_token.Tipo)
                {
                    case TokenTipo.NumeroLiteral:
                        return _token.Valor;
                    case TokenTipo.Identificador:
                        if(!LibraHelper.Variaveis.ContainsKey(_token.Valor.ToString()))
                            Erro.ErroGenerico($"Variável não declarada: `{_token.Valor.ToString()}`", _token.Linha);
                        return double.Parse(LibraHelper.Variaveis[_token.Valor.ToString()].ToString());

                }
            
            return 0;
        }

    }

    public class NodoTermoBooleano : Nodo
    {
        private Token _bool = null;
        public NodoTermoBooleano(Token token)
        {
            if (token.Tipo != TokenTipo.BoolLiteral)
                Erro.ErroGenerico($"{token.Tipo} não é válido para uma expressão booleana");

            _bool = token;
        }

        public override object Avaliar()
        {
            if ((bool)_bool.Valor == true)
            {
                return "Verdade";
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
            else
            {
                termo = new NodoTermoBooleano(new Token(TokenTipo.BoolLiteral, 0));
            }
        }

        public override object Avaliar()
        {
            return termo.Avaliar();
        }

    }

    public class NodoExpressaoBinaria : NodoExpressao
    {
        private NodoExpressao _esquerda;
        private Token _operador;
        private NodoExpressao _direita;

        public NodoExpressaoBinaria(NodoExpressao esquerda, Token operador, NodoExpressao direita)
        {
            _esquerda = esquerda;
            _operador = operador;
            _direita = direita;
        }

        public override object Avaliar()
        {
            var esquerda = double.Parse(_esquerda.Avaliar().ToString());
            var direita = double.Parse(_direita.Avaliar().ToString());

            switch(_operador.Tipo)
            {
                case TokenTipo.OperadorSoma:
                    return esquerda + direita;
                case TokenTipo.OperadorSub:
                    return esquerda - direita;
                case TokenTipo.OperadorMult:
                    return esquerda * direita;
                case TokenTipo.OperadorDiv:
                    if(direita == 0)
                        Erro.ErroGenerico("Divisão por zero"); // TODO: Isso deveria ser checado aqui? R: não porra
                    return esquerda / direita;
            }
            
            return 0;
        }
    }

    public class NodoString : Nodo
    {
        private string _texto;

        public NodoString(NodoExpressao expressao)
        {
            _texto = expressao.Avaliar().ToString();
        }

        public NodoString(NodoExpressaoBooleana booleana)
        {
            _texto = booleana.Avaliar().ToString();
        }

        public NodoString(NodoInstrucaoTipo tipo)
        {
            _texto = tipo.Avaliar().ToString();
        }

        public NodoString(Token str)
        {
            if(str.Tipo != TokenTipo.StringLiteral)
                Erro.ErroGenerico($"Não é possível converter {str.Tipo} para String!");

            _texto = str.Valor.ToString();
        }

        public override object Avaliar()
        {
            return _texto;
        }
    }
}