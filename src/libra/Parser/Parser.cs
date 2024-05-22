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

public class NodoPrograma
{
    public NodoPrograma(NodoExpressao expressao)
    {
        ExpressaoSaida = expressao;
    }

    public NodoExpressao ExpressaoSaida { get; private set; }


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

        var expressao = ParseExpressao();

        if(expressao != null)
        {
            programa = new NodoPrograma(expressao);
        }

        return programa;
    }


    private NodoExpressao ParseExpressao()
    {
        NodoExpressao expressao = null;

        while(Atual().Tipo != TokenTipo.FimDoArquivo)
        {
            Passar();

            if(Atual().Tipo == TokenTipo.Numero)
            {
                expressao = new NodoExpressao(ConsumirToken());
            }
        }

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