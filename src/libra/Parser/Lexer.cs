public class Lexer 
{
    public Lexer(string source)
    {
        _source = source;
    }

    private int _posicao;
    private string _source;

    public List<Token> Tokenize() 
    {
        var texto = "";
        var tokens = new List<Token>();

        while (Atual() != '\0')
        {
            if(char.IsDigit(Atual()))
            {
                texto += ConsumirChar();

                while(char.IsDigit(Atual()))
                {
                    texto += ConsumirChar();
                }

                tokens.Add(new Token(TokenTipo.Numero, texto));
                texto = "";
                continue;
            }

            else if(char.IsLetter(Atual()))
            {
                texto += ConsumirChar();

                while(char.IsLetterOrDigit(Atual()))
                {
                    texto += ConsumirChar();
                }

                if(texto == "sair")
                {
                    tokens.Add(new Token(TokenTipo.Sair));
                    texto = "";
                }
                else
                {
                    tokens.Add(new Token(TokenTipo.Identificador, texto));
                    texto = "";
                }
                    
            }

            else 
            {

                switch(Atual())
                {
                    case ' ':
                        Passar();
                        break;
                    case ';':
                        tokens.Add(new Token(TokenTipo.PontoEVirgula));
                        Passar();
                        break;
                    case '(':
                        tokens.Add(new Token(TokenTipo.AbrirParen));
                        Passar();
                        break;
                    case ')':
                        tokens.Add(new Token(TokenTipo.FecharParen));
                        Passar();
                        break;
                    case '#':
                        while(Atual() != '\n')
                        {
                            Passar();
                        }

                        Passar();
                        break;
                    default:
                        Libra.Erro($"Simbolo inválido {Atual()}");
                        break;
                }
                
            }

        }
        
        tokens.Add(new Token(TokenTipo.FimDoArquivo));

        return tokens;
    }

    private char Atual() 
    {
        return Peek(0);
    }
    
    private char Peek(int offset)
    {
        if(_posicao + offset < _source.Length)
        {
            return _source[_posicao + offset];
        }

        return '\0';
    }

    private void Passar(int quantidade = 1) 
    {
        _posicao += quantidade;
    }

    private char ConsumirChar()
    {
        var c = Atual();
        Passar();

        return c;
    }
}