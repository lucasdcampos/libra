using System;
using System.Text.RegularExpressions;
using System.Linq;
using System.ComponentModel;

namespace Libra;

public class Tokenizador 
{
    private int _posicao;
    private string? _fonte;
    private List<Token>? _tokens;
    private Dictionary<string, int> _arquivosImportados = new();
    private LocalToken _local;
    private Dictionary<string, TokenTipo> _palavrasReservadas = new Dictionary<string, TokenTipo>
    {
        { "var", TokenTipo.Var },
        { "const", TokenTipo.Const },
        { "funcao", TokenTipo.Funcao },
        { "se", TokenTipo.Se },
        { "senao", TokenTipo.Senao },
        { "enquanto", TokenTipo.Enquanto },
        { "faca", TokenTipo.Faca },
        { "romper", TokenTipo.Romper },
        { "continuar", TokenTipo.Continuar },
        { "retornar", TokenTipo.Retornar },
        { "entao", TokenTipo.Entao },
        { "fim", TokenTipo.Fim },
        { "nulo", TokenTipo.Nulo },
        { "ou", TokenTipo.OperadorOu },
        { "e", TokenTipo.OperadorE },
        { "neg", TokenTipo.OperadorNeg },
        { "nao", TokenTipo.OperadorNeg }
    };

    public List<Token> Tokenizar(string source, string arquivo = "") 
    {
        _fonte = source.ReplaceLineEndings("\n");
        _tokens = new();
        _local = new LocalToken(arquivo, 1);
        _posicao = 0;
        
        var texto = "";
        try
        {
            while (Atual() != '\0')
            {
                if(Atual() == '0')
                {
                    if(Proximo(1) == 'b')
                    {
                        ConsumirChar();
                        ConsumirChar();
                        TokenizarBinario();
                        continue;
                    }
                    if(Proximo(1) == 'x')
                    {
                        ConsumirChar();
                        ConsumirChar();
                        TokenizarHexa();
                        continue;
                    }
                }
                if(char.IsDigit(Atual()))
                {
                    TokenizarNumero();
                }

                else if(char.IsLetter(Atual()) || Atual() == '_')
                {
                    TokenizarPalavra();
                }
                else 
                {
                    TokenizarSimbolo();
                }
            }
        
            AdicionarTokenALista(TokenTipo.FimDoArquivo);

            return _tokens;
        }
        catch (Exception e)
        {
            Erro.MensagemBug(e);
        }

        return null;
    }

    private void TokenizarBinario()
    {
        string buffer = "";
        
        if(Atual() != '0' && Atual() != '1')
            throw new Erro("Esperado `0` ou `1`, recebido: " + Atual(), _local);

        while (Atual() == '0' || Atual() == '1')
        {
            buffer += ConsumirChar();
            if(buffer.Length >= 31)
                break;
        }

        int resultado = 0;
        try 
        {
            resultado = Convert.ToInt32(buffer, 2);
        }
        catch
        {
            throw new ErroAcessoNulo($" Não foi possível converter {buffer} para Int");
        }

        AdicionarTokenALista(TokenTipo.NumeroLiteral, resultado);
    }

    private bool DigitoHexadecimal(char c)
    {
        switch(c.ToString().ToUpper()[0])
        {
            case '0':
            case '1':
            case '2':
            case '3':
            case '4':
            case '5':
            case '6':
            case '7':
            case '8':
            case '9':
            case 'A':
            case 'B':
            case 'C':
            case 'D':
            case 'E':
            case 'F':
                return true;
                break;
            default:
                return false;
            break;
        }
    }
    private void TokenizarHexa()
    {
        string buffer = "";
        
        if(DigitoHexadecimal(Atual()))
            buffer += ConsumirChar();
        else
            throw new Erro("Esperado digito Hexadecimal, recebido: " + Atual(), _local);
        
        
        while (DigitoHexadecimal(Atual()))
        {
            buffer += ConsumirChar();
            if(buffer.Length >= 7)
                break;
        }

        int resultado = 0;
        try 
        {
            resultado = Convert.ToInt32(buffer, 16);
        }
        catch
        {
            throw new ErroAcessoNulo($" Não foi possível converter {buffer} para Int");
        }
        AdicionarTokenALista(TokenTipo.NumeroLiteral, resultado);
    }

    private void TokenizarNumero()
    {
        string buffer = "";
        var ponto = false;
        while (char.IsDigit(Atual()) || Atual() == '_')
        {
            // Ignorando underscores em números, facilita visualização
            // de números grandes. Ex: 1_000_000_000 = 1000000000
            if(Atual() == '_')
            {
                ConsumirChar();
                continue;
            }
                
            buffer += ConsumirChar();
            if (Atual() == '.')
            {
                if (ponto)
                    throw new Erro("Número inválido!", _local);

                buffer += ConsumirChar();
                ponto = true;
            }
        }

        if (ponto)
            AdicionarTokenALista(TokenTipo.NumeroLiteral, double.Parse(buffer));
        else
            AdicionarTokenALista(TokenTipo.NumeroLiteral, int.Parse(buffer));
    }

    private string TokenizarIdentificador()
    {
        string buffer = "";

        while (char.IsLetterOrDigit(Atual()) || Atual() == '_')
        {
            buffer += ConsumirChar();
        }

        return buffer;
    }

    private void TokenizarPalavra()
    {
        string buffer = "" + ConsumirChar();

        while (char.IsLetterOrDigit(Atual()) || Atual() == '_')
        {
            buffer += ConsumirChar();
        }
        
        if(buffer == "importar")
        {
            ConsumirEspacos();

            if(Atual() != '"')
            {
                var arquivo = TokenizarIdentificador();
                if(string.IsNullOrEmpty(arquivo))
                    throw new Erro("Esperado `\"`", _local);
                ImportarArquivo(arquivo + ".libra");
                return;
            }
                
            
            ConsumirChar(); // Consumindo `"`
            string caminhoArquivo = "";

            while(Atual() != '"')
            {
                if(Atual() == '\n' || ConsumirEspacos() != 0)
                    throw new Erro("Esperado `\"`", _local);

                caminhoArquivo += ConsumirChar();
            }
            ConsumirChar(); // Consumindo `"`

            ImportarArquivo(caminhoArquivo);

            return;
        }

        if(buffer == "senao")
        {
            ConsumirEspacos();

            if(Atual() == 's' && Proximo(1) == 'e')
            {
                ConsumirChar();
                ConsumirChar();
                AdicionarTokenALista(TokenTipo.SenaoSe);
                return;
            }
        }

        if (_palavrasReservadas.ContainsKey(buffer))
        {
            AdicionarTokenALista(_palavrasReservadas[buffer]);
        }
        else
        {
            AdicionarTokenALista(TokenTipo.Identificador, buffer);
        }
    }

    private void TokenizarSimbolo()
    {
        string buffer = "";
        switch (Atual())
        {
            case ' ':
                ConsumirChar();
                break;
            case '\n':
                ConsumirChar();
                break;
            case '\r':
                ConsumirChar();
                break;
            case '\t':
                ConsumirChar();
                break;
            case ';':
                AdicionarTokenALista(TokenTipo.PontoEVirgula);
                ConsumirChar();
                break;
            case '(':
                AdicionarTokenALista(TokenTipo.AbrirParen);
                ConsumirChar();
                break;
            case '[':
                ConsumirChar();
                AdicionarTokenALista(TokenTipo.AbrirCol);
                break;
            case ']':
                AdicionarTokenALista(TokenTipo.FecharCol);
                ConsumirChar();
                break;
            case '{':
                ConsumirChar();
                AdicionarTokenALista(TokenTipo.AbrirChave);
                break;
            case '}':
                AdicionarTokenALista(TokenTipo.FecharChave);
                ConsumirChar();
                break;
            case '>':
                if (Proximo(1) == '=')
                {
                    AdicionarTokenALista(TokenTipo.OperadorMaiorIgualQue);
                    ConsumirChar();
                    ConsumirChar();
                    break;
                }

                AdicionarTokenALista(TokenTipo.OperadorMaiorQue);
                ConsumirChar();
                break;
            case '<':
                if (Proximo(1) == '=')
                {
                    AdicionarTokenALista(TokenTipo.OperadorMenorIgualQue);
                    ConsumirChar();
                    ConsumirChar();
                    break;
                }

                AdicionarTokenALista(TokenTipo.OperadorMenorQue);
                ConsumirChar();
                break;
            case '!':
                if (Proximo(1) == '=')
                {
                    AdicionarTokenALista(TokenTipo.OperadorDiferente);
                    ConsumirChar();
                    ConsumirChar();
                    break;
                }
                AdicionarTokenALista(TokenTipo.OperadorNeg);
                ConsumirChar();
                break;
            case '+':
                AdicionarTokenALista(TokenTipo.OperadorSoma);
                ConsumirChar();
                break;
            case '-':
                AdicionarTokenALista(TokenTipo.OperadorSub);
                ConsumirChar();
                break;
            case '*':
                AdicionarTokenALista(TokenTipo.OperadorMult);
                ConsumirChar();
                break;
            case '/':
                ConsumirChar();
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
                ConsumirChar();
                break;
            case '%':
                AdicionarTokenALista(TokenTipo.OperadorResto);
                ConsumirChar();
                break;
            case ')':
                AdicionarTokenALista(TokenTipo.FecharParen);
                ConsumirChar();
                break;
            case '=':
                ConsumirChar();

                if (Atual() == '=')
                {
                    AdicionarTokenALista(TokenTipo.OperadorComparacao);
                    ConsumirChar();
                }
                else
                {
                    AdicionarTokenALista(TokenTipo.OperadorDefinir);
                }
                break;
            case '"':
                ConsumirChar();

                while (Atual() != '"')
                {
                    if (Atual() == '\\' && Proximo(1) == '"')
                    {
                        ConsumirChar();
                    }
                    buffer += ConsumirChar();
                }

                AdicionarTokenALista(TokenTipo.TextoLiteral, buffer);
                buffer = "";
                ConsumirChar();
                break;
            case '\'':
                ConsumirChar();
                buffer += ConsumirChar();
                AdicionarTokenALista(TokenTipo.CaractereLiteral, buffer);
                buffer = "";
                if (Atual() != '\'')
                {
                    throw new Erro("Esperado `'`", _local);
                }
                ConsumirChar();
                break;
            case ',':
                AdicionarTokenALista(TokenTipo.Virgula);
                ConsumirChar();
                break;
            case ':':
                AdicionarTokenALista(TokenTipo.DoisPontos);
                ConsumirChar();
                break;
            case '\0':
                throw new ErroAcessoNulo(" Chegou ao fim do arquivo de forma inesperada.");
                break;
            default:
                throw new ErroTokenInvalido($"{Atual()} ASCII = {(int)Atual()}", _local);
                break;
        }
    }

    private string ConsumirAte(char caractere)
    {
        string buffer = "";
        while(Atual() != caractere)
        {
            buffer += ConsumirChar();

            if (Atual() == '\0')
                throw new Erro($"Esperado `{caractere}`", _local);
        }

        ConsumirChar();
        return buffer;
    }

    private int ConsumirEspacos()
    {
        int espacosConsumidos = 0;
        while(Atual() == ' ')
        {
            espacosConsumidos++;
            ConsumirChar();
        }
        return espacosConsumidos;
    }

    private void ConsumirEspacosELinhas()
    {
        while(Atual() == ' ' || Atual() == '\n')
        {
            if(Atual() == '\n')
                _local.Linha++;

            ConsumirChar();
        }
    }

    private void ConsumirComentarioLinha()
    {
        ConsumirChar();
        while(Atual() != '\n' && Atual() != '\0')
        {
            ConsumirChar();
        }
        ConsumirChar();
    }

    private void ConsumirComentarioBloco()
    {
        ConsumirChar();
        while (Proximo(1) != '\0')
        {
            if (Atual() == '*' && Proximo(1) == '/')
            {
                ConsumirChar(); // Consome '*'
                ConsumirChar(); // Consome '/'
                break;
            }

            ConsumirChar();
        }
    }

    private void ImportarArquivo(string caminho)
    {
        if(_arquivosImportados.ContainsKey(caminho))
            return;
        
        _arquivosImportados.Add(caminho, 1);

        string caminhoExecutavel = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "biblioteca/" + caminho);
        string caminhoUsuario = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), caminho);

        if (!File.Exists(caminhoExecutavel) && !File.Exists(caminhoUsuario))
            throw new ErroAcessoNulo(caminhoExecutavel);

        string caminhoFinalArquivo = File.Exists(caminhoExecutavel) ? caminhoExecutavel : caminhoUsuario;

        string codigoArquivo = File.ReadAllText(caminhoFinalArquivo).ReplaceLineEndings("\n");
        
        var novosTokens = new Tokenizador().Tokenizar(codigoArquivo, caminho);

        for (int i = 0; i < novosTokens.Count - 1; i++)
        {
            _tokens.Add(novosTokens[i]);
        }

    }

    public void PrintarListaTokens()
    {
        Console.WriteLine($"Tokens {_local.Arquivo}:");
        foreach(var token in _tokens)
        {
            Ambiente.Msg("    " + token.ToString());
        }
        Ambiente.Msg("\n");
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

    private char ConsumirChar()
    {
        var c = Atual();
        _posicao++;
        if(c == '\n')
            _local.Linha++;
            
        return c;
    }

    private void AdicionarTokenALista(TokenTipo tipo)
    {
        _tokens.Add(new Token(tipo, _local));
    }

    private void AdicionarTokenALista(TokenTipo tipo, object valor)
    {
        _tokens.Add(new Token(tipo, _local, valor));
    }
}