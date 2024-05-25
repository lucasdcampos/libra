public class Tokenizador 
{
    private int m_posicao;
    private string m_fonte;

    public List<Token> Tokenizar(string source) 
    {
        m_fonte = source;

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

                tokens.Add(new Token(TokenTipo.NumeroLiteral, texto));
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

                switch(texto)
                {
                    case "sair":
                        tokens.Add(new Token(TokenTipo.Sair));
                        break;
                    case "var":
                        tokens.Add(new Token(TokenTipo.Var));
                        break;
                    case "exibir":
                        tokens.Add(new Token(TokenTipo.Exibir));
                        break;
                    default:
                        tokens.Add(new Token(TokenTipo.Identificador, texto));
                        break;
                }

                texto = "";
                    
            }

            else 
            {

                switch(Atual())
                {
                    case ' ':
                        Passar();
                        break;
                    case '\n':
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
                    case '+':
                        tokens.Add(new Token(TokenTipo.OperadorSoma));
                        Passar();
                        break;
                    case '-':
                        tokens.Add(new Token(TokenTipo.OperadorSub));
                        Passar();
                        break;
                    case '*':
                        tokens.Add(new Token(TokenTipo.OperadorMult));
                        Passar();
                        break;
                    case '/':
                        
                        if(Peek(1) == '/')
                        {
                            Passar();

                            while(Atual() != '\n' || Atual() != '\0')
                            {
                                // solução imbecil pra conseguir parar o comentário sem quebrar linha
                                if(Atual() == '*')
                                {
                                    if(Peek(1) == '\\')
                                    {
                                        Passar();
                                        Passar();
                                        break;
                                    }
                                }
                                Passar();
                            }
                                
                            break;
                        }

                        tokens.Add(new Token(TokenTipo.OperadorDiv));
                        Passar();
                        break;
                    case ')':
                        tokens.Add(new Token(TokenTipo.FecharParen));
                        Passar();
                        break;
                    case '=':
                        Passar();

                        if(Atual() == '=')
                        {
                            tokens.Add(new Token(TokenTipo.OperadorComparacao));
                            Passar();
                        }
                        else
                        {
                            tokens.Add(new Token(TokenTipo.OperadorDefinir));
                            Passar();
                        }

                        break;
                    case '#':
                        while(Atual() != '\n')
                        {
                            Passar();
                        }

                        Passar();
                        break;
                    default:
                        //Libra.Erro($"Simbolo inválido {Atual()}");
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
        if(m_posicao + offset < m_fonte.Length)
        {
            return m_fonte[m_posicao + offset];
        }

        return '\0';
    }

    private void Passar(int quantidade = 1) 
    {
        m_posicao += quantidade;
    }

    private char ConsumirChar()
    {
        var c = Atual();
        Passar();

        return c;
    }
}