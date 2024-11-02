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

        while(TentarConsumirToken(TokenTipo.FimDoArquivo) == null)
        {
            instrucoes.Add(ParseInstrucao("'Programa'"));
        }

        return new Programa(instrucoes);
    }

    private Instrucao? ParseInstrucao(string chamador = "")
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

        Erros.LancarErro(new Erro($"Instrução inválida: {Atual().Tipo.ToString()}, por {chamador}", _linha));

        return null;
    }

    int escopos = 0;
    private Escopo? ParseEscopo()
    {
        var instrucoes = new List<Instrucao>();

        while(TentarConsumirToken(TokenTipo.Fim) == null)
        {
            var instrucao = ParseInstrucao("Escopo");

            instrucoes.Add(instrucao);
        }

        escopos++;

        return new Escopo(instrucoes);
    }

    private InstrucaoSair? ParseInstrucaoSair()
    {
        ConsumirToken(TokenTipo.Sair);
        ConsumirToken(TokenTipo.AbrirParen);

        // Assim podemos sair sem passar nenhum argumento. Ex: sair()
        if(TentarConsumirToken(TokenTipo.FecharParen) != null)
            return new InstrucaoSair(new ExpressaoTermo(new Token(TokenTipo.NumeroLiteral, 0)));;

        var expr = ParseExpressao();

        ConsumirToken(TokenTipo.FecharParen);

        return new InstrucaoSair(expr);
    }

    private InstrucaoVar? ParseInstrucaoVar(bool declaracao = true)
    {
        TentarConsumirToken(TokenTipo.Var);

        string identificador = ConsumirToken(TokenTipo.Identificador).Valor;

        ConsumirToken(TokenTipo.OperadorDefinir);

        var expressao = ParseExpressao();

        return new InstrucaoVar(identificador, expressao, declaracao);
    }

    private InstrucaoConst? ParseInstrucaoConst()
    {
        ConsumirToken(TokenTipo.Const);

        string identificador = ConsumirToken(TokenTipo.Identificador).Valor;

        ConsumirToken(TokenTipo.OperadorDefinir);

        var expressao = ParseExpressao();

        return new InstrucaoConst(identificador, expressao);
    }

    private InstrucaoChamadaFuncao? ParseInstrucaoChamadaFuncao()
    {
        string identificador = ConsumirToken(TokenTipo.Identificador).Valor;

        ConsumirToken(TokenTipo.AbrirParen);

        var argumentos = new List<Expressao>();
        while(Atual().Tipo != TokenTipo.FecharParen)
        {
            var expr = ParseExpressao();
            argumentos.Add(expr);

            if(expr == null)
                Erros.LancarErro(new ErroEsperado("expressão", _linha));

            TentarConsumirToken(TokenTipo.Virgula);
        }

        ConsumirToken(TokenTipo.FecharParen);

        return new InstrucaoChamadaFuncao(identificador, argumentos);
    }

    private InstrucaoFuncao? ParseInstrucaoFuncao()
    {
        TentarConsumirToken(TokenTipo.Funcao);
        
        string? identificador = ConsumirToken(TokenTipo.Identificador)?.Valor;

        ConsumirToken(TokenTipo.AbrirParen);

        var parametros = new List<string>();

        while(Atual().Tipo != TokenTipo.FecharParen)
        {
            if(Atual().Tipo != TokenTipo.Identificador)
                ConsumirToken(TokenTipo.FecharParen); // tentar fechar paren (vai dar erro da msm forma)
            
            var param = ConsumirToken()?.Valor;
            parametros.Add(param);

            TentarConsumirToken(TokenTipo.Virgula);
        }

        ConsumirToken(TokenTipo.FecharParen);

        var escopo = ParseEscopo();

        return new InstrucaoFuncao(identificador, escopo, parametros);
    }
    
    private InstrucaoSe? ParseInstrucaoSe()
    {
        ConsumirToken(TokenTipo.Se);

        var expressao = ParseExpressao();

        ConsumirToken(TokenTipo.Entao);

        var escopo = ParseEscopo();
        
        Escopo escopoSenao = null;
        if(TentarConsumirToken(TokenTipo.Senao) != null)
            escopoSenao = ParseEscopo();

        return new InstrucaoSe(expressao, escopo, escopoSenao);
    }

    private InstrucaoEnquanto? ParseInstrucaoEnquanto()
    {
        ConsumirToken(TokenTipo.Enquanto);

        var expressao = ParseExpressao();
        ConsumirToken(TokenTipo.Faca);

        var escopo = ParseEscopo();

        return new InstrucaoEnquanto(expressao, escopo);
    }

    private Expressao? ParseExpressao()
    {

        if(Atual().Tipo == TokenTipo.NumeroLiteral || Atual().Tipo == TokenTipo.CaractereLiteral ||
           Atual().Tipo == TokenTipo.Identificador)
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

        Erros.LancarErro(new Erro("Impossível determinar expressão", _linha, 1000));
        return null;
    }

    // TODO: Implementar ordem correta das operações
    private ExpressaoBinaria? ParseExpressaoBinaria()
    {
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

        if(esquerda == null || direita == null)
        {
            Erros.LancarErro(new Erro("Impossível determinar expressão", _linha));
        }

        TentarConsumirToken(TokenTipo.FecharParen);

        return new ExpressaoBinaria(esquerda, operador, direita);;
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

    private Token? ConsumirToken(TokenTipo tipo = TokenTipo.TokenInvalido)
    {
        var token = Atual();
        
        if(tipo != TokenTipo.TokenInvalido && token.Tipo != tipo)
        {
            Erros.LancarErro(new ErroEsperado(Token.TipoParaString(tipo)));
        }

        Passar();

        _linha = token.Linha;

        return token;
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