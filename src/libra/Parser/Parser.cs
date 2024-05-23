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

// termo matemático
public class NodoTermo : Nodo
{
    public NodoTermo(Token token)
    {
        _token = token;
    }

    private Token _token;

    public override object Avaliar()
    {
        switch (_token.Tipo)
            {
                case TokenTipo.Numero:
                    return _token.Valor;
                case TokenTipo.Identificador:
                    return double.Parse(Libra.Variaveis[_token.Valor.ToString()].ToString());
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
        if(_operador.Tipo == TokenTipo.OperadorSoma)
        {
            double resultado = double.Parse(_esquerda.Avaliar().ToString()) + double.Parse(_direita.Avaliar().ToString());
            return resultado;
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

        if(TentarConsumirToken(TokenTipo.Sair) != null)
        {
            var sair = ParseInstrucaoSair();
            TentarConsumirToken(TokenTipo.PontoEVirgula, "Esperado `;`");

            instrucao = new NodoInstrucao(sair);
        }

        if(TentarConsumirToken(TokenTipo.Var) != null)
        {
            var ident = ParseInstrucaoVar();

            TentarConsumirToken(TokenTipo.PontoEVirgula, "Esperado `;`");
            
            instrucao = new NodoInstrucao(ident);
        }

        if(instrucao == null)
            Libra.Erro("Instrução inválida!");

        return instrucao;
    }

    private NodoInstrucaoSair ParseInstrucaoSair()
    {
        NodoInstrucaoSair sair = null;

        TentarConsumirToken(TokenTipo.AbrirParen, "Esperado `(`");

        sair = new NodoInstrucaoSair(ParseExpressao());

        TentarConsumirToken(TokenTipo.FecharParen, "Esperado `)`");

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

        TentarConsumirToken(TokenTipo.FecharParen, "Esperado `=`");

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
                expressao = new NodoExpressao(new NodoTermo(ConsumirToken()));
            }

        }

        if(expressao == null)
            Libra.Erro("Expressão inválida!");

        return expressao;
    }

    private NodoExpressaoBinaria ParseExpressaoBinaria()
    {
        NodoExpressaoBinaria binaria = null;

        NodoExpressao esquerda = null;
        Token operador = null;
        NodoExpressao direita = null;

        if(Atual().Tipo == TokenTipo.Numero || Atual().Tipo == TokenTipo.Identificador)
        {
            esquerda = new NodoExpressao(new NodoTermo(ConsumirToken()));
        }

        if(Atual().Tipo == TokenTipo.OperadorSoma)
        {
            operador = ConsumirToken();

            direita = ParseExpressao();
        }

        if(esquerda != null && direita != null)
        {
            binaria = new NodoExpressaoBinaria(esquerda, operador, direita);
        }

        if(binaria == null)
            Libra.Erro("Expressão binária inválida");

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

    private Token TentarConsumirToken(TokenTipo tipo, string erro)
    {
        if(Atual().Tipo == tipo)
        {
            return ConsumirToken();
        }

        Libra.Erro(erro);

        return null;
    }

    private Token TentarConsumirToken(TokenTipo tipo)
    {
        if(Atual().Tipo == tipo)
        {
            return ConsumirToken();
        }

        return null;
    }
}