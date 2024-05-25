using Libra.Arvore;

// TODO: Onde eu deixo isso?
public class Variavel
{
    public string Identificador;
    public string Valor;

    public Token Tipo { get; private set; }

    public Variavel()
    {

    }

    public Variavel(string identificador, string valor)
    {

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

            instrucao = sair;
        }

        else if(TentarConsumirToken(TokenTipo.Var) != null)
        {
            var ident = ParseInstrucaoVar();

            TentarConsumirToken(TokenTipo.PontoEVirgula, "Esperado `;`");
            
            instrucao = ident;
        }

        else if(TentarConsumirToken(TokenTipo.Exibir) != null)
        {
            var exibir = ParseInstrucaoExibir();

            TentarConsumirToken(TokenTipo.PontoEVirgula, "Esperado `;`");
            
            instrucao = exibir;
        }

        if(instrucao == null)
            LibraHelper.Erro("Instrução inválida!");

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

        // novo recorde: maior linha de código que já escrevi na vida
        nomeIdentificador = TentarConsumirToken(TokenTipo.Identificador, $"{Atual().Tipo} não pode ser usado como um identificador!").Valor.ToString();

        TentarConsumirToken(TokenTipo.OperadorDefinir, "Esperado `=`");

        NodoExpressao expressao = null;

        if(Atual().Valor != null)
        {
            expressao = ParseExpressao();
        }

        Variavel var = new Variavel();

        if(expressao != null)
        {
            var.Identificador = nomeIdentificador;
            var.Valor = expressao.Avaliar().ToString();

            LibraHelper.Variaveis[nomeIdentificador] = var.Valor;
        }

        return new NodoInstrucaoVar(var);
    }

    private NodoInstrucaoExibir ParseInstrucaoExibir()
    {
        NodoInstrucaoExibir exibir = null;

        TentarConsumirToken(TokenTipo.AbrirParen, "Esperado `(`");

        var str = ParseString();

        if(str != null)
        {
            exibir = new NodoInstrucaoExibir(str);
        }

        TentarConsumirToken(TokenTipo.FecharParen, "Esperado `)`");

        return exibir;
    }

    private NodoString ParseString()
    {
        NodoString nodoString;

        if(Atual().Tipo == TokenTipo.StringLiteral)
        {
            nodoString = new NodoString(ConsumirToken());
        }
        else
        {
            var expr = ParseExpressao();

            if(expr == null)
                LibraHelper.Erro("Expressão inválida");

            nodoString = new NodoString(expr);
        }

        return nodoString;
    }

    private NodoExpressao ParseExpressao()
    {
        NodoExpressao expressao = null;

        if(Atual().Tipo == TokenTipo.NumeroLiteral || Atual().Tipo == TokenTipo.Identificador)
        {
            if(Peek(1).Tipo == TokenTipo.OperadorSoma || Peek(1).Tipo == TokenTipo.OperadorSub
            || Peek(1).Tipo == TokenTipo.OperadorMult || Peek(1).Tipo == TokenTipo.OperadorDiv)
            {
                expressao = ParseExpressaoBinaria();
            }
            else
            {
                expressao = new NodoExpressaoTermo(ConsumirToken());
            }

        }

        if(expressao == null)
            LibraHelper.Erro("Expressão inválida!");

        return expressao;
    }

    private NodoExpressaoBinaria ParseExpressaoBinaria()
    {
        NodoExpressaoBinaria binaria = null;

        NodoExpressaoTermo esquerda = null; // TEM que ser um TERMO
        Token operador = null;
        NodoExpressao direita = null;

        if(Atual().Tipo == TokenTipo.NumeroLiteral || Atual().Tipo == TokenTipo.Identificador)
        {
            esquerda = new NodoExpressaoTermo(ConsumirToken());
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
            LibraHelper.Erro("Expressão binária inválida");

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

        LibraHelper.Erro(erro);

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