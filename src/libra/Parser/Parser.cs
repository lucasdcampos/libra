using Libra.Arvore;

namespace Libra;

public class Parser
{
    private List<Token> _tokens;
    private int _posicao;
    private int _linha;

    public Programa Parse(List<Token> tokens)
    {
        _tokens = tokens;

        foreach(var t in tokens)
            LibraLogger.Debug(t.ToString(),5);
        
        var instrucoes = new List<Instrucao>();

        var i = ParseInstrucao();

        while(i != null)
        {
            instrucoes.Add(i);
            i = ParseInstrucao();
        }

        return new Programa(instrucoes);
    }

    private Instrucao? ParseInstrucao()
    {
        switch(Atual().Tipo)
        {
            case TokenTipo.Sair: return ParseInstrucaoSair();
            case TokenTipo.Var: return ParseInstrucaoVar();
            case TokenTipo.Const: return ParseInstrucaoConst();
            case TokenTipo.Funcao: return ParseInstrucaoFuncao();
            case TokenTipo.Se: return ParseInstrucaoSe();
            case TokenTipo.Enquanto: return ParseInstrucaoEnquanto();
            case TokenTipo.Romper: return new InstrucaoRomper();
            case TokenTipo.Identificador:
                if(Proximo(1).Tipo == TokenTipo.AbrirParen)
                    return ParseInstrucaoChamadaFuncao();
                else
                    return ParseInstrucaoVar(false);
        }

        return null;
    }

    private Escopo? ParseEscopo()
    {
        var instrucoes = new List<Instrucao>();

        var i = ParseInstrucao();

        while(i != null)
        {
            Console.WriteLine(i);
            instrucoes.Add(i);
            i = ParseInstrucao();
        }

        TentarConsumirToken(TokenTipo.Fim);
        
        return new Escopo(instrucoes);
    }

    private InstrucaoSair? ParseInstrucaoSair()
    {
        TentarConsumirToken(TokenTipo.Sair);
        TentarConsumirToken(TokenTipo.AbrirParen);

        // Assim podemos sair sem passar nenhum argumento. Ex: sair()
        if(Atual().Tipo == TokenTipo.FecharParen)
        {
            ConsumirToken();
            return new InstrucaoSair(new ExpressaoTermo(new Token(TokenTipo.NumeroLiteral, 0)));;
        }

        var expr = ParseExpressao();

        TentarConsumirToken(TokenTipo.FecharParen);

        return new InstrucaoSair(expr);
    }

    private InstrucaoVar? ParseInstrucaoVar(bool declaracao = true)
    {
        TentarConsumirToken(TokenTipo.Var);

        string identificador = TentarConsumirToken(TokenTipo.Identificador).Valor;

        TentarConsumirToken(TokenTipo.OperadorDefinir);

        var expressao = ParseExpressao();

        return new InstrucaoVar(identificador, expressao, declaracao);
    }

    private InstrucaoConst? ParseInstrucaoConst()
    {
        TentarConsumirToken(TokenTipo.Const);

        string identificador = TentarConsumirToken(TokenTipo.Identificador).Valor;

        TentarConsumirToken(TokenTipo.OperadorDefinir);

        var expressao = ParseExpressao();

        return new InstrucaoConst(identificador, expressao);
    }

    private InstrucaoChamadaFuncao? ParseInstrucaoChamadaFuncao()
    {
        string identificador = TentarConsumirToken(TokenTipo.Identificador).Valor;

        TentarConsumirToken(TokenTipo.AbrirParen);

        var argumentos = new List<Expressao>();
        while(Atual().Tipo != TokenTipo.FecharParen)
        {
            var expr = ParseExpressao();
            argumentos.Add(expr);

            TentarConsumirToken(TokenTipo.Virgula);
        }

        TentarConsumirToken(TokenTipo.FecharParen);

        return new InstrucaoChamadaFuncao(identificador, argumentos);
    }

    private InstrucaoFuncao? ParseInstrucaoFuncao()
    {
        TentarConsumirToken(TokenTipo.Funcao);
        
        string identificador = TentarConsumirToken(TokenTipo.Identificador).Valor;
        var instrucoes = new List<Instrucao>();

        TentarConsumirToken(TokenTipo.AbrirParen);

        var parametros = new List<string>();

        while(Atual().Tipo != TokenTipo.FecharParen)
        {
            if(Atual().Tipo != TokenTipo.Identificador)
                Erros.LancarErro(new ErroEsperado($"Identificador", Atual().Linha));
            
            parametros.Add(ConsumirToken().Valor);
            TentarConsumirToken(TokenTipo.Virgula);
        }

        TentarConsumirToken(TokenTipo.FecharParen, "Esperado `)`");

        var escopo = ParseEscopo();

        TentarConsumirToken(TokenTipo.Fim);

        return new InstrucaoFuncao(identificador, escopo, parametros);
    }

    
    private InstrucaoSe? ParseInstrucaoSe()
    {
        TentarConsumirToken(TokenTipo.Se);

        var expressao = ParseExpressao();

        TentarConsumirToken(TokenTipo.Entao);

        var escopo = ParseEscopo();

        TentarConsumirToken(TokenTipo.Fim);
        
        Escopo escopoSenao = null;
        if(TentarConsumirToken(TokenTipo.Senao) != null)
            escopoSenao = ParseEscopo();

        return new InstrucaoSe(expressao, escopo, escopoSenao);
    }

    private InstrucaoEnquanto? ParseInstrucaoEnquanto()
    {
        TentarConsumirToken(TokenTipo.Enquanto);

        var expressao = ParseExpressao();
        TentarConsumirToken(TokenTipo.Faca);

        var escopo = ParseEscopo();

        TentarConsumirToken(TokenTipo.Fim);

        return new InstrucaoEnquanto(expressao, escopo);
    }

    private Expressao? ParseExpressao()
    {

        if(Atual().Tipo == TokenTipo.NumeroLiteral || Atual().Tipo == TokenTipo.Identificador)
        {
            if(TokenEhOperador(Proximo(1)))
            {
                return ParseExpressaoBinaria();
            }
            else
            {
                return new ExpressaoTermo(ConsumirToken());
            }
        }

        return null;
    }

    // TODO: Implementar ordem correta das operações
    private ExpressaoBinaria? ParseExpressaoBinaria()
    {
        ExpressaoBinaria? binaria = null;

        ExpressaoTermo? esquerda = null; // TEM que ser um TERMO
        Token? operador = null;
        Expressao? direita = null;

        TentarConsumirToken(TokenTipo.AbrirParen);

        if(Atual().Tipo == TokenTipo.NumeroLiteral || Atual().Tipo == TokenTipo.Identificador)
        {
            esquerda = new ExpressaoTermo(ConsumirToken());
        }

        if(TokenEhOperador(Atual()))
        {
            operador = ConsumirToken();

            direita = ParseExpressao();
        }

        if(esquerda != null && direita != null)
        {
            binaria = new ExpressaoBinaria(esquerda, operador, direita);
        }

        TentarConsumirToken(TokenTipo.AbrirParen);

        return binaria;
    }

    private bool TokenEhOperador(Token token)
    {
        switch(token.Tipo)
        {
            case TokenTipo.OperadorSoma:
            case TokenTipo.OperadorSub:
            case TokenTipo.OperadorMult:
            case TokenTipo.OperadorDiv:
            case TokenTipo.OperadorMaiorQue:
            case TokenTipo.OperadorMaiorIgualQue:
            case TokenTipo.OperadorMenorQue:
            case TokenTipo.OperadorMenorIgualQue:
            case TokenTipo.OperadorComparacao:
            case TokenTipo.OperadorOu:
            case TokenTipo.OperadorE:
            case TokenTipo.OperadorOuExclusivo:
            case TokenTipo.OperadorDiferente:
            case TokenTipo.OperadorNeg:
                return true;
        }

        return false;
    }

    private bool NaoForFim()
    {
        return Atual().Tipo != TokenTipo.FimDoArquivo;
    }

    private Token Atual() 
    {
        return Proximo(0);
    }

    private Token Proximo(int offset)
    {
        if(_posicao + offset < _tokens.Count)
        {
            return _tokens[_posicao + offset];
        }

        return new Token(TokenTipo.FimDoArquivo, _linha);
    }

    private void Passar(int quantidade = 1) 
    {
        _posicao += quantidade;
    }

    private Token? ConsumirToken()
    {
        var token = Atual();
        Passar();

        _linha = token.Linha;

        return token;
    }

    private Token? TentarConsumirToken(TokenTipo tipo, string erro)
    {
        if(Atual().Tipo == tipo)
        {
            return ConsumirToken();
        }

        Erros.LancarErro(new ErroEsperado(tipo.ToString(), _linha));

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