using Libra.Arvore;

namespace Libra;

public class r
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

        return new Programa(Instrucoes(TokenTipo.FimDoArquivo));
    }

    public Instrucao[] Instrucoes(TokenTipo fim = TokenTipo.Fim)
    {
        // Inicia uma lista com um pouco de espaço alocado
        var instrucoes = new List<Instrucao>(_tokens.Length / 3);

        while(!TentarConsumirToken(fim))
        {
            if(TentarConsumirToken(TokenTipo.FimDoArquivo))
                break;

            instrucoes.Add(InstrucaoAtual());
        }
        
        return instrucoes.ToArray();
    }

    private Instrucao? InstrucaoAtual()
    {   
        var atual = Atual();
        _local = atual.Local;

        switch (atual.Tipo)
        {
            case TokenTipo.Var: return DeclVar();
            case TokenTipo.Const: return DeclVar();
            case TokenTipo.Funcao: return DeclFuncao();
            case TokenTipo.Classe: return DeclClasse();
            case TokenTipo.Se: return InstrucaoSe();
            case TokenTipo.Enquanto: return InstrucaoEnquanto();
            case TokenTipo.Para: return InstrucaoParaCada();
            case TokenTipo.Romper: Passar(); return new InstrucaoRomper(_local);
            case TokenTipo.Continuar: Passar(); return new InstrucaoContinuar(_local);
            case TokenTipo.Retornar: Passar(); return new InstrucaoRetornar(_local, Expressao());
            case TokenTipo.Tentar: return InstrucaoTentar();
            case TokenTipo.Identificador:
                if (Proximo(1).Tipo == TokenTipo.AbrirParen)
                    return ChamadaFuncao();
                else if (Proximo(1).Tipo == TokenTipo.AbrirCol)
                    return AtribIndice();
                else if (Proximo(1).Tipo == TokenTipo.OperadorDefinir)
                    return AtribVar();
                break;
        }

        var expr = Expressao();

        if (expr != null)
        {
            if (expr.TipoExpr == TipoExpressao.ExpressaoPropriedade &&
                Atual().Tipo == TokenTipo.OperadorDefinir)
            {
                ConsumirToken(TokenTipo.OperadorDefinir);

                return AtribProp((ExpressaoPropriedade)expr);
            }

            return new InstrucaoExpressao(_local, expr);
        }
            

        throw new Erro($"Instrução inválida {atual}", _local);
    }

    private Instrucao? AtribProp(ExpressaoPropriedade alvo)
    {
        var expr = Expressao();

        return new AtribuicaoPropriedade(_local, alvo, expr);
    }
    private Instrucao? InstrucaoTentar()
    {
        ConsumirToken(TokenTipo.Tentar);

        Instrucao[] blocoTentar = Instrucoes(TokenTipo.Capturar);

        string variavelErro = ConsumirToken(TokenTipo.Identificador).Valor.ToString();

        Instrucao[] blocoCapturar = Instrucoes();

        return new InstrucaoTentar(_local, blocoTentar, variavelErro, blocoCapturar);
    }

    private Instrucao? AtribVar()
    {
        string identificador = ConsumirToken(TokenTipo.Identificador).Valor.ToString();
        ConsumirToken(TokenTipo.OperadorDefinir);
        var expr = Expressao();

        return new AtribuicaoVar(_local, identificador, expr);
    }

    private Instrucao? AtribIndice()
    {
        string ident = ConsumirToken(TokenTipo.Identificador).Valor.ToString();
        ConsumirToken(TokenTipo.AbrirCol);
        var exprIndice = Expressao();
        ConsumirToken(TokenTipo.FecharCol);
        ConsumirToken(TokenTipo.OperadorDefinir);
        var expressao = Expressao();
        
        return new AtribuicaoIndice(_local, ident, exprIndice, expressao);
    }

    private Instrucao? DeclVar()
    {
        bool constante = false;
        if(TentarConsumirToken(TokenTipo.Const))
            constante = true;
        else
            ConsumirToken(TokenTipo.Var);

        string identificador = ConsumirToken(TokenTipo.Identificador).Valor.ToString();

        string tipo = Interpretador.Flags.ForcarTiposEstaticos ? TiposPadrao.Indefinido : TiposPadrao.Objeto;

        if(TentarConsumirToken(TokenTipo.DoisPontos))
        {
            tipo = ConsumirToken(TokenTipo.Identificador).Valor.ToString();

            // "var n: T" (Declara uma variável de tipo T nula)
            if(Atual().Tipo != TokenTipo.OperadorDefinir)
            {
                if(constante) // Constante precisa de um valor (não pode ser atribuído depois)
                    throw new ErroEsperado(TokenTipo.OperadorDefinir, Atual().Tipo, _local);

                return new DeclaracaoVar(_local, identificador, null, tipo, false);
            }
        }

        ConsumirToken(TokenTipo.OperadorDefinir);
        
        var expressao = Expressao();

        return new DeclaracaoVar(_local, identificador, expressao, tipo, constante);
    }

    private ExpressaoNovoVetor? Vetor()
    {
        ConsumirToken(TokenTipo.AbrirCol);

        // Converte [ ] para [0] automaticamente
        if(TentarConsumirToken(TokenTipo.FecharCol))
        {
            return new ExpressaoNovoVetor(_local, ExpressaoLiteral.CriarInt(_local, 0));
        }

        Expressao expr = Expressao();
        ConsumirToken(TokenTipo.FecharCol);
        return new ExpressaoNovoVetor(_local, expr);
    }

    private DefinicaoTipo? DeclClasse()
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
            var atual = InstrucaoAtual();
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

    private Parametro[] Parametros()
    {
        var parametros = new List<Parametro>();
        while(Atual().Tipo != TokenTipo.FecharParen)
        {
            if(Atual().Tipo != TokenTipo.Identificador)
                ConsumirToken(TokenTipo.FecharParen); // tentar fechar paren (vai dar erro da msm forma)
            
            var ident = (string)ConsumirToken()?.Valor;
            string tipo = TiposPadrao.Objeto;
            
            if (TentarConsumirToken(TokenTipo.DoisPontos))
            {
                tipo = ConsumirToken(TokenTipo.Identificador).Valor.ToString();
            }
            else
            {
                if (Interpretador.Flags.ForcarTiposEstaticos)
                    throw new Erro("Obrigatório especificar tipo quando a flag --estrito estiver marcada.", _local);
            }
            parametros.Add(new Parametro(ident, tipo));

            TentarConsumirToken(TokenTipo.Virgula);
        }

        ConsumirToken(TokenTipo.FecharParen);

        return parametros.ToArray();
    }
    private DefinicaoFuncao? DeclFuncao()
    {
        ConsumirToken(TokenTipo.Funcao);
        string identificador = ConsumirToken(TokenTipo.Identificador).Valor.ToString();
        ConsumirToken(TokenTipo.AbrirParen);
        var parametros = Parametros();

        // Chamem um psiquiatra pra esse cidadão
        if(parametros.Length > 255)
            throw new Erro($"Função {identificador} passou de 255 parâmetros", _local, 255, "Procure ajuda.");

        string tipoRetorno = TiposPadrao.Objeto;
        if(TentarConsumirToken(TokenTipo.DoisPontos))
        {
            tipoRetorno = ConsumirToken(TokenTipo.Identificador).Valor.ToString();
        }
        else
        {
            // Quando tipos estáticos são forçados, se não especificar o tipo de retorno, ele será interpretado como nulo.
            // Em casos normais, o tipo de retorno poderá ser qualquer objeto
            tipoRetorno = Interpretador.Flags.ForcarTiposEstaticos ? TiposPadrao.Nulo : TiposPadrao.Objeto;
        }

        var instrucoes = Instrucoes();

        return new DefinicaoFuncao(_local, identificador, instrucoes, parametros, tipoRetorno);
    }
    
    private InstrucaoSe? InstrucaoSe()
    {
        ConsumirToken(TokenTipo.Se);

        var expressao = Expressao();

        ConsumirToken(TokenTipo.Entao);

        Instrucao[] corpoSe = CorpoSe();
        List<InstrucaoSenaoSe> listaSenaoSe = new();

        while(Atual().Tipo == TokenTipo.SenaoSe || Atual().Tipo == TokenTipo.Senao)
        {
            listaSenaoSe.Add(InstrucaoSenaoSe());
        }

        return new InstrucaoSe(_local, expressao, corpoSe, listaSenaoSe.Count > 0 ? listaSenaoSe.ToArray() : null);
    }

    private InstrucaoSenaoSe? InstrucaoSenaoSe()
    {
        
        // Senao será convertido para um "senao se 1", que é uma expressão sempre verdadeira
        if(TentarConsumirToken(TokenTipo.Senao))
        {
            return new InstrucaoSenaoSe(_local, ExpressaoLiteral.CriarInt(_local, 1), CorpoSe());
        }

        while(TentarConsumirToken(TokenTipo.SenaoSe))
        {
            var expr = Expressao();
            ConsumirToken(TokenTipo.Entao);
            List<Instrucao> corpo = new();

            return new InstrucaoSenaoSe(_local, expr, CorpoSe());
        }

        return null;
    }

    private Instrucao[] CorpoSe()
    {
        List<Instrucao> corpoSe = new();
        while (!TentarConsumirToken(TokenTipo.Fim) && 
        Atual().Tipo != TokenTipo.Senao &&
        Atual().Tipo != TokenTipo.SenaoSe)
        {
            corpoSe.Add(InstrucaoAtual());
        }
        return corpoSe.ToArray();
    }

    private InstrucaoEnquanto? InstrucaoEnquanto()
    {
        ConsumirToken(TokenTipo.Enquanto);

        var expressao = Expressao();
        ConsumirToken(TokenTipo.Repetir);

        var instrucoes = Instrucoes();

        return new InstrucaoEnquanto(_local, expressao, instrucoes);
    }
    
    private InstrucaoParaCada? InstrucaoParaCada()
    {
        ConsumirToken(TokenTipo.Para);
        ConsumirToken(TokenTipo.Cada);
        var identificador = ConsumirToken(TokenTipo.Identificador);
        ConsumirToken(TokenTipo.Em);
        var vetor = Expressao(); // TODO: Conferir se a expressão é enumerável
        var instrucoes = Instrucoes(); // até encontrar "fim"

        return new InstrucaoParaCada(_local, identificador, vetor, instrucoes);
    }

    private Expressao Expressao(int precedenciaMinima = 0)
    {
        var expr_esq = Primaria();

        while (Atual().Tipo == TokenTipo.Ponto)
        {
            ConsumirToken();

            var tokenIdent = ConsumirToken(TokenTipo.Identificador);

            expr_esq = new ExpressaoPropriedade(_local, expr_esq, tokenIdent.Valor.ToString());
        }

        while (true)
        {
            if (Atual() == null || PrioridadeOperador(Atual()) == null ||
                PrioridadeOperador(Atual()) < precedenciaMinima)
                break;

            var opr = ConsumirToken();
            int proxPrecedenciaMinima = precedenciaMinima + 1; // ESQ -> DIR
            var expr_dir = Expressao(proxPrecedenciaMinima);

            // Detectando problemas em tempo de compilação
            if (opr.Tipo == TokenTipo.OperadorDiv && expr_dir is ExpressaoLiteral exprLit)
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
    
    private Expressao Primaria()
    {
        switch (Atual().Tipo)
        {
            case TokenTipo.OperadorNeg:
            case TokenTipo.OperadorSub:
                return new ExpressaoUnaria(_local, ConsumirToken(), Expressao());
            case TokenTipo.AbrirCol:
                return Vetor();
            case TokenTipo.AbrirChave:
                return InicializacaoVetor();
            case TokenTipo.NumeroLiteral:
            case TokenTipo.CaractereLiteral:
            case TokenTipo.TextoLiteral:
            case TokenTipo.Nulo:
                return new ExpressaoLiteral(_local, ConsumirToken());
            case TokenTipo.Identificador:
                if (Proximo(1).Tipo == TokenTipo.AbrirParen)
                {
                    return ChamadaFuncao();
                }
                else if(Proximo(1).Tipo == TokenTipo.AbrirCol)
                {
                    return ExpressaoAcessoVetor();
                }
                return new ExpressaoVariavel(_local, ConsumirToken());

            case TokenTipo.AbrirParen:
                Passar();
                var exprDentroParenteses = Expressao();
                ConsumirToken(TokenTipo.FecharParen);
                return exprDentroParenteses;
        }

        throw new Erro($" Não foi possível parsear a expressão: {Atual().Tipo}", _local);
    }

    private ExpressaoInicializacaoVetor InicializacaoVetor()
    {
        ConsumirToken(TokenTipo.AbrirChave);
        if (TentarConsumirToken(TokenTipo.FecharChave))
        {
            return new ExpressaoInicializacaoVetor(_local, new List<Expressao>());
        }

        var expressoes = new List<Expressao>();
        while (true)
        {
            expressoes.Add(Expressao());

            TentarConsumirToken(TokenTipo.Virgula);

            if (TentarConsumirToken(TokenTipo.FecharChave))
                break;
        }

        return new ExpressaoInicializacaoVetor(_local, expressoes);
    }

    private Expressao[] Argumentos()
    {
        var argumentos = new List<Expressao>();
        while(Atual().Tipo != TokenTipo.FecharParen)
        {
            var expr = Expressao();
            argumentos.Add(expr);

            if(expr == null)
                ConsumirToken(TokenTipo.FecharParen);

            TentarConsumirToken(TokenTipo.Virgula);
        }
        return argumentos.ToArray();
    }
    private Expressao? ExpressaoAcessoVetor()
    {
        string identificador = (string)ConsumirToken(TokenTipo.Identificador).Valor;
        ConsumirToken(TokenTipo.AbrirCol);
        var indice = Expressao();
        ConsumirToken(TokenTipo.FecharCol);

        return new ExpressaoAcessoVetor(_local, identificador, indice);
    }

    private ChamadaFuncao? ChamadaFuncao()
    {
        string identificador = (string)ConsumirToken(TokenTipo.Identificador).Valor;
        ConsumirToken(TokenTipo.AbrirParen);

        var argumentos = Argumentos();
        
        if(argumentos.Length > 255)
            throw new Erro($"Função {identificador} passou de 255 argumentos", _local, 255, "Procure ajuda.");

        ConsumirToken(TokenTipo.FecharParen);
        return new ChamadaFuncao(_local, identificador, argumentos);
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