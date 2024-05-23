// Árvore Semântica
// Esse arquivo necessita revisão

// * --> revisar

public abstract class Nodo
{
    // cada nó retornará um tipo de valor diferente
    public abstract object Avaliar();
}

public struct Var
{
    public string Identificador;
    public object Valor;
}

public class NodoExpressao : Nodo
{
    public NodoExpressao(Token token)
    {
        if(token.Tipo == TokenTipo.Numero || token.Tipo == TokenTipo.Identificador)
        {
            Expressao = token;
        }
    }

    public NodoExpressao(NodoExpressaoBinaria exprBinaria)
    {
        Expressao = exprBinaria;
    }

    public object Expressao { get; private set; }

    public override object Avaliar()
    {
        if(Expressao.GetType() == typeof(Token))
        {
            var token = (Token)Expressao;

            switch (token.Tipo)
            {
                case TokenTipo.Numero:
                    return token.Valor;
                case TokenTipo.Identificador:
                    return double.Parse(token.Valor.ToString()); // *
            }
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
    private Token _esquerda;
    private Token _operador;
    private Token _direita;

    public NodoExpressaoBinaria(Token esquerda, Token operador, Token direita)
    {
        _esquerda = esquerda;
        _operador = operador;
        _direita = direita;
    }

    public override object Avaliar()
    {
        if(_operador.Tipo == TokenTipo.OperadorSoma)
        {
            return double.Parse(_esquerda.Valor.ToString()) + double.Parse(_direita.Valor.ToString());
        }

        else if(_operador.Tipo == TokenTipo.OperadorSub)
        {
            return double.Parse(_esquerda.Valor.ToString()) - double.Parse(_direita.Valor.ToString());
        }

        else if(_operador.Tipo == TokenTipo.OperadorMult)
        {
            return double.Parse(_esquerda.Valor.ToString()) * double.Parse(_direita.Valor.ToString());
        }

        else if(_operador.Tipo == TokenTipo.OperadorDiv)
        {
            if(double.Parse(_direita.Valor.ToString()) == 0)
                Libra.Erro("Divisão por zero");
    
            return double.Parse(_esquerda.Valor.ToString()) + double.Parse(_direita.Valor.ToString());
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
        _var = var;
    }

    private Var _var;

    public override object Avaliar()
    {
        return _var;
    }
}

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

public class NodoPrograma : Nodo
{
    public NodoPrograma(List<NodoInstrucao> instrucoes)
    {
        Instrucoes = instrucoes;
    }

    public List<NodoInstrucao> Instrucoes { get; private set; }

    public int CodigoSaida { get; private set; }

    public override object Avaliar()
    {
        return CodigoSaida;
    }

}

public class Parser
{
    public Parser(List<Token> tokens)
    {
        _tokens = tokens;
    }

    private List<Token> _tokens;
    private int _posicao;


    public NodoPrograma Parse()
    {
        NodoPrograma programa = null;

        var instrucoes = new List<NodoInstrucao>();

        while(Atual().Tipo != TokenTipo.FimDoArquivo)
        {
            instrucoes.Add(ParseInstrucao());
        }

        programa = new NodoPrograma(instrucoes);

        return programa;
    }

    private NodoInstrucao ParseInstrucao()
    {
        NodoInstrucao instrucao = null;

        if(Atual().Tipo == TokenTipo.Sair)
        {
            Passar();

            var sair = ParseInstrucaoSair();

            if(Atual().Tipo != TokenTipo.PontoEVirgula)
            {
                Libra.Erro("Esperado ';'");
            }

            Passar();
            
            instrucao = new NodoInstrucao(sair);
        }

        if(Atual().Tipo == TokenTipo.Var)
        {
            Passar();

            var ident = ParseInstrucaoVar();

            if(Atual().Tipo != TokenTipo.PontoEVirgula)
            {
                Libra.Erro("Esperado ';'");
            }

            Passar();
            
            instrucao = new NodoInstrucao(ident);
        }

        if(instrucao == null)
            Libra.Erro("Instrução inválida!");

        return instrucao;
    }

    private NodoInstrucaoSair ParseInstrucaoSair()
    {
        NodoInstrucaoSair sair = null;

        if(Atual().Tipo == TokenTipo.AbrirParen)
        {
            Passar();
        }
        else
        {
            Libra.Erro("Esperado '('");
        }

        sair = new NodoInstrucaoSair(ParseExpressao());

        if(Atual().Tipo == TokenTipo.FecharParen)
        {
            Passar();
        }
        else
        {
            Libra.Erro("Esperado ')'");
        }

        return sair;
    }

    private NodoInstrucaoVar ParseInstrucaoVar()
    {
        string nome_identificador = "";

        Token identificador = null;

        if(Atual().Tipo == TokenTipo.Identificador)
        {
            nome_identificador = Atual().Valor.ToString();
            identificador = Atual();
            Passar();
        }

        if(Atual().Tipo == TokenTipo.OperadorDefinir)
        {
            Passar();
        }

        NodoExpressao expressao = null;

        if(Atual().Valor != null)
        {
            expressao = ParseExpressao();
        }

        Var var = new Var();

        if(expressao != null)
        {
            var.Identificador = nome_identificador;
            var.Valor = expressao.Avaliar();

            Libra.Variaveis[nome_identificador] = var.Valor;
        }

        return new NodoInstrucaoVar(var);
    }

    
    private NodoExpressao ParseExpressao()
    {
        NodoExpressao expressao = null;

        if(Atual().Tipo == TokenTipo.Numero || Atual().Tipo == TokenTipo.Identificador)
        {
            if(Peek(1).Tipo == TokenTipo.OperadorSoma)
            {
                expressao = new NodoExpressao(ParseExpressaoBinaria());
            }
            else
            {
                expressao = new NodoExpressao(ConsumirToken());
            }

        }

        if(expressao == null)
            Libra.Erro("Expressão inválida!");

        return expressao;
    }

    private NodoExpressaoBinaria ParseExpressaoBinaria()
    {
        NodoExpressaoBinaria binaria = null;

        if(Peek(1).Tipo == TokenTipo.OperadorSoma)
        {
            if(Atual().Tipo == TokenTipo.Numero && Peek(2).Tipo == TokenTipo.Numero)
            {
                binaria = new NodoExpressaoBinaria(ConsumirToken(), ConsumirToken(), ConsumirToken());
            }
        }

        return binaria;
    }

    private Token Atual() 
    {
        return Peek(0);
    }
    private Token Peek(int offset)
    {
        if(_posicao + offset < _tokens.Count)
        {
            return _tokens[_posicao + offset];
        }

        return new Token(TokenTipo.FimDoArquivo);
    }

    private void Passar(int quantidade = 1) 
    {
        _posicao += quantidade;
    }

    private Token ConsumirToken()
    {
        var token = Atual();
        Passar();

        return token;
    }
}