namespace Libra.Arvore
{
    public abstract class NodoInstrucao : Nodo {} // TODO: Por que não interface? Não me pergunte

    public class NodoInstrucaoSair : NodoInstrucao
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

    public class NodoInstrucaoVar : NodoInstrucao
    {
        public NodoInstrucaoVar(Variavel var)
        {
            _var = var;
        }

        private Variavel _var;

        public override object Avaliar()
        {
            return _var;
        }
    }

    public class NodoInstrucaoTipo : NodoInstrucao
    {
        public NodoInstrucaoTipo(Token token)
        {
            Token = token;
        }

        public Token Token { get; private set; }

        public override object Avaliar()
        {
            return Token.TipoParaString(Token.Tipo);
        }

    }

    public class NodoInstrucaoSe : NodoInstrucao
    {
        public NodoInstrucaoSe(NodoExpressaoBooleana condicao, List<NodoInstrucao> instrucoes)
        {
            Condicao = condicao;
            Escopo = instrucoes;
        }

        public NodoInstrucaoSe(Token condicao, List<NodoInstrucao> instrucoes)
        {
            Condicao = condicao;
            Escopo = instrucoes;
        }

        public object Condicao { get; private set; }
        public List<NodoInstrucao> Escopo = new List<NodoInstrucao>();

        public override object Avaliar()
        {
            //var valor = Condicao.Avaliar().ToString();
            Token token = (Token)Condicao;
            var valor = token.Valor.ToString();
            return valor;
        }

    }

    public class NodoInstrucaoExibir : NodoInstrucao
    {
        public NodoInstrucaoExibir(NodoString str)
        {
            _string = str;
        }

        private NodoString _string;

        public override object Avaliar()
        {
            return _string.Avaliar();
        }
    }
}