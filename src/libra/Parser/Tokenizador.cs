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
    private bool _falha;

    public List<Token> Tokenizar(string source) 
    {
        _fonte = source;
        _tokens = new();
        _linha = 1;

        var texto = "";
        try
        {
            while (Atual() != '\0' && !_falha)
            {
                if(char.IsDigit(Atual()))
                {
                    var ponto = false;
                    while(char.IsDigit(Atual()))
                    {
                        texto += ConsumirChar();
                        if(Atual() == '.') {
                            if(ponto)
                                throw new Erro("Número inválido!", _linha);

                            texto += ConsumirChar();
                            ponto = true;
                        }
                    }

                    if(ponto)
                        AdicionarTokenALista(TokenTipo.NumeroLiteral, double.Parse(texto));
                    else
                        AdicionarTokenALista(TokenTipo.NumeroLiteral, int.Parse(texto));
                    texto = "";
                    ponto = false;
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
                        case "var":
                            AdicionarTokenALista(TokenTipo.Var);
                            break;
                        case "const":
                            AdicionarTokenALista(TokenTipo.Const);
                            break;
                        case "funcao":
                            AdicionarTokenALista(TokenTipo.Funcao);
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
                        case "ou":
                            AdicionarTokenALista(TokenTipo.OperadorOu);
                            break;
                        case "e":
                            AdicionarTokenALista(TokenTipo.OperadorE);
                            break;
                        case "importar":
                            texto = "";
                            while(Atual() == ' ')
                                Passar();

                            if(Atual() != '"')
                                throw new Erro("Esperado `\"`");

                            ConsumirChar();

                            while(Atual() != '"')
                            {
                                texto += ConsumirChar();
                            }

                            ConsumirChar(); // Consumindo `"`
  
                            string arquivoCarregado = File.ReadAllText(texto);
                            _fonte = arquivoCarregado + _fonte;
                            string linha = $"importar \"{texto}\"";
                            _fonte = _fonte.Replace(linha, "");
                            _posicao -= linha.Length;
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
                        case '[':
                            ConsumirChar();
                            AdicionarTokenALista(TokenTipo.AbrirCol);
                            break;
                        case ']':
                            AdicionarTokenALista(TokenTipo.FecharCol);
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
                            Passar();
                            if (Atual() == '/')
                            {
                                ConsumirComentarioLinha();
                            }
                            else if (Atual() == '*')
                            {
                                ConsumirComentarioBloco();
                            }
                            else
                            {
                                AdicionarTokenALista(TokenTipo.OperadorDiv);
                            }
                            break;
                        case '^':
                            AdicionarTokenALista(TokenTipo.OperadorPot);
                            Passar();
                            break;
                        case ')':
                            AdicionarTokenALista(TokenTipo.FecharParen);
                            Passar();
                            break;
                        case '=':
                            ConsumirChar();

                            if(Atual() == '=')
                            {
                                AdicionarTokenALista(TokenTipo.OperadorComparacao);
                                Passar();
                            }
                            else
                            {
                                AdicionarTokenALista(TokenTipo.OperadorDefinir);
                            }
                            break;
                        case '"':
                            Passar();

                            while(Atual() != '"')
                            {
                                if(Atual() == '\\' && Proximo(1) == '"')
                                {
                                    Passar();
                                }
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
                            if(Atual() != '\'')
                            {
                                throw new Erro("Esperado `'`", _linha);
                            }
                            Passar();
                            break;
                        case ',':
                            AdicionarTokenALista(TokenTipo.Virgula);
                            Passar();
                            break;
                        default:
                            throw new ErroTokenInvalido($"{Atual()} (ASCII = {(int)Atual()})", _linha);
                            break;
                    }
                    
                }

            }
        
            AdicionarTokenALista(TokenTipo.FimDoArquivo);

            if (_falha)
                return new List<Token>();

            return _tokens;
            }
        catch (Exception e)
        {
            new Erro($"{e}");
        }

        return null;
    }

    private void ConsumirComentarioLinha()
    {
        Passar();
        while (Proximo(1) != '\n' && Proximo(1) != '\0')
        {
            Passar();
        }
    }

    private void ConsumirComentarioBloco()
    {
        Passar();
        while (Proximo(1) != '\0')
        {
            if (Atual() == '*' && Proximo(1) == '/')
            {
                Passar(); // Consome '*'
                Passar(); // Consome '/'
                break;
            }
            Passar();
        }
    }

    public void PrintarListaTokens()
    {
        foreach(var token in _tokens)
        {
            Ambiente.Msg(token.ToString());
        }
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

    private void AdicionarTokenALista(TokenTipo tipo, object valor)
    {
        _tokens.Add(new Token(tipo, _linha, valor));
    }
}