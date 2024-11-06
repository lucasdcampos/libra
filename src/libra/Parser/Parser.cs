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

        return new Programa(ParseInstrucoes(TokenTipo.FimDoArquivo));
    }

    private Instrucao[] ParseInstrucoes(TokenTipo fim = TokenTipo.Fim)
    {
        var instrucoes = new List<Instrucao>();

        while(TentarConsumirToken(fim) == null)
        {
            instrucoes.Add(ParseInstrucao());
        }

        return instrucoes.ToArray();
    }

    private Instrucao? ParseInstrucao()
    {
        switch(Atual().Tipo)
        {
            case TokenTipo.Var: return ParseInstrucaoVar();
            case TokenTipo.Const: return ParseInstrucaoVar();
            case TokenTipo.Funcao: return ParseInstrucaoFuncao();
            case TokenTipo.Se: return ParseInstrucaoSe();
            case TokenTipo.Enquanto: return ParseInstrucaoEnquanto();
            case TokenTipo.Romper: return new InstrucaoRomper();
            case TokenTipo.Retornar: ConsumirToken(); return new InstrucaoRetornar(ParseExpressao());
            case TokenTipo.Identificador:
                if(Proximo(1).Tipo == TokenTipo.AbrirParen)
                    return new InstrucaoChamadaFuncao(ParseExpressaoChamadaFuncao());
                else
                    return ParseInstrucaoVar();
        }

        new Erro($"Instrução inválida: {Atual().Tipo.ToString()}", _linha, 1000).LancarErro();

        return null;
    }

    // Declarando uma variável
    private InstrucaoVar? ParseInstrucaoVar()
    {
        bool constante = TentarConsumirToken(TokenTipo.Const) != null;
        bool declaracao = TentarConsumirToken(TokenTipo.Var) != null;
        string identificador = (string)ConsumirToken(TokenTipo.Identificador).Valor;
        
        // Modificando o valor de um indice num Vetor existente
        var indiceVetor = ParseVetor();
        if(indiceVetor != null)
        {
            ConsumirToken(TokenTipo.OperadorDefinir);

            var expressaoValor = ParseExpressao(); // valor que será armazenado no indice do vetor
            return new InstrucaoVar(identificador, expressaoValor, constante, declaracao, TokenTipo.TokenInvalido, indiceVetor);
        }

        // Declarando um novo Vetor:
        ConsumirToken(TokenTipo.OperadorDefinir);
        indiceVetor = ParseVetor();

        if(indiceVetor != null)
            return new InstrucaoVar(identificador, indiceVetor, constante, declaracao, TokenTipo.Vetor);

        // Variável normal (sem ser Vetor)
        var expressao = ParseExpressao();

        return new InstrucaoVar(identificador, expressao, constante, declaracao);
    }

    private Expressao? ParseVetor()
    {
        if(TentarConsumirToken(TokenTipo.AbrirCol) == null)
            return null;
        Expressao expr = ParseExpressao();
        TentarConsumirToken(TokenTipo.FecharCol);

        return expr;
    }

    private ExpressaoChamadaFuncao? ParseExpressaoChamadaFuncao()
    {
        string identificador = (string)ConsumirToken(TokenTipo.Identificador).Valor;

        ConsumirToken(TokenTipo.AbrirParen);

        var argumentos = new List<Expressao>();
        while(Atual().Tipo != TokenTipo.FecharParen)
        {
            var expr = ParseExpressao();
            argumentos.Add(expr);

            if(expr == null)
                new ErroEsperado("expressão", _linha).LancarErro();

            TentarConsumirToken(TokenTipo.Virgula);
        }

        ConsumirToken(TokenTipo.FecharParen);

        return new ExpressaoChamadaFuncao(identificador, argumentos);
    }

    private InstrucaoFuncao? ParseInstrucaoFuncao()
    {
        ConsumirToken(TokenTipo.Funcao);
        
        string? identificador = (string)ConsumirToken(TokenTipo.Identificador)?.Valor;

        ConsumirToken(TokenTipo.AbrirParen);

        var parametros = new List<string>();

        while(Atual().Tipo != TokenTipo.FecharParen)
        {
            if(Atual().Tipo != TokenTipo.Identificador)
                ConsumirToken(TokenTipo.FecharParen); // tentar fechar paren (vai dar erro da msm forma)
            
            var param = (string)ConsumirToken()?.Valor;
            parametros.Add(param);

            TentarConsumirToken(TokenTipo.Virgula);
        }

        ConsumirToken(TokenTipo.FecharParen);

        var instrucoes = ParseInstrucoes();

        return new InstrucaoFuncao(identificador, instrucoes, parametros);
    }
    
    private InstrucaoSe? ParseInstrucaoSe()
    {
        ConsumirToken(TokenTipo.Se);

        var expressao = ParseExpressao();

        ConsumirToken(TokenTipo.Entao);

        var instrucoes = ParseInstrucoes().ToArray();
        
        var instrucoesSenao = new List<Instrucao>().ToArray();
        if(TentarConsumirToken(TokenTipo.Senao) != null)
            instrucoesSenao = ParseInstrucoes().ToArray();

        return new InstrucaoSe(expressao, instrucoes, instrucoesSenao);
    }

    private InstrucaoEnquanto? ParseInstrucaoEnquanto()
    {
        ConsumirToken(TokenTipo.Enquanto);

        var expressao = ParseExpressao();
        ConsumirToken(TokenTipo.Faca);

        var instrucoes = ParseInstrucoes();

        return new InstrucaoEnquanto(expressao, instrucoes);
    }

    private Expressao? ParseExpressao()
    {
        switch(Atual().Tipo)
        {
            case TokenTipo.NumeroLiteral:
            case TokenTipo.CaractereLiteral:
            case TokenTipo.TextoLiteral:
                if(TokenEhOperador(Proximo(1)))
                    return ParseExpressaoBinaria(); // Expressão Binária
                return new ExpressaoUnaria(ConsumirToken()); // Expressao Unária

            case TokenTipo.Identificador:
                if(Proximo(1).Tipo == TokenTipo.AbrirParen)
                {
                    var chamada = ParseExpressaoChamadaFuncao();

                    if(TokenEhOperador(Atual()))
                        return ParseExpressaoBinaria(new ExpressaoUnaria(chamada));
                    
                    return new ExpressaoUnaria(chamada);
                }

                if(TokenEhOperador(Proximo(1)))
                    return ParseExpressaoBinaria();
                
                if(Proximo(1).Tipo == TokenTipo.AbrirCol) // Acessando um Vetor -> identificador[expressao]
                {
                    var ident = ConsumirToken(TokenTipo.Identificador);
                    return new ExpressaoUnaria(new ExpressaoAcessarVetor(ident.Valor.ToString(), ParseVetor()));
                }
                
               return new ExpressaoUnaria(ConsumirToken()); // Expressão Unária (identificador)
        }
        
        new Erro("Impossível determinar expressão", _linha, 1000).LancarErro();
        return null;
    }

    private ExpressaoBinaria? ParseExpressaoBinaria(Expressao esquerda = null)
    {
        Token? operador = null;
        Expressao? direita = null;

        if(esquerda == null && Atual().Tipo == TokenTipo.NumeroLiteral 
            || Atual().Tipo == TokenTipo.Identificador || Atual().Tipo == TokenTipo.TextoLiteral)
                esquerda = new ExpressaoUnaria(ConsumirToken());

        if(TokenEhOperador(Atual()))
        {
            operador = ConsumirToken();
            direita = ParseExpressao();
        }

        if(esquerda == null || direita == null)
            new Erro("Impossível determinar expressão", _linha).LancarErro();
        
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
            new ErroEsperado(Token.TipoParaString(tipo)).LancarErro();
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