using System;
using System.Text.RegularExpressions;
using System.Linq;

public class Tokenizador 
{
    private int m_posicao;
    private string? m_fonte;
    private List<Token>? m_tokens;
    private int m_linha;

    public List<Token> Tokenizar(string source) 
    {
        m_fonte = source;
        MacrosDefinir();
        m_tokens = new();
        m_linha = 1;

        var texto = "";
        
        while (Atual() != '\0')
        {
            if(char.IsDigit(Atual()))
            {
                texto += ConsumirChar();

                while(char.IsDigit(Atual()))
                {
                    texto += ConsumirChar();
                }

                AdicionarTokenALista(TokenTipo.NumeroLiteral, texto);
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
                        AdicionarTokenALista(TokenTipo.Sair);
                        break;
                    case "var":
                        AdicionarTokenALista(TokenTipo.Var);
                        break;
                    case "exibir":
                        AdicionarTokenALista(TokenTipo.Exibir);
                        break;
                    default:
                        AdicionarTokenALista(TokenTipo.Identificador, texto);
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
                        m_linha++;
                        Passar();
                        break;
                    case ';':
                        AdicionarTokenALista(TokenTipo.PontoEVirgula);
                        Passar();
                        break;
                    case '(':
                        AdicionarTokenALista(TokenTipo.AbrirParen);
                        Passar();
                        break;
                    case '+':
                        AdicionarTokenALista(TokenTipo.OperadorSoma);
                        Passar();
                        break;
                    case '-':
                        AdicionarTokenALista(TokenTipo.OperadorSub);
                        Passar();
                        break;
                    case '*':
                        AdicionarTokenALista(TokenTipo.OperadorMult);
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

                        AdicionarTokenALista(TokenTipo.OperadorDiv);
                        Passar();
                        break;
                    case ')':
                        AdicionarTokenALista(TokenTipo.FecharParen);
                        Passar();
                        break;
                    case '=':
                        Passar();

                        if(Atual() == '=')
                        {
                            AdicionarTokenALista(TokenTipo.OperadorComparacao);
                            Passar();
                        }
                        else
                        {
                            AdicionarTokenALista(TokenTipo.OperadorDefinir);
                            Passar();
                        }
                        break;
                    case '"':
                        Passar();

                        while(Atual() != '"')
                        {
                            texto += ConsumirChar();
                        }

                        AdicionarTokenALista(TokenTipo.StringLiteral, texto);
                        texto = "";
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
                        Erro.ErroGenerico($"Simbolo inválido '{Atual()}'");
                        break;
                }
                
            }

        }
        
        AdicionarTokenALista(TokenTipo.FimDoArquivo);

        return m_tokens;
    }

    // esse é literalmente o pior método que eu já criei na vida
    // em minha defesa, é 1 da manhã, eu dormi apenas 4 horas na noite passada
    // e eu vi algo parecido no c++ e decidi implementar aqui
    // talvez eu refatore isso depois (isso se for pra build final)
    private void MacrosDefinir()
    {
        Dictionary<string,string> defs = new Dictionary<string, string>();

        string def = "";
        string buf = "";

        var esq = string.Empty;
        var dir = string.Empty;

        var lendoDef = false;

        for(int i = 0; i < m_fonte.Length; i++)
        {
            if(m_fonte[i] == '#')
            {
                def += m_fonte[i];
                buf += m_fonte[i];
                i++;

                while(char.IsLetter(m_fonte[i]))
                {
                    def += m_fonte[i];
                    buf += m_fonte[i];
                    i++;
                }

                i--;

                if(buf != "#def")
                    Erro.ErroGenerico($"Macro inválida {buf}");
                
                lendoDef = true;
                buf = "";
            }

            else if (char.IsLetterOrDigit(m_fonte[i]) || char.IsSymbol(m_fonte[i]))
            {
                if(lendoDef)
                {
                    def += m_fonte[i];
                    buf += m_fonte[i];
                    i++;

                    if(esq == string.Empty)
                    {
                        while(m_fonte[i] != ' ')
                        {
                            def += m_fonte[i];
                            buf += m_fonte[i];
                            i++;
                        }
                        i--;

                        esq = buf;
                    }
                    else
                    {
                        while(m_fonte[i] != '\n')
                        {
                            def += m_fonte[i];
                            buf += m_fonte[i];
                            i++;
                        }
                        i--;

                        dir = buf;

                        defs[esq] = dir;

                        esq = "";
                        dir = "";

                        lendoDef = false;
                    }

                    buf = "";
                }
                
            }
        }

        foreach (string d in defs.Keys)
        {
            m_fonte = m_fonte.Replace($"#def {d} {defs[d]}", "");

            m_fonte = m_fonte.Replace(d, defs[d]);
        }
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

    private void AdicionarTokenALista(TokenTipo tipo)
    {
        m_tokens.Add(new Token(tipo, m_linha));
    }

    private void AdicionarTokenALista(TokenTipo tipo, object valor)
    {
        m_tokens.Add(new Token(tipo, m_linha, valor));
    }

}