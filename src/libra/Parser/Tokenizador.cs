using System;
using System.Text.RegularExpressions;
using System.Linq;

namespace Libra;

public class Tokenizador 
{
    private int _posicao;
    private string? _fonte;
    private List<Token>? _tokens;
    private int _linha;

    public List<Token> Tokenizar(string source) 
    {
        _fonte = source;
        _tokens = new();
        _linha = 1;

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

            else if(char.IsLetter(Atual()) || Atual() == '_')
            {
                texto += ConsumirChar();

                while(char.IsLetterOrDigit(Atual()) || Atual() == '_')
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
                    case "const":
                        AdicionarTokenALista(TokenTipo.Const);
                        break;
                    case "funcao":
                        AdicionarTokenALista(TokenTipo.Funcao);
                        break;
                    case "exibir":
                        AdicionarTokenALista(TokenTipo.Exibir);
                        break;
                    case "tipo":
                        AdicionarTokenALista(TokenTipo.Tipo);
                        break;
                    case "se":
                        AdicionarTokenALista(TokenTipo.Se);
                        break;
                    case "senao":
                        AdicionarTokenALista(TokenTipo.Senao);
                        break;
                    case "enquanto":
                        AdicionarTokenALista(TokenTipo.Enquanto);
                        break;
                    case "faca":
                        AdicionarTokenALista(TokenTipo.Faca);
                        break;
                    case "romper":
                        AdicionarTokenALista(TokenTipo.Romper);
                        break;
                    case "retornar":
                        AdicionarTokenALista(TokenTipo.Retornar);
                        break;
                    case "entao":
                        AdicionarTokenALista(TokenTipo.Entao);
                        break;
                    case "fim":
                        AdicionarTokenALista(TokenTipo.Fim);
                        break;
                    case "bytes":
                        AdicionarTokenALista(TokenTipo.Bytes);
                        break;
                    case "ou":
                        AdicionarTokenALista(TokenTipo.OperadorOu);
                        break;
                    case "e":
                        AdicionarTokenALista(TokenTipo.OperadorE);
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
                        _linha++;
                        //AdicionarTokenALista(TokenTipo.NovaLinha);
                        Passar();
                        break;
                    case '\r':
                        Passar();
                        break;
                    case '\t':
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
                    case '>':
                        if(Proximo(1) == '=')
                        {
                            AdicionarTokenALista(TokenTipo.OperadorMaiorIgualQue);
                            Passar();
                            Passar();
                            break;
                        }

                        AdicionarTokenALista(TokenTipo.OperadorMaiorQue);
                        Passar();
                        break;
                    case '<':
                        if(Proximo(1) == '=')
                        {
                            AdicionarTokenALista(TokenTipo.OperadorMenorIgualQue);
                            Passar();
                            Passar();
                            break;
                        }

                        AdicionarTokenALista(TokenTipo.OperadorMenorQue);
                        Passar();
                        break;
                    case '!':
                        if(Proximo(1) == '=')
                        {
                            AdicionarTokenALista(TokenTipo.OperadorDiferente);
                            Passar();
                            Passar();
                            break;
                        }

                        AdicionarTokenALista(TokenTipo.OperadorNeg);
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
                        
                        if(Proximo(1) == '/')
                        {
                            Passar();

                            while(Atual() != '\n' || Atual() != '\0')
                            {
                                // solução imbecil pra conseguir parar o comentário sem quebrar linha
                                if(Atual() == '*')
                                {
                                    if(Proximo(1) == '\\')
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

                        AdicionarTokenALista(TokenTipo.TextoLiteral, texto);
                        texto = "";
                        Passar();
                        break;
                    case '\'':
                        Passar();
                        texto += ConsumirChar();
                        AdicionarTokenALista(TokenTipo.CaractereLiteral, texto);
                        texto = "";
                        if(Proximo(1) != '\'')
                        {
                            Erros.LancarErro(new ErroEsperado("`'`"));
                        }
                        Passar();
                        break;
                    case '#':
                        while(Atual() != '\n')
                        {
                            Passar();
                        }

                        Passar();
                        break;
                    case ',':
                        AdicionarTokenALista(TokenTipo.Virgula);
                        break;
                    default:
                        Erros.LancarErro(new ErroTokenInvalido($"{Atual()} (ASCII = {(int)Atual()})"));
                        break;
                }
                
            }

        }
        
        AdicionarTokenALista(TokenTipo.FimDoArquivo);

        return _tokens;
    }

    private char Atual() 
    {
        return Proximo(0);
    }
    
    private char Proximo(int offset)
    {
        if(_posicao + offset < _fonte.Length)
        {
            return _fonte[_posicao + offset];
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

    private void AdicionarTokenALista(TokenTipo tipo)
    {
        _tokens.Add(new Token(tipo, _linha));
    }

    private void AdicionarTokenALista(TokenTipo tipo, string valor)
    {
        _tokens.Add(new Token(tipo, _linha, valor));
    }

}