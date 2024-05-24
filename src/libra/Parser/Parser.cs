// preciso refatorar isso depois

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
                    return double.Parse(Libra.Variaveis[m_token.Valor.ToString()].ToString());
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
    private List<Token> m_tokens;
    private int m_posicao;


    public NodoPrograma Parse(List<Token> tokens)
    {
        m_tokens = tokens;

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

        else if(TentarConsumirToken(TokenTipo.Var) != null)
        {
            var ident = ParseInstrucaoVar();

            TentarConsumirToken(TokenTipo.PontoEVirgula, "Esperado `;`");
            
            instrucao = new NodoInstrucao(ident);
        }

        else if(TentarConsumirToken(TokenTipo.Imprimir) != null)
        {
            var imprimir = ParseInstrucaoImprimir();

            TentarConsumirToken(TokenTipo.PontoEVirgula, "Esperado `;`");
            
            instrucao = new NodoInstrucao(imprimir);
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
        string nomeIdentificador = "";

        Token identificador = null;

        if(Atual().Tipo == TokenTipo.Identificador)
        {
            nomeIdentificador = Atual().Valor.ToString();
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
            var.Identificador = nomeIdentificador;
            var.Valor = expressao.Avaliar();

            Libra.Variaveis[nomeIdentificador] = var.Valor;
        }

        return new NodoInstrucaoVar(var);
    }

    private NodoInstrucaoImprimir ParseInstrucaoImprimir()
    {
        NodoInstrucaoImprimir imprimir = null;

        TentarConsumirToken(TokenTipo.AbrirParen, "Esperado `(`");

        var expressao = ParseExpressao();

        if(expressao != null)
        {
            imprimir = new NodoInstrucaoImprimir(expressao);
        }

        TentarConsumirToken(TokenTipo.FecharParen, "Esperado `)`");

        return imprimir;
    }

    private NodoExpressao ParseExpressao()
    {
        NodoExpressao expressao = null;

        if(Atual().Tipo == TokenTipo.Numero || Atual().Tipo == TokenTipo.Identificador)
        {
            if(Peek(1).Tipo == TokenTipo.OperadorSoma || Peek(1).Tipo == TokenTipo.OperadorSub
            || Peek(1).Tipo == TokenTipo.OperadorMult || Peek(1).Tipo == TokenTipo.OperadorDiv)
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

        if(Atual().Tipo == TokenTipo.OperadorSoma || Atual().Tipo == TokenTipo.OperadorSub
        || Atual().Tipo == TokenTipo.OperadorMult || Atual().Tipo == TokenTipo.OperadorDiv)
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
        if(m_posicao + offset < m_tokens.Count)
        {
            return m_tokens[m_posicao + offset];
        }

        return new Token(TokenTipo.FimDoArquivo);
    }

    private void Passar(int quantidade = 1) 
    {
        m_posicao += quantidade;
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