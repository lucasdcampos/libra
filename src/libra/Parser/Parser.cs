using Libra.Arvore;

namespace Libra;

public class Parser
{
    private List<Token> _tokens;
    private int _posicao;
    private LocalToken _local;

    private static readonly Dictionary<TokenTipo, int> _precedenciaOperadores = new()
    {
        { TokenTipo.OperadorPot, 4 },
        { TokenTipo.OperadorMult, 3 },
        { TokenTipo.OperadorDiv, 3 },
        { TokenTipo.OperadorSoma, 2 },
        { TokenTipo.OperadorSub, 2 },
        { TokenTipo.OperadorMaiorQue, 1 },
        { TokenTipo.OperadorMaiorIgualQue, 1 },
        { TokenTipo.OperadorMenorQue, 1 },
        { TokenTipo.OperadorMenorIgualQue, 1 },
        { TokenTipo.OperadorComparacao, 1 },
        { TokenTipo.OperadorDiferente, 1 },
        { TokenTipo.OperadorResto, 1},
        { TokenTipo.OperadorE, 0 },
        { TokenTipo.OperadorOu, 0 }
    };

    public void Resetar()
    {
        _posicao = 0;
        _local = new LocalToken();
    }

    public Programa Parse(List<Token> tokens, List<Token> extra = null)
    {
        Resetar();
        _tokens = tokens;

        if(extra != null)
            _tokens.AddRange(extra);

        return new Programa(ParseInstrucoes(TokenTipo.FimDoArquivo));
    }

    public Instrucao[] ParseInstrucoes(List<Token> tokens)
    {
        Resetar();

        _tokens = tokens;

        if(_tokens == null)
            return new Instrucao[0];

        return ParseInstrucoes(TokenTipo.FimDoArquivo);
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
            case TokenTipo.Retornar: ConsumirToken(); return new InstrucaoRetornar(_local, ParseExpressao());
            case TokenTipo.Identificador:
                if(Proximo(1).Tipo == TokenTipo.AbrirParen)
                    return new InstrucaoChamadaFuncao(_local, ParseExpressaoChamadaFuncao());
                else
                    return ParseInstrucaoVar();
        }

        var expr = new InstrucaoExibirExpressao(_local, ParseExpressao());
        
        if(expr.Expressao != null && expr.Expressao is not ExpressaoChamadaFuncao)
            return expr;
        
        return null;
    }

    private Instrucao? ParseInstrucaoVar()
    {
        bool constante = TentarConsumirToken(TokenTipo.Const) != null;
        bool declaracao = TentarConsumirToken(TokenTipo.Var) != null;
        var tokenIdentificador = ConsumirToken(TokenTipo.Identificador);
        string identificador = tokenIdentificador.Valor.ToString();

        bool modificacaoVetor = false;
        Expressao indiceExpr = null;
        
        if(TentarConsumirToken(TokenTipo.AbrirCol) != null)
        {
            indiceExpr = ParseExpressao();
            ConsumirToken(TokenTipo.FecharCol);
            modificacaoVetor = true;
        }

        if(TentarConsumirToken(TokenTipo.OperadorDefinir) == null)
        {
            return new InstrucaoExibirExpressao(_local, new ExpressaoVariavel(tokenIdentificador));
        }

        var expressao = ParseExpressao();
        
        if(modificacaoVetor) return new InstrucaoModificacaoVetor(_local, identificador, indiceExpr, expressao);

        return new InstrucaoVar(_local, identificador, expressao, constante, declaracao || constante);
    }

    private ExpressaoDeclaracaoVetor? ParseVetor()
    {
        ConsumirToken(TokenTipo.AbrirCol);

        // Converte [ ] para [0] automaticamente
        if(TentarConsumirToken(TokenTipo.FecharCol) != null)
        {
            return new ExpressaoDeclaracaoVetor(ExpressaoLiteral.CriarInt(_local, 0));
        }

        Expressao expr = ParseExpressao();
        ConsumirToken(TokenTipo.FecharCol);
        return new ExpressaoDeclaracaoVetor(expr);
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

        // Chamem um psiquiatra pra esse cidadão
        if(parametros.Count > 255)
            throw new Erro($"Função {identificador} passou de 255 parâmetros", _local);
            
        ConsumirToken(TokenTipo.FecharParen);

        var instrucoes = ParseInstrucoes();

        return new InstrucaoFuncao(_local, identificador, instrucoes, parametros);
    }
    
    private InstrucaoSe? ParseInstrucaoSe()
    {
        ConsumirToken(TokenTipo.Se);

        var expressao = ParseExpressao();

        ConsumirToken(TokenTipo.Entao);

        Instrucao[] corpoSe = ParseCorpoSe();
        List<InstrucaoSenaoSe> listaSenaoSe = new();

        while(Atual().Tipo == TokenTipo.SenaoSe || Atual().Tipo == TokenTipo.Senao)
        {
            listaSenaoSe.Add(ParseInstrucaoSenaoSe());
        }

        return new InstrucaoSe(_local, expressao, corpoSe, listaSenaoSe.Count > 0 ? listaSenaoSe.ToArray() : null);
    }

    private InstrucaoSenaoSe? ParseInstrucaoSenaoSe()
    {
        
        // Senao será convertido para um "senao se 1", que é uma expressão sempre verdadeira
        if(TentarConsumirToken(TokenTipo.Senao) != null)
        {
            return new InstrucaoSenaoSe(_local, ExpressaoLiteral.CriarInt(_local, 1), ParseCorpoSe());
        }

        while(TentarConsumirToken(TokenTipo.SenaoSe) != null)
        {
            var expr = ParseExpressao();
            ConsumirToken(TokenTipo.Entao);
            List<Instrucao> corpo = new();

            return new InstrucaoSenaoSe(_local, expr, ParseCorpoSe());
        }

        return null;
    }

    private Instrucao[] ParseCorpoSe()
    {
        List<Instrucao> corpoSe = new();
        while (TentarConsumirToken(TokenTipo.Fim) == null && 
        Atual().Tipo != TokenTipo.Senao &&
        Atual().Tipo != TokenTipo.SenaoSe)
        {
            corpoSe.Add(ParseInstrucao());
        }
        return corpoSe.ToArray();
    }

    private InstrucaoEnquanto? ParseInstrucaoEnquanto()
    {
        ConsumirToken(TokenTipo.Enquanto);

        var expressao = ParseExpressao();
        ConsumirToken(TokenTipo.Faca);

        var instrucoes = ParseInstrucoes();

        return new InstrucaoEnquanto(_local, expressao, instrucoes);
    }

    private Expressao ParseExpressao(int precedenciaMinima = 0)
    {
        var expr_esq = ParseExpressaoTermo();

        while (true)
        {
            if (Atual() == null || PrioridadeOperador(Atual()) == null ||
                PrioridadeOperador(Atual()) < precedenciaMinima)
                break;

            var opr = ConsumirToken();
            int proxPrecedenciaMinima = precedenciaMinima + 1; // ESQ -> DIR
            var expr_dir = ParseExpressao(proxPrecedenciaMinima);

            // Detectando problemas em tempo de compilação
            if(opr.Tipo == TokenTipo.OperadorDiv && expr_dir is ExpressaoLiteral exprLit)
            {
                if ((exprLit.Valor is int intValue && intValue == 0) || 
                    (exprLit.Valor is double doubleValue && doubleValue == 0.0))
                {
                    throw new ErroDivisaoPorZero(_local);
                }

            }
            
            expr_esq = new ExpressaoBinaria(expr_esq, opr, expr_dir);
        }

        return expr_esq;
    }

    private ExpressaoInicializacaoVetor ParseInicializacaoVetor()
    {
        ConsumirToken(TokenTipo.AbrirChave);
        if(TentarConsumirToken(TokenTipo.FecharChave) != null)
        {
            return new ExpressaoInicializacaoVetor(new List<Expressao>());
        }
        
        var expressoes = new List<Expressao>();
        while(true)
        {
            expressoes.Add(ParseExpressao());

            TentarConsumirToken(TokenTipo.Virgula);

            if(TentarConsumirToken(TokenTipo.FecharChave) != null)
                break;
        }

        return new ExpressaoInicializacaoVetor(expressoes);
    }

    private Expressao ParseExpressaoTermo()
    {
        switch (Atual().Tipo)
        {
            case TokenTipo.OperadorNeg:
            case TokenTipo.OperadorSub:
                return new ExpressaoUnaria(ConsumirToken(), ParseExpressao());
            case TokenTipo.AbrirCol:
                return ParseVetor();
            case TokenTipo.AbrirChave:
                return ParseInicializacaoVetor();
            case TokenTipo.NumeroLiteral:
            case TokenTipo.CaractereLiteral:
            case TokenTipo.TextoLiteral:
            case TokenTipo.Nulo:
                return new ExpressaoLiteral(ConsumirToken());
            case TokenTipo.Identificador:
                if (Proximo(1).Tipo == TokenTipo.AbrirParen)
                {
                    return ParseExpressaoChamadaFuncao();
                }
                else if(Proximo(1).Tipo == TokenTipo.AbrirCol)
                {
                    return ParseExpressaoAcessoVetor();
                }
                return new ExpressaoVariavel(ConsumirToken());

            case TokenTipo.AbrirParen:
                ConsumirToken();
                var exprDentroParenteses = ParseExpressao();
                ConsumirToken(TokenTipo.FecharParen);
                return exprDentroParenteses;
        }

        throw new ErroAcessoNulo($" Não foi possível parsear a expressão: {Atual().Tipo}", _local);
    }

    private Expressao? ParseExpressaoAcessoVetor()
    {
        string identificador = (string)ConsumirToken(TokenTipo.Identificador).Valor;
        ConsumirToken(TokenTipo.AbrirCol);
        var indice = ParseExpressao();
        ConsumirToken(TokenTipo.FecharCol);

        return new ExpressaoAcessoVetor(identificador, indice);
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
                ConsumirToken(TokenTipo.FecharParen);

            TentarConsumirToken(TokenTipo.Virgula);
        }

        ConsumirToken(TokenTipo.FecharParen);
        return new ExpressaoChamadaFuncao(identificador, argumentos);
    }

    private int? PrioridadeOperador(Token token)
    {
        // Retorna NULL se não for um operador
        return _precedenciaOperadores.TryGetValue(token.Tipo, out var prioridade) ? prioridade : null;
    }

    private Token Atual() 
    {
        return Proximo(0);
    }

    private Token Proximo(int offset)
    {
        if (_posicao + offset >= _tokens.Count)
        {
            return new Token(TokenTipo.FimDoArquivo, _local);
        }

        return _tokens[_posicao + offset];
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
            throw new ErroEsperado(tipo, Atual().Tipo, _local);
        }

        Passar();

        _local = token.Local;
        return token;
    }

    private Token? TentarConsumirToken(TokenTipo tipo, bool seguro = false)
    {
        if(Atual().Tipo == TokenTipo.FimDoArquivo && seguro)
            new ErroTokenInvalido("Fim do Arquivo", _local);
        if(Atual().Tipo == tipo)
        {
            return ConsumirToken();
        }

        return null;
    }
}