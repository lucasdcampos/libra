// TODO: O Parser por completo precisa ser revisado e reescrito.
// Quando eu programei isso, provavelmente era de madrugada e eu só fui escrevendo.
// Agora eu criei um documento especificando a gramática que vai ajudar.
// Além disso, vou seguir o planejamento, ao invés de sair programando aleatoriamente,
// que nao da certo ¯\_(ツ)_/¯ 
using Libra.Arvore;

namespace Libra;

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

        else if (TentarConsumirToken(TokenTipo.Tipo) != null)
        {
            var tipo = ParseInstrucaoTipo();

            //TentarConsumirToken(TokenTipo.PontoEVirgula);

            instrucao = tipo;

            if (instrucao == null)
                Erro.ErroGenerico("Instrução tipo() inválida!", m_linha);
        }

        else if (TentarConsumirToken(TokenTipo.Se) != null)
        {
            var se = ParseInstrucaoSe();

            instrucao = se;

            if (instrucao == null)
                Erro.ErroGenerico("Instrução se() inválida!", m_linha);
        }

        else
        { 
            Erro.ErroGenerico($"Instrução inválida: {Proximo(0).Tipo} --> {ConsumirToken().Valor}");
        }


        return instrucao;
    }

    private NodoInstrucaoSair? ParseInstrucaoSair()
    {
        NodoInstrucaoSair? sair = null;

        TentarConsumirToken(TokenTipo.AbrirParen);
        if(Atual().Tipo == TokenTipo.FecharParen)
        {
            sair = new NodoInstrucaoSair(new NodoExpressaoTermo(new Token(TokenTipo.NumeroLiteral, 0)));

            TentarConsumirToken(TokenTipo.FecharParen);

            return sair;
        }

        sair = new NodoInstrucaoSair(ParseExpressao());

        TentarConsumirToken(TokenTipo.FecharParen);

        return sair;
    }

    // Isso ainda não funciona (tá quase funcionando!)
    private NodoInstrucaoSe? ParseInstrucaoSe()
    {
        NodoInstrucaoSe? se = null;
        List<NodoInstrucao> escopo = new List<NodoInstrucao>();
        //NodoExpressaoBooleana expressao = null;
        Token expressao = null; 
        TentarConsumirToken(TokenTipo.AbrirParen);

        expressao = TentarConsumirToken(TokenTipo.BoolLiteral);

        TentarConsumirToken(TokenTipo.FecharParen);
        TentarConsumirToken(TokenTipo.Entao);

        while(Atual().Tipo != TokenTipo.Fim)
        {
            var instrucao = ParseInstrucao();

            if(instrucao != null)
            {
                escopo.Add(instrucao);
            }
            else
            {
                Erro.ErroGenerico("Instrução inválida!", m_linha);
            }
        }

        TentarConsumirToken(TokenTipo.Fim);

        se = new NodoInstrucaoSe(expressao, escopo);

        return se;
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

    private NodoInstrucaoTipo? ParseInstrucaoTipo()
    {
        NodoInstrucaoTipo? tipo = null;

        TentarConsumirToken(TokenTipo.AbrirParen, "Esperado `(`");

        tipo = new NodoInstrucaoTipo(ConsumirToken());

        TentarConsumirToken(TokenTipo.FecharParen, "Esperado `)`");

        return tipo;
    }

    private NodoString? ParseString()
    {
        NodoString? nodoString;

        if(Atual().Tipo == TokenTipo.StringLiteral)
        {
            nodoString = new NodoString(ConsumirToken());
        }

        else if (Atual().Tipo == TokenTipo.BoolLiteral)
        {
            nodoString = new NodoString(new NodoExpressaoBooleana(ConsumirToken()));
        }
        else if (Atual().Tipo == TokenTipo.Tipo)
        {
            nodoString = new NodoString(ParseInstrucaoTipo());
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
            if(Proximo(1).Tipo == TokenTipo.OperadorSoma || Proximo(1).Tipo == TokenTipo.OperadorSub
            || Proximo(1).Tipo == TokenTipo.OperadorMult || Proximo(1).Tipo == TokenTipo.OperadorDiv)
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

    // TODO: Implementar ordem correta das operações
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

    // TODO: Fazer funcionar e adicionar outros operadores
    private NodoExpressaoBooleana? ParseExpressaoBooleana()
    {
        NodoExpressaoBooleana booleana = null;

        object esq = null;
        Token opr = null;
        object dir = null;

        if (Proximo(1).Tipo == TokenTipo.OperadorMaiorQue)
        {
            esq = ParseExpressao();
            opr = ConsumirToken();
            dir = ParseExpressao();
        }

        if (esq != null && dir != null)
        {
            booleana = new NodoExpressaoBooleana((NodoExpressao)esq, opr, (NodoExpressao)dir);
        }

        return booleana;
    }

    private Token Atual() 
    {
        return Proximo(0);
    }

    private Token Proximo(int offset)
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