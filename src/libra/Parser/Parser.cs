// Árvore Semântica
// Esse arquivo necessita revisão

public abstract class Nodo
{
    // cada nó retornará um tipo de valor diferente
    public abstract object Avaliar();
}

public class NodoExpressao
{
    public NodoExpressao(Token token)
    {
        if(token.Tipo == TokenTipo.Numero)
        {
            Numero = token;
        }
    }

    public Token Numero { get; private set; }

}

public class NodoInstrucaoSair
{
    public NodoInstrucaoSair(NodoExpressao expressao)
    {
        Expressao = expressao;
    }

    public NodoExpressao Expressao { get; private set; }
}

public class NodoInstrucao
{
    public NodoInstrucao(NodoInstrucaoSair saida)
    {
        Saida = saida;
    }

    public NodoInstrucaoSair Saida { get; private set; }

}

public class NodoPrograma
{
    public NodoPrograma(List<NodoInstrucao> instrucoes)
    {
        Instrucoes = instrucoes;
    }

    public List<NodoInstrucao> Instrucoes { get; private set; }

}

public class Parser
{
    public Parser(List<Token> tokens)
    {
        _tokens = tokens;
    }

    private List<Token> _tokens;
    private int _posicao;


    public NodoPrograma Parse()
    {
        NodoPrograma programa = null;

        var instrucoes = new List<NodoInstrucao>();

        while(Atual().Tipo != TokenTipo.FimDoArquivo)
        {
            instrucoes.Add(ParseInstrucao());
        }

        programa = new NodoPrograma(instrucoes);

        return programa;
    }

    private NodoInstrucao ParseInstrucao()
    {
        NodoInstrucao instrucao = null;

        if(Atual().Tipo == TokenTipo.Sair)
        {
            Passar();

            var sair = ParseInstrucaoSair();

            if(Atual().Tipo != TokenTipo.PontoEVirgula)
            {
                Libra.Erro("Esperado ';'");
            }

            Passar();
            
            instrucao = new NodoInstrucao(sair);
        }

        if(instrucao == null)
            Libra.Erro("Instrução inválida!");

        return instrucao;
    }

    private NodoInstrucaoSair ParseInstrucaoSair()
    {
        NodoInstrucaoSair sair = null;

        if(Atual().Tipo == TokenTipo.AbrirParen)
        {
            Passar();
        }
        else
        {
            Libra.Erro("Esperado '('");
        }

        sair = new NodoInstrucaoSair(ParseExpressao());

        if(Atual().Tipo == TokenTipo.FecharParen)
        {
            Passar();
        }
        else
        {
            Libra.Erro("Esperado ')'");
        }

        return sair;
    }
    
    private NodoExpressao ParseExpressao()
    {
        NodoExpressao expressao = null;

        if(Atual().Tipo == TokenTipo.Numero)
        {
            expressao = new NodoExpressao(ConsumirToken());
        }

        if(expressao == null)
            Libra.Erro("Expressão inválida!");

        return expressao;
    }

    private Token Atual() 
    {
        return Peek(0);
    }
    private Token Peek(int offset)
    {
        if(_posicao + offset < _tokens.Count)
        {
            return _tokens[_posicao + offset];
        }

        return new Token(TokenTipo.FimDoArquivo);
    }

    private void Passar(int quantidade = 1) 
    {
        _posicao += quantidade;
    }

    private Token ConsumirToken()
    {
        var token = Atual();
        Passar();

        return token;
    }
}