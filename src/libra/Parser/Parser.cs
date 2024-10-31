using Libra.Arvore;

namespace Libra;

public class Parser
{
    private List<Token> _tokens;
    private int _posicao;
    private int _linha;

    public NodoPrograma Parse(List<Token> tokens, bool debugTokens = false)
    {
        _tokens = tokens;

        if(debugTokens)
            foreach(var t in tokens)
                Console.WriteLine(t.ToString());
        

        NodoPrograma? programa = null;

        var escopo = new NodoEscopo();
        var instrucoes = new List<NodoInstrucao>();

        while(Atual().Tipo != TokenTipo.FimDoArquivo)
        {
            instrucoes.Add(ParseInstrucao(escopo));
        }

        escopo.Instrucoes = instrucoes;

        programa = new NodoPrograma(escopo);

        if(programa == null)
            Erro.ErroGenerico("Programa inválido. Não foi possível determinar as instruções");

        return programa;
    }

    private NodoInstrucao? ParseInstrucao(NodoEscopo pai = null)
    {
        NodoInstrucao? instrucao = null;

        if(TentarConsumirToken(TokenTipo.Sair) != null)
        {
            var sair = ParseInstrucaoSair();

            instrucao = sair;

            if(instrucao == null)
                Erro.ErroGenerico("Instrução sair() inválida!", _linha);
        }

        else if(TentarConsumirToken(TokenTipo.Var) != null)
        {
            instrucao = ParseInstrucaoVar();

            if(instrucao == null)
                Erro.ErroGenerico("Instrução var inválida!", _linha);
        }

        else if(TentarConsumirToken(TokenTipo.Const) != null)
        {
            instrucao = ParseInstrucaoConst();

            if(instrucao == null)
                Erro.ErroGenerico("Instrução var inválida!", _linha);
        }

        else if(Atual().Tipo == TokenTipo.Identificador)
        {
            if(Proximo(1).Tipo == TokenTipo.AbrirParen)
            {
                instrucao = ParseInstrucaoChamadaFuncao();
            }
            else
            {
                instrucao = ParseInstrucaoVar(false);
            }

        }

        else if(TentarConsumirToken(TokenTipo.Funcao) != null)
        {
            instrucao = ParseInstrucaoFuncao();
        }

        else if(TentarConsumirToken(TokenTipo.Se) != null)
        {
            instrucao = ParseInstrucaoSe();

            if(instrucao == null)
                Erro.ErroGenerico("Instrução var inválida!", _linha);
        }
        else if(TentarConsumirToken(TokenTipo.Enquanto) != null)
        {
            instrucao = ParseInstrucaoEnquanto();

            if(instrucao == null)
                Erro.ErroGenerico("Instrução var inválida!", _linha);
        }
        else if(TentarConsumirToken(TokenTipo.Romper) != null)
        {
            instrucao = new NodoInstrucaoRomper();
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

    private NodoInstrucaoVar? ParseInstrucaoVar(bool declaracao = true)
    {
        NodoInstrucaoVar? instrucao = null;

        string identificador = TentarConsumirToken(TokenTipo.Identificador).Valor;

        TentarConsumirToken(TokenTipo.OperadorDefinir);

        var expressao = ParseExpressao();

        instrucao = new NodoInstrucaoVar(identificador, expressao, declaracao);

        return instrucao;
    }

    private NodoInstrucaoConst? ParseInstrucaoConst()
    {
        NodoInstrucaoConst? instrucao = null;

        string identificador = TentarConsumirToken(TokenTipo.Identificador).Valor;

        TentarConsumirToken(TokenTipo.OperadorDefinir);

        var expressao = ParseExpressao();

        instrucao = new NodoInstrucaoConst(identificador, expressao);

        return instrucao;
    }

    private NodoInstrucaoChamadaFuncao? ParseInstrucaoChamadaFuncao()
    {
        NodoInstrucaoChamadaFuncao? instrucao = null;

        string identificador = TentarConsumirToken(TokenTipo.Identificador).Valor;

        TentarConsumirToken(TokenTipo.AbrirParen);

        var argumentos = new List<NodoExpressao>();
        while(Atual().Tipo != TokenTipo.FecharParen)
        {
            var expr = ParseExpressao();
            argumentos.Add(expr);

            TentarConsumirToken(TokenTipo.Virgula);
        }

        TentarConsumirToken(TokenTipo.FecharParen);

        instrucao = new NodoInstrucaoChamadaFuncao(identificador, argumentos);

        return instrucao;
    }

    private NodoInstrucaoFuncao? ParseInstrucaoFuncao()
    {
        NodoInstrucaoFuncao? instrucao = null;

        string identificador = TentarConsumirToken(TokenTipo.Identificador).Valor;
        var instrucoes = new List<NodoInstrucao>();

        TentarConsumirToken(TokenTipo.AbrirParen);

        var parametros = new List<string>();

        while(Atual().Tipo != TokenTipo.FecharParen)
        {
            if(Atual().Tipo != TokenTipo.Identificador)
                Erro.ErroGenerico($"Esperado argumento para função {identificador}", Atual().Linha);
            
            parametros.Add(ConsumirToken().Valor);
            TentarConsumirToken(TokenTipo.Virgula);
        }

        TentarConsumirToken(TokenTipo.FecharParen, "Esperado `)`");

        while(Atual().Tipo != TokenTipo.Fim)
        {
            var i = ParseInstrucao();

            if(i != null)
            {
                instrucoes.Add(i);
            }
            else
            {
                Erro.ErroGenerico("Instrução inválida!", _linha);
            }
        }

        TentarConsumirToken(TokenTipo.Fim);

        instrucao = new NodoInstrucaoFuncao(identificador, new NodoEscopo(instrucoes), parametros);

        return instrucao;
    }

    
    private NodoInstrucaoSe? ParseInstrucaoSe()
    {
        NodoInstrucaoSe? instrucao = null;

        var expressao = ParseExpressao();

        TentarConsumirToken(TokenTipo.Entao);

        var instrucoes = new List<NodoInstrucao>();
        var senaoInstrucoes = new List<NodoInstrucao>();

        while(Atual().Tipo != TokenTipo.Fim)
        {
            var i = ParseInstrucao();

            if(i != null)
            {
                instrucoes.Add(i);
            }
            else
            {
                Erro.ErroGenerico("Instrução inválida!", _linha);
            }
        }

        TentarConsumirToken(TokenTipo.Fim);

        if(TentarConsumirToken(TokenTipo.Senao) != null)
        {
            while(Atual().Tipo != TokenTipo.Fim)
            {
                var i = ParseInstrucao();

                if(i != null)
                {
                    senaoInstrucoes.Add(i);
                }
                else
                {
                    Erro.ErroGenerico("Instrução inválida!", _linha);
                }
            }

            TentarConsumirToken(TokenTipo.Fim);
        }
        
        var senaoEscopo = new NodoEscopo();
        if(senaoInstrucoes.Count > 0)
        {
            senaoEscopo = new NodoEscopo(senaoInstrucoes);
        }

        instrucao = new NodoInstrucaoSe(expressao, new NodoEscopo(instrucoes), senaoEscopo);

        return instrucao;
    }

    private NodoInstrucaoEnquanto? ParseInstrucaoEnquanto()
    {
        NodoInstrucaoEnquanto? instrucao = null;

        var expressao = ParseExpressao();

        TentarConsumirToken(TokenTipo.Faca);

        var instrucoes = new List<NodoInstrucao>();
        var escopo = new NodoEscopo(instrucoes);

        while(Atual().Tipo != TokenTipo.Fim)
        {
            var i = ParseInstrucao();

            if(i != null)
            {
                instrucoes.Add(i);
            }
            else
            {
                Erro.ErroGenerico("Instrução inválida!", _linha);
            }
        }

        TentarConsumirToken(TokenTipo.Fim);

        instrucao = new NodoInstrucaoEnquanto(expressao, new NodoEscopo(instrucoes));

        return instrucao;
    }

    private NodoExpressao? ParseExpressao()
    {
        NodoExpressao? expressao = null;

        if(Atual().Tipo == TokenTipo.NumeroLiteral || Atual().Tipo == TokenTipo.Identificador)
        {
            if(TokenEhOperador(Proximo(1)))
            {
                expressao = ParseExpressaoBinaria();
            }
            else
            {
                expressao = new NodoExpressaoTermo(ConsumirToken());
            }

        }

        if(expressao == null)
            Erro.ErroGenerico("Expressão inválida!", _linha);

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

        if(TokenEhOperador(Atual()))
        {
            operador = ConsumirToken();

            direita = ParseExpressao();
        }

        if(esquerda != null && direita != null)
        {
            binaria = new NodoExpressaoBinaria(esquerda, operador, direita);
        }

        if(binaria == null)
            Erro.ErroGenerico("Expressão binária inválida", _linha);

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

        Erro.ErroEsperado(tipo, _linha);

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