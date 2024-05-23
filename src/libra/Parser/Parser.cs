// Árvore Semântica
// Esse arquivo necessita revisão

// eu poderia usar typeof() ao invés de enums, mas prefiro dessa forma
public enum ExpressaoTipo
{
    Numero, ExpressaoBinaria, Identificador
}

public abstract class Nodo
{
    // cada nó retornará um tipo de valor diferente
    public abstract object Avaliar();
}

public class NodoExpressao : Nodo
{
    public NodoExpressao(Token token)
    {
        if(token.Tipo == TokenTipo.Numero)
        {
            Tipo = ExpressaoTipo.Numero;
            _token = token;
        }
    }

    public ExpressaoTipo Tipo { get; private set; }
    private Token _token;
    
    public override object Avaliar()
    {
        switch (Tipo)
        {
            case ExpressaoTipo.Numero:
                return _token.Valor;
            case ExpressaoTipo.Identificador:
                return 0; // depois eu desenvolvo isso
        }

        return 0;
    }
}

public class NodoInstrucaoSair : Nodo
{
    public NodoInstrucaoSair(NodoExpressao expressao)
    {
        Expressao = expressao;
    }

    public NodoExpressao Expressao { get; private set; }

    public override object Avaliar()
    {
        return Expressao.Avaliar();
    }
}

public class NodoInstrucao : Nodo
{
    public NodoInstrucao(NodoInstrucaoSair saida)
    {
        _instrucao = saida;
    }

    private object _instrucao;

    public override object Avaliar()
    {
        if(_instrucao.GetType() == typeof(NodoInstrucaoSair))
        {
            var sair = (NodoInstrucaoSair)_instrucao;

            return sair.Avaliar();
        }

        return 0;
    }
}

public class NodoPrograma : Nodo
{
    public NodoPrograma(List<NodoInstrucao> instrucoes)
    {
        Instrucoes = instrucoes;
    }

    public List<NodoInstrucao> Instrucoes { get; private set; }

    public int CodigoSaida { get; private set; }

    public override object Avaliar()
    {
        return CodigoSaida;
    }

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