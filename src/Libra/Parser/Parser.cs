using Libra.Arvore;

namespace Libra;

public class Parser
{
    private Token[] _tokens;
    private int _posicao;
    private LocalToken _local;

    private static readonly Dictionary<TokenTipo, int> _precedenciaOperadores = new()
    {
        { TokenTipo.Ponto, 999 }, // sempre deve ser o primeiro (a.b)
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

        while(!TentarConsumirToken(fim))
        {
            if(TentarConsumirToken(TokenTipo.FimDoArquivo))
                break;

            instrucoes.Add(ParseInstrucaoAtual());
        }
        
        return instrucoes.ToArray();
    }

    private Instrucao? ParseInstrucaoAtual()
    {   
        var atual = Atual();
        _local = atual.Local;

        switch (atual.Tipo)
        {
            case TokenTipo.Var: return ParseDeclVar();
            case TokenTipo.Const: return ParseDeclVar();
            case TokenTipo.Funcao: return ParseDeclFuncao();
            case TokenTipo.Classe: return ParseDeclClasse();
            case TokenTipo.Se: return ParseInstrucaoSe();
            case TokenTipo.Enquanto: return ParseInstrucaoEnquanto();
            case TokenTipo.Romper: Passar(); return new InstrucaoRomper(_local);
            case TokenTipo.Continuar: Passar(); return new InstrucaoContinuar(_local);
            case TokenTipo.Retornar: Passar(); return new InstrucaoRetornar(_local, ParseExpressao());
            case TokenTipo.Identificador:
                if (Proximo(1).Tipo == TokenTipo.AbrirParen)
                    return ParseExpressaoChamadaFuncao();
                else if (Proximo(1).Tipo == TokenTipo.AbrirCol)
                    return ParseAtribIndice();
                else if (Proximo(1).Tipo == TokenTipo.OperadorDefinir)
                    return ParseAtribVar();
                break;
        }

        var expr = ParseExpressao();

        if (expr != null)
            return new InstrucaoExpressao(_local, expr);

        throw new Erro($"Instrução inválida {atual}", _local);
    }

    private Instrucao? ParseAtribProp()
    {
        string ident = ConsumirToken().Valor.ToString();
        Passar(); // .
        string prop = ConsumirToken(TokenTipo.Identificador).Valor.ToString();

        // Não é uma atribuição de propriedade, mas sim chamada de método
        if(TentarConsumirToken(TokenTipo.AbrirParen))
        {
            var args = ParseArgumentos();
            ConsumirToken(TokenTipo.FecharParen);
            return new ExpressaoChamadaMetodo(_local, ident, new ExpressaoChamadaFuncao(_local, prop, args));
        }

        ConsumirToken(TokenTipo.OperadorDefinir);
        var expr = ParseExpressao();

        return new AtribuicaoPropriedade(_local, ident, prop, expr);
    }

    private Instrucao? ParseAtribVar()
    {
        string identificador = ConsumirToken(TokenTipo.Identificador).Valor.ToString();
        ConsumirToken(TokenTipo.OperadorDefinir);
        var expr = ParseExpressao();

        return new AtribuicaoVar(_local, identificador, expr);
    }

    private Instrucao? ParseAtribIndice()
    {
        string ident = ConsumirToken(TokenTipo.Identificador).Valor.ToString();
        ConsumirToken(TokenTipo.AbrirCol);
        var exprIndice = ParseExpressao();
        ConsumirToken(TokenTipo.FecharCol);
        ConsumirToken(TokenTipo.OperadorDefinir);
        var expressao = ParseExpressao();
        
        return new AtribuicaoIndice(_local, ident, exprIndice, expressao);
    }

    private Instrucao? ParseDeclVar()
    {
        bool constante = false;
        if(TentarConsumirToken(TokenTipo.Const))
            constante = true;
        else
            ConsumirToken(TokenTipo.Var);
        
        string identificador = ConsumirToken(TokenTipo.Identificador).Valor.ToString();

        bool tipoModificavel = false;
        string tipo = "Objeto";

        if(TentarConsumirToken(TokenTipo.DoisPontos))
        {
            string tipoStr = ConsumirToken(TokenTipo.Identificador).Valor.ToString();
            tipoModificavel = tipo == "Objeto";

            // "var n: T" (Declara uma variável de tipo T nula)
            if(Atual().Tipo != TokenTipo.OperadorDefinir)
            {
                if(constante) // Constante precisa de um valor (não pode ser atribuído depois)
                    throw new ErroEsperado(TokenTipo.OperadorDefinir, Atual().Tipo, _local);

                return new DeclaracaoVar(_local, identificador, null, tipo, tipoModificavel, false);
            }
        }

        ConsumirToken(TokenTipo.OperadorDefinir);
        
        var expressao = ParseExpressao();

        if(Interpretador.Flags.ForcarTiposEstaticos && tipo == "Objeto")
            throw new Erro("Obrigatório especificar tipo quando a flag --rigido estiver marcada.", _local);

        return new DeclaracaoVar(_local, identificador, expressao, tipo, tipoModificavel, constante);
    }

    private ExpressaoNovoVetor? ParseVetor()
    {
        ConsumirToken(TokenTipo.AbrirCol);

        // Converte [ ] para [0] automaticamente
        if(TentarConsumirToken(TokenTipo.FecharCol))
        {
            return new ExpressaoNovoVetor(_local, ExpressaoLiteral.CriarInt(_local, 0));
        }

        Expressao expr = ParseExpressao();
        ConsumirToken(TokenTipo.FecharCol);
        return new ExpressaoNovoVetor(_local, expr);
    }

    private DefinicaoTipo? ParseDeclClasse()
    {
        ConsumirToken(TokenTipo.Classe);
        bool estatica = TentarConsumirToken(TokenTipo.Anotacao);
        string? identificador = (string)ConsumirToken(TokenTipo.Identificador)?.Valor;

        var variaveis = new List<DeclaracaoVar>();
        var funcoes = new List<DefinicaoFuncao>();

        while(Atual().Tipo != TokenTipo.Fim)
        {
            if(Atual().Tipo == TokenTipo.FimDoArquivo)
                throw new ErroEsperado(TokenTipo.Fim, TokenTipo.FimDoArquivo, _local);

            TentarConsumirToken(TokenTipo.Anotacao);
            var atual = ParseInstrucaoAtual();
            if(atual.Tipo == TipoInstrucao.DeclVar)
                variaveis.Add((DeclaracaoVar)atual);
            else if(atual.Tipo == TipoInstrucao.DeclFunc)
                funcoes.Add((DefinicaoFuncao)atual);
            else
                throw new Erro("Instruções esperadas: Declaração de Variável e Definição de Função.", _local);
        }
        Passar();

        return new DefinicaoTipo(_local, identificador, variaveis.ToArray(), funcoes.ToArray());
    }

    private Parametro[] ParseParametros()
    {
        var parametros = new List<Parametro>();
        while(Atual().Tipo != TokenTipo.FecharParen)
        {
            if(Atual().Tipo != TokenTipo.Identificador)
                ConsumirToken(TokenTipo.FecharParen); // tentar fechar paren (vai dar erro da msm forma)
            
            var ident = (string)ConsumirToken()?.Valor;
            string tipo = "Objeto";
            if(TentarConsumirToken(TokenTipo.DoisPontos))
            {
                tipo = ConsumirToken(TokenTipo.Identificador).Valor.ToString();
            }
            else
            {
                if(Interpretador.Flags.ForcarTiposEstaticos)
                    throw new Erro("Obrigatório especificar tipo quando a flag --rigido estiver marcada.", _local);
            }
            parametros.Add(new Parametro(ident, tipo));

            TentarConsumirToken(TokenTipo.Virgula);
        }

        ConsumirToken(TokenTipo.FecharParen);

        return parametros.ToArray();
    }
    private DefinicaoFuncao? ParseDeclFuncao()
    {
        ConsumirToken(TokenTipo.Funcao);
        string identificador = ConsumirToken(TokenTipo.Identificador).Valor.ToString();
        ConsumirToken(TokenTipo.AbrirParen);
        var parametros = ParseParametros();

        // Chamem um psiquiatra pra esse cidadão
        if(parametros.Length > 255)
            throw new Erro($"Função {identificador} passou de 255 parâmetros", _local, 255, "Procure ajuda.");

        string tipoRetorno = "Objeto";
        if(TentarConsumirSeta())
        {
            tipoRetorno = ConsumirToken(TokenTipo.Identificador).Valor.ToString();
        }
        else
        {
            // Quando tipos estáticos são forçados, se não especificar o tipo de retorno, ele será interpretado como nulo.
            // Em casos normais, o tipo de retorno poderá ser qualquer objeto
            tipoRetorno = Interpretador.Flags.ForcarTiposEstaticos ? "Nulo" : "Objeto";
        }

        var instrucoes = ParseInstrucoes();

        return new DefinicaoFuncao(_local, identificador, instrucoes, parametros, tipoRetorno);
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
        if(TentarConsumirToken(TokenTipo.Senao))
        {
            return new InstrucaoSenaoSe(_local, ExpressaoLiteral.CriarInt(_local, 1), ParseCorpoSe());
        }

        while(TentarConsumirToken(TokenTipo.SenaoSe))
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
        while (!TentarConsumirToken(TokenTipo.Fim) && 
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
            
            expr_esq = new ExpressaoBinaria(_local, expr_esq, opr, expr_dir);
        }

        return expr_esq;
    }

    private ExpressaoInicializacaoVetor ParseInicializacaoVetor()
    {
        ConsumirToken(TokenTipo.AbrirChave);
        if(TentarConsumirToken(TokenTipo.FecharChave))
        {
            return new ExpressaoInicializacaoVetor(_local, new List<Expressao>());
        }
        
        var expressoes = new List<Expressao>();
        while(true)
        {
            expressoes.Add(ParseExpressao());

            TentarConsumirToken(TokenTipo.Virgula);

            if(TentarConsumirToken(TokenTipo.FecharChave))
                break;
        }

        return new ExpressaoInicializacaoVetor(_local, expressoes);
    }

    // Identificador que possui ponto, ex: pessoa.nome
    private Token ParseIdentificadorComposto()
    {
        var final = ConsumirToken(TokenTipo.Identificador).Valor.ToString() + ".";
        ConsumirToken(TokenTipo.Ponto);
        final += ConsumirToken(TokenTipo.Identificador).Valor;
        
        return new Token(TokenTipo.Identificador, _local, final);
    }

    private Expressao ParseExpressaoTermo()
    {
        switch (Atual().Tipo)
        {
            case TokenTipo.OperadorNeg:
            case TokenTipo.OperadorSub:
                return new ExpressaoUnaria(_local, ConsumirToken(), ParseExpressao());
            case TokenTipo.AbrirCol:
                return ParseVetor();
            case TokenTipo.AbrirChave:
                return ParseInicializacaoVetor();
            case TokenTipo.NumeroLiteral:
            case TokenTipo.CaractereLiteral:
            case TokenTipo.TextoLiteral:
            case TokenTipo.Nulo:
                return new ExpressaoLiteral(_local, ConsumirToken());
            case TokenTipo.Identificador:
                if (Proximo(1).Tipo == TokenTipo.AbrirParen)
                {
                    return ParseExpressaoChamadaFuncao();
                }
                else if(Proximo(1).Tipo == TokenTipo.AbrirCol)
                {
                    return ParseExpressaoAcessoVetor();
                }
                /*else if(Proximo(1).Tipo == TokenTipo.Ponto)
                {
                    var ident = ConsumirToken().Valor.ToString();
                    Passar();
                    var prop = ConsumirToken(TokenTipo.Identificador).Valor.ToString();
                    if(TentarConsumirToken(TokenTipo.AbrirParen))
                    {
                        var args = ParseArgumentos();
                        ConsumirToken(TokenTipo.FecharParen);
                        return new ExpressaoChamadaMetodo(_local, ident, new ExpressaoChamadaFuncao(_local, prop, args));
                    }
                    return new ExpressaoPropriedade(_local, ident, prop);
                }*/
                return new ExpressaoVariavel(_local, ConsumirToken());

            case TokenTipo.AbrirParen:
                Passar();
                var exprDentroParenteses = ParseExpressao();
                ConsumirToken(TokenTipo.FecharParen);
                return exprDentroParenteses;
        }

        throw new ErroAcessoNulo($" Não foi possível parsear a expressão: {Atual().Tipo}", _local);
    }

    private Expressao[] ParseArgumentos()
    {
        var argumentos = new List<Expressao>();
        while(Atual().Tipo != TokenTipo.FecharParen)
        {
            var expr = ParseExpressao();
            argumentos.Add(expr);

            if(expr == null)
                ConsumirToken(TokenTipo.FecharParen);

            TentarConsumirToken(TokenTipo.Virgula);
        }
        return argumentos.ToArray();
    }
    private Expressao? ParseExpressaoAcessoVetor()
    {
        string identificador = (string)ConsumirToken(TokenTipo.Identificador).Valor;
        ConsumirToken(TokenTipo.AbrirCol);
        var indice = ParseExpressao();
        ConsumirToken(TokenTipo.FecharCol);

        return new ExpressaoAcessoVetor(_local, identificador, indice);
    }

    private ExpressaoChamadaFuncao? ParseExpressaoChamadaFuncao()
    {
        string identificador = (string)ConsumirToken(TokenTipo.Identificador).Valor;
        ConsumirToken(TokenTipo.AbrirParen);

        var argumentos = ParseArgumentos();
        
        if(argumentos.Length > 255)
            throw new Erro($"Função {identificador} passou de 255 argumentos", _local, 255, "Procure ajuda.");

        ConsumirToken(TokenTipo.FecharParen);
        return new ExpressaoChamadaFuncao(_local, identificador, argumentos);
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

    private bool TentarConsumirToken(TokenTipo tipo, bool seguro = false)
    {
        if(Atual().Tipo == TokenTipo.FimDoArquivo && seguro)
            throw new ErroTokenInvalido("Fim do Arquivo", _local);

        if(Atual().Tipo == tipo)
        {
            Passar();
            return true;
        }

        return false;
    }
}