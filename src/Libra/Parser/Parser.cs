using Libra.Runtime;    // TODO: remover
using Libra.Arvore;

namespace Libra;

public class Parser
{
    private Token[] _tokens;
    private int _posicao;
    private LocalFonte _local;
    public bool ModoEstrito { get; set; }

    private static readonly Dictionary<TokenTipo, int> _precedenciaOperadores = new()
    {
        { TokenTipo.OperadorPot, 6 },
        { TokenTipo.OperadorMult, 5 },
        { TokenTipo.OperadorDiv, 5 },
        { TokenTipo.OperadorResto, 5 },
        { TokenTipo.OperadorSoma, 4 },
        { TokenTipo.OperadorSub, 4 },
        { TokenTipo.OperadorMaiorQue, 3 },
        { TokenTipo.OperadorMaiorIgualQue, 3 },
        { TokenTipo.OperadorMenorQue, 3 },
        { TokenTipo.OperadorMenorIgualQue, 3 },
        { TokenTipo.OperadorComparacao, 2 },
        { TokenTipo.OperadorDiferente, 2 },
        { TokenTipo.OperadorE, 1 },
        { TokenTipo.OperadorOu, 0 }
    };

    public Parser(Token[] tokens, bool modoEstrito = false)
    {
        _posicao = 0;
        _local = new LocalFonte();
        _tokens = tokens;
        ModoEstrito = modoEstrito;
    }

    public Programa Parse()
    {
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
            instrucoes.Add(ParseInstrucao());
        }
        
        return instrucoes.ToArray();
    }

    private Instrucao? ParseInstrucao()
    {   
        var atual = Atual();
        _local = atual.Local;

        switch (atual.Tipo)
        {
            case TokenTipo.Var:
            case TokenTipo.Const: return DeclVar();
            case TokenTipo.Funcao: return DeclFuncao();
            case TokenTipo.Classe: return DeclClasse();
            case TokenTipo.Se: return Se();
            case TokenTipo.Enquanto: return Enquanto();
            case TokenTipo.Repetir: return Repetir();
            case TokenTipo.Para: return ParaCada();
            case TokenTipo.Romper: Passar(); return new Romper(_local);
            case TokenTipo.Continuar: Passar(); return new Continuar(_local);
            case TokenTipo.Retornar: Passar(); return new Retornar(_local, Expressao());
            case TokenTipo.Tentar: return Tentar();
            case TokenTipo.Importar: return Importar();
            case TokenTipo.Identificador:
                {
                    // Tenta detectar se é uma atribuição (var = ... ou obj.prop = ...)
                    int offset = 1;
                    while (Proximo(offset).Tipo == TokenTipo.Ponto && Proximo(offset + 1).Tipo == TokenTipo.Identificador)
                    {
                        offset += 2;
                    }

                    if (Proximo(offset).Tipo == TokenTipo.OperadorDefinir)
                    {
                        var exprAlvo = Expressao();
                        if (TentarConsumirToken(TokenTipo.OperadorDefinir))
                        {
                            var valor = Expressao();
                            if (exprAlvo is ExpressaoVariavel varExpr)
                                return new AtribuicaoVar(_local, varExpr.Identificador.Valor.ToString(), valor);
                            if (exprAlvo is ExpressaoPropriedade propExpr)
                                return new AtribuicaoPropriedade(_local, propExpr, valor);
                            
                            return new InstrucaoExpressao(_local, exprAlvo);
                        }
                    }
                    
                    if (Proximo(1).Tipo == TokenTipo.AbrirCol)
                    {
                        // Lookahead para ver se é atribuição de índice
                        int colOffset = 2;
                        int colNivel = 1;
                        while (colNivel > 0 && Proximo(colOffset).Tipo != TokenTipo.FimDoArquivo)
                        {
                            if (Proximo(colOffset).Tipo == TokenTipo.AbrirCol) colNivel++;
                            if (Proximo(colOffset).Tipo == TokenTipo.FecharCol) colNivel--;
                            colOffset++;
                        }
                        
                        if (Proximo(colOffset).Tipo == TokenTipo.OperadorDefinir)
                            return AtribIndice();
                    }
                }
                break;
        }

        // Se não é nenhuma instrução, então pode ser uma Expressão
        var expr = Expressao();

        return new InstrucaoExpressao(_local, expr);
    }

    private Instrucao Importar()
    {
        ConsumirToken(TokenTipo.Importar);
        string caminho;
        string identificador;

        if (Atual().Tipo == TokenTipo.TextoLiteral)
        {
            var tokenCaminho = ConsumirToken(TokenTipo.TextoLiteral);
            caminho = tokenCaminho.Valor.ToString();
            identificador = Path.GetFileNameWithoutExtension(caminho);
        }
        else
        {
            var partes = new List<string>();
            partes.Add(ConsumirToken(TokenTipo.Identificador).Valor.ToString());

            while (TentarConsumirToken(TokenTipo.Ponto))
            {
                partes.Add(ConsumirToken(TokenTipo.Identificador).Valor.ToString());
            }

            caminho = string.Join("/", partes) + ".libra";
            identificador = partes.Last();
        }

        // Sem apelido (`como`), os elementos do módulo também são injetados no
        // escopo atual (permite chamar `mostrarVetor()` em vez de `vetores.mostrarVetor()`).
        bool injetarNoEscopo = true;
        if (TentarConsumirToken(TokenTipo.Como))
        {
            identificador = ConsumirToken(TokenTipo.Identificador).Valor.ToString();
            injetarNoEscopo = false;
        }

        return new InstrucaoImportar(_local, caminho, identificador, injetarNoEscopo);
    }

    private Instrucao? Repetir()
    {
        ConsumirToken(TokenTipo.Repetir);
        Expressao verdadeira = ExpressaoLiteral.CriarInt(_local, 1);
        var corpo = Bloco();
        return new Enquanto(_local, verdadeira, corpo); // repetir é basicamente um "enquanto 1"
    }

    private Instrucao? AtribProp(ExpressaoPropriedade alvo)
    {
        var expr = Expressao();

        return new AtribuicaoPropriedade(_local, alvo, expr);
    }
    private Instrucao? Tentar()
    {
        ConsumirToken(TokenTipo.Tentar);

        Instrucao[] blocoTentar = Instrucoes(TokenTipo.Capturar);

        bool temParenteses = TentarConsumirToken(TokenTipo.AbrirParen);
        string variavelErro = ConsumirToken(TokenTipo.Identificador).Valor.ToString();
        if(temParenteses) ConsumirToken(TokenTipo.FecharParen);

        Instrucao[] blocoCapturar = Instrucoes();

        return new Tentar(_local, blocoTentar, variavelErro, blocoCapturar);
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

        // TODO: Arrumar!
        //string tipo = Interpretador.Flags.ForcarTiposEstaticos ? TiposPadrao.Indefinido : TiposPadrao.Objeto;
        string tipo = TiposPadrao.Indefinido;

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
            var atual = ParseInstrucao();
            if(atual is DeclaracaoVar)
                variaveis.Add((DeclaracaoVar)atual);
            else if(atual is DefinicaoFuncao)
                funcoes.Add((DefinicaoFuncao)atual);
            else
                throw new Erro("Instrução inválida dentro da classe. Apenas variáveis e funções são permitidas.", _local, 1004, "Remova instruções de controle de fluxo ou expressões soltas de dentro da definição da classe.");
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
                if (ModoEstrito)
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
        if (TentarConsumirToken(TokenTipo.DoisPontos))
        {
            tipoRetorno = ConsumirToken(TokenTipo.Identificador).Valor.ToString();
        }
        else
        {
            // Quando tipos estáticos são forçados, se não especificar o tipo de retorno, ele será interpretado como nulo.
            // Em casos normais, o tipo de retorno poderá ser qualquer objeto
            tipoRetorno = ModoEstrito ? TiposPadrao.Nulo : TiposPadrao.Objeto;
        }

        var instrucoes = Instrucoes();

        return new DefinicaoFuncao(_local, identificador, instrucoes, parametros, tipoRetorno);
    }
    
    private Se Se()
    {
        ConsumirToken(TokenTipo.Se);

        var expressao = Expressao();

        ConsumirToken(TokenTipo.Entao);

        var entao = Bloco();

        Instrucao? senao = null;

        if (TentarConsumirToken(TokenTipo.Senao))
        {
            if (Atual().Tipo == TokenTipo.Se)
            {
                senao = Se(); // else if
                return new Se(_local, expressao, entao, senao);
            }
            else
            {
                senao = Bloco();
            }
        }

        ConsumirToken(TokenTipo.Fim);

        return new Se(_local, expressao, entao, senao);
    }

    private Bloco? Bloco()
    {
        List<Instrucao> instrucoes = new();
        while(Atual().Tipo != TokenTipo.Fim && Atual().Tipo != TokenTipo.Senao)
        {
            if(Atual().Tipo == TokenTipo.FimDoArquivo)
                throw new ErroEsperado(TokenTipo.Fim, TokenTipo.FimDoArquivo, _local);

            instrucoes.Add(ParseInstrucao());
        }
        return new Bloco(_local, instrucoes.ToArray());
    }

    private Enquanto? Enquanto()
    {
        ConsumirToken(TokenTipo.Enquanto);

        var expressao = Expressao();
        ConsumirToken(TokenTipo.Repetir);

        var corpo = Bloco();

        ConsumirToken(TokenTipo.Fim);

        return new Enquanto(_local, expressao, corpo);
    }
    
    private ParaCada? ParaCada()
    {
        ConsumirToken(TokenTipo.Para);
        ConsumirToken(TokenTipo.Cada);
        var identificador = ConsumirToken(TokenTipo.Identificador);
        ConsumirToken(TokenTipo.Em);
        var vetor = Expressao(); // TODO: Conferir se a expressão é enumerável
        var instrucoes = Instrucoes(); // até encontrar "fim"

        return new ParaCada(_local, identificador, vetor, instrucoes);
    }

    private Expressao Expressao(int precedenciaMinima = 0)
    {
        var expr_esq = Primaria();

        while (true)
        {
            if (TentarConsumirToken(TokenTipo.Ponto))
            {
                var tokenIdent = ConsumirToken(TokenTipo.Identificador);
                expr_esq = new ExpressaoPropriedade(_local, expr_esq, tokenIdent.Valor.ToString());
            }
            else if (Atual().Tipo == TokenTipo.AbrirParen)
            {
                if (expr_esq is ExpressaoPropriedade prop)
                {
                    ConsumirToken(TokenTipo.AbrirParen);
                    var argumentos = Argumentos();
                    ConsumirToken(TokenTipo.FecharParen);
                    expr_esq = new ExpressaoChamadaMetodo(_local, prop.Alvo, new ExpressaoChamadaFuncao(_local, prop.Propriedade, argumentos));
                }
                else if (expr_esq is ExpressaoVariavel varExpr)
                {
                    ConsumirToken(TokenTipo.AbrirParen);
                    var argumentos = Argumentos();
                    ConsumirToken(TokenTipo.FecharParen);
                    expr_esq = new ExpressaoChamadaFuncao(_local, varExpr.Identificador.Valor.ToString(), argumentos);
                }
                else
                {
                    // TODO: Suporte para chamar qualquer expressão como função
                    break;
                }
            }
            else
            {
                break;
            }
        }

        while (true)
        {
            var prioridade = PrioridadeOperador(Atual());
            if (prioridade == null || prioridade < precedenciaMinima)
                break;

            var opr = ConsumirToken();
            var expr_dir = Expressao(prioridade.Value + 1);

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
                    return AcessoVetor();
                }
                return new ExpressaoVariavel(_local, ConsumirToken());

            case TokenTipo.AbrirParen:
                Passar();
                var exprDentroParenteses = Expressao();
                ConsumirToken(TokenTipo.FecharParen);
                return exprDentroParenteses;
        }

        throw new Erro($"Não foi possível processar este símbolo: {Token.TipoParaString(Atual().Tipo)}", _local, 1005, "Verifique se a expressão está escrita corretamente ou se falta algum operador.");
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
    private Expressao? AcessoVetor()
    {
        string identificador = (string)ConsumirToken(TokenTipo.Identificador).Valor;
        ConsumirToken(TokenTipo.AbrirCol);
        var indice = Expressao();
        ConsumirToken(TokenTipo.FecharCol);

        return new ExpressaoAcessoVetor(_local, identificador, indice);
    }

    private ExpressaoChamadaFuncao? ChamadaFuncao()
    {
        string identificador = (string)ConsumirToken(TokenTipo.Identificador).Valor;
        ConsumirToken(TokenTipo.AbrirParen);

        var argumentos = Argumentos();
        
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
