using Libra.Arvore;

namespace Libra;

public class Parser
{
    private Token[] _tokens;
    private int _posicao;
    private LocalToken _local;

    private static readonly Dictionary<TokenTipo, int> _precedenciaOperadores = new()
    {
        { TokenTipo.OperadorPot, 4 },
        { TokenTipo.OperadorMult, 3 },
        { TokenTipo.OperadorDiv, 3 },
        { TokenTipo.OperadorSoma, 2 },
        { TokenTipo.OperadorSub, 2 },
        { TokenTipo.OperadorResto, 1},
        { TokenTipo.OperadorComparacao, 0 },
        { TokenTipo.OperadorDiferente, 0 },
        { TokenTipo.OperadorMaiorQue, 0 },
        { TokenTipo.OperadorMaiorIgualQue, 0 },
        { TokenTipo.OperadorMenorQue, 0 },
        { TokenTipo.OperadorMenorIgualQue, 0 },
        { TokenTipo.OperadorE, 0 },
        { TokenTipo.OperadorOu, 0 }
    };

    public void Resetar()
    {
        _posicao = 0;
        _local = new LocalToken();
    }

    public Programa Parse(Token[] tokens, Token[] extra = null)
    {
        Resetar();
        _tokens = tokens;

        if(tokens == null)
            throw new Erro("Falha ao gerar Tokens");

        if (extra != null)
        {
            _tokens = new Token[tokens.Length + extra.Length];
            Array.Copy(tokens, _tokens, tokens.Length);
            Array.Copy(extra, 0, _tokens, tokens.Length, extra.Length);
        }

        return new Programa(ParseInstrucoes(TokenTipo.FimDoArquivo));
    }

    public Instrucao[] ParseInstrucoes(TokenTipo fim = TokenTipo.Fim)
    {
        // Inicia uma lista com um pouco de espaço alocado
        var instrucoes = new List<Instrucao>(_tokens.Length / 3);

        while(TentarConsumirToken(fim) == null)
        {
            if(TentarConsumirToken(TokenTipo.FimDoArquivo) != null)
                break;

            instrucoes.Add(ParseInstrucaoAtual());
        }
        
        return instrucoes.ToArray();
    }

    private Instrucao? ParseInstrucaoAtual()
    {   
        var atual = Atual();
        _local = atual.Local;

        switch(atual.Tipo)
        {
            case TokenTipo.Var: return ParseInstrucaoVar();
            case TokenTipo.Const: return ParseInstrucaoVar();
            case TokenTipo.Funcao: return ParseInstrucaoFuncao();
            case TokenTipo.Se: return ParseInstrucaoSe();
            case TokenTipo.Enquanto: return ParseInstrucaoEnquanto();
            case TokenTipo.Romper: Passar(); return new InstrucaoRomper(_local);
            case TokenTipo.Continuar: Passar(); return new InstrucaoContinuar(_local);
            case TokenTipo.Retornar: Passar(); return new InstrucaoRetornar(_local, ParseExpressao());
            case TokenTipo.Identificador:
                if(Proximo(1).Tipo == TokenTipo.AbrirParen)
                    return new InstrucaoChamadaFuncao(_local, ParseExpressaoChamadaFuncao());
                else
                    return ParseInstrucaoVar();
        }
            
        throw new Erro($"Instrução inválida {atual}", _local);
    }

    private Instrucao? ParseInstrucaoVar()
    {
        bool constante = TentarConsumirToken(TokenTipo.Const) != null;
        bool declaracao = TentarConsumirToken(TokenTipo.Var) != null;
        var tokenIdentificador = ConsumirToken(TokenTipo.Identificador);
        string identificador = tokenIdentificador.Valor.ToString();

        bool tipoModificavel = Atual().Tipo != TokenTipo.DoisPontos;
        LibraTipo tipo = LibraTipo.Objeto;
        if(!tipoModificavel)
        {
            Passar();
            string tipoStr = ConsumirToken(TokenTipo.Identificador).Valor.ToString();
            tipo = LibraUtil.PegarTipo(tipoStr);
        }

        bool modificacaoVetor = false;
        Expressao indiceExpr = null;
        
        if(TentarConsumirToken(TokenTipo.AbrirCol) != null)
        {
            indiceExpr = ParseExpressao();
            ConsumirToken(TokenTipo.FecharCol);
            modificacaoVetor = true;
        }

        ConsumirToken(TokenTipo.OperadorDefinir);
        
        var expressao = ParseExpressao();
        
        if(modificacaoVetor) return new InstrucaoModificacaoVetor(_local, identificador, indiceExpr, expressao);

        return new InstrucaoVar(_local, identificador, expressao, constante, declaracao || constante, tipo, tipoModificavel);
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

        var parametros = new List<Parametro>();

        while(Atual().Tipo != TokenTipo.FecharParen)
        {
            if(Atual().Tipo != TokenTipo.Identificador)
                ConsumirToken(TokenTipo.FecharParen); // tentar fechar paren (vai dar erro da msm forma)
            
            var ident = (string)ConsumirToken()?.Valor;
            LibraTipo tipo = LibraTipo.Objeto;
            if(TentarConsumirToken(TokenTipo.DoisPontos) != null)
            {
                tipo = LibraUtil.PegarTipo(ConsumirToken(TokenTipo.Identificador).Valor.ToString());
            }
            parametros.Add(new Parametro(ident, tipo));

            TentarConsumirToken(TokenTipo.Virgula);
        }

        // Chamem um psiquiatra pra esse cidadão
        if(parametros.Count > 255)
            throw new Erro($"Função {identificador} passou de 255 parâmetros", _local);
            
        ConsumirToken(TokenTipo.FecharParen);

        LibraTipo tipoRetorno = LibraTipo.Objeto;
        if(TentarConsumirSeta())
        {
            string tipo = ConsumirToken(TokenTipo.Identificador).Valor.ToString();
            tipoRetorno = LibraUtil.PegarTipo(tipo);
        }

        var instrucoes = ParseInstrucoes();

        return new InstrucaoFuncao(_local, identificador, instrucoes, parametros, tipoRetorno);
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
            corpoSe.Add(ParseInstrucaoAtual());
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
                Passar();
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

    private bool TentarConsumirSeta()
    {
        var seta = Atual().Tipo == TokenTipo.OperadorSub && Proximo(1).Tipo == TokenTipo.OperadorMaiorQue;
        if(seta) 
        {
            ConsumirToken();
            ConsumirToken();
        }
        return seta;
    }
    
    private Token Atual() 
    {
        return Proximo(0);
    }

    private Token Proximo(int offset)
    {
        if (_posicao + offset >= _tokens.Length)
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