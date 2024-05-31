using Libra.Arvore;

public class Parser
{
    private List<Token> m_tokens;
    private int m_posicao;
    private int m_linha;

    public NodoPrograma Parse(List<Token> tokens, bool debugTokens = false)
    {
        m_tokens = tokens;

        if(debugTokens)
            foreach(var t in tokens)
                Console.WriteLine(t.ToString());
        

        NodoPrograma? programa = null;

        var instrucoes = new List<NodoInstrucao>();

        while(Atual().Tipo != TokenTipo.FimDoArquivo)
        {
            instrucoes.Add(ParseInstrucao());
        }

        programa = new NodoPrograma(instrucoes);

        if(programa == null)
            Erro.ErroGenerico("Programa inválido. Não foi possível determinar as instruções");

        return programa;
    }

    private NodoInstrucao? ParseInstrucao()
    {
        NodoInstrucao? instrucao = null;

        if(TentarConsumirToken(TokenTipo.Sair) != null)
        {
            var sair = ParseInstrucaoSair();
            TentarConsumirToken(TokenTipo.PontoEVirgula);

            instrucao = sair;

            if(instrucao == null)
                Erro.ErroGenerico("Instrução sair() inválida!", m_linha);
        }

        else if(TentarConsumirToken(TokenTipo.Var) != null)
        {
            var ident = ParseInstrucaoVar();

            TentarConsumirToken(TokenTipo.PontoEVirgula);
            
            instrucao = ident;

            if(instrucao == null)
                Erro.ErroGenerico("Declaração de variáveis inválida!", m_linha);
        }

        else if(TentarConsumirToken(TokenTipo.Exibir) != null)
        {
            var exibir = ParseInstrucaoExibir();

            TentarConsumirToken(TokenTipo.PontoEVirgula);
            
            instrucao = exibir;

            if(instrucao == null)
                Erro.ErroGenerico("Instrução exibir() inválida!", m_linha);
        }

        else
        { 
            Erro.ErroGenerico($"Instrução inválida: {Peek(0).Tipo} --> {ConsumirToken().Valor}");
        }


        return instrucao;
    }

    private NodoInstrucaoSair? ParseInstrucaoSair()
    {
        NodoInstrucaoSair? sair = null;

        TentarConsumirToken(TokenTipo.AbrirParen);

        sair = new NodoInstrucaoSair(ParseExpressao());

        TentarConsumirToken(TokenTipo.FecharParen);

        return sair;
    }

    private NodoInstrucaoVar? ParseInstrucaoVar()
    {
        // var nome = valor;
        //     ^^^^
        var nomeIdentificador = TentarConsumirToken(TokenTipo.Identificador).Valor.ToString();


        // var nome = valor;
        //          ^
        TentarConsumirToken(TokenTipo.OperadorDefinir);

        Variavel variavel = null;
        
        // var nome = valor;
        //            ^^^^^
        if(Atual().Tipo == TokenTipo.StringLiteral)
        {
            var str = ParseString();
            if(str == null)
                Erro.ErroGenerico("String inválida");

            variavel = new Variavel(nomeIdentificador, str);
        }
        
        else if(Atual().Valor != null)
        {
            var expressao = ParseExpressao();

            if(expressao == null)
                Erro.ErroGenerico("Expressão inválida");

            variavel = new Variavel(nomeIdentificador, expressao);
            
        }

        if(variavel == null)
            Erro.ErroGenerico("Variável inválida");

        LibraHelper.Variaveis[variavel.Identificador] = variavel.Valor;
        
        return new NodoInstrucaoVar(variavel);
    }

    private NodoInstrucaoExibir? ParseInstrucaoExibir()
    {
        NodoInstrucaoExibir? exibir = null;

        TentarConsumirToken(TokenTipo.AbrirParen, "Esperado `(`");

        var str = ParseString();

        if(str != null)
        {
            exibir = new NodoInstrucaoExibir(str);
        }

        TentarConsumirToken(TokenTipo.FecharParen, "Esperado `)`");

        return exibir;
    }

    private NodoString? ParseString()
    {
        NodoString? nodoString;

        if(Atual().Tipo == TokenTipo.StringLiteral)
        {
            nodoString = new NodoString(ConsumirToken());
        }
        else
        {
            var expr = ParseExpressao();

            if(expr == null)
                Erro.ErroGenerico("Expressão inválida", m_linha);

            nodoString = new NodoString(expr);
        }

        return nodoString;
    }

    private NodoExpressao? ParseExpressao()
    {
        NodoExpressao? expressao = null;

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
            Erro.ErroGenerico("Expressão inválida!", m_linha);

        return expressao;
    }

    private NodoExpressaoBinaria? ParseExpressaoBinaria()
    {
        NodoExpressaoBinaria? binaria = null;

        NodoExpressaoTermo? esquerda = null; // TEM que ser um TERMO
        Token? operador = null;
        NodoExpressao? direita = null;

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
            Erro.ErroGenerico("Expressão binária inválida", m_linha);

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

        return new Token(TokenTipo.FimDoArquivo, m_linha);
    }

    private void Passar(int quantidade = 1) 
    {
        m_posicao += quantidade;
    }

    private Token? ConsumirToken()
    {
        var token = Atual();
        Passar();

        m_linha = token.Linha;

        return token;
    }

    private Token? TentarConsumirToken(TokenTipo tipo, string erro)
    {
        if(Atual().Tipo == tipo)
        {
            return ConsumirToken();
        }

        Erro.ErroEsperado(tipo, m_linha);

        return null;
    }

    private Token? TentarConsumirToken(TokenTipo tipo)
    {
        if(Atual().Tipo == tipo)
        {
            return ConsumirToken();
        }

        return null;
    }
}