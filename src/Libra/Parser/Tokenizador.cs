using System;
using System.Text.RegularExpressions;
using System.Linq;
using System.ComponentModel;
using System.Text;

namespace Libra;

public class Tokenizador 
{
    private int _posicao;
    private string? _fonte;
    private List<Token>? _tokens;
    private HashSet<string> _arquivosImportados = new();
    private LocalFonte _local;
    private Dictionary<string, TokenTipo> _palavrasReservadas = new Dictionary<string, TokenTipo>
    {
        { "var", TokenTipo.Var },
        { "const", TokenTipo.Const },
        { "funcao", TokenTipo.Funcao },
        { "classe", TokenTipo.Classe },
        { "se", TokenTipo.Se },
        { "senao", TokenTipo.Senao },
        { "enquanto", TokenTipo.Enquanto },
        { "repetir", TokenTipo.Repetir },
        { "para", TokenTipo.Para },
        { "cada", TokenTipo.Cada },
        { "em", TokenTipo.Em },
        { "romper", TokenTipo.Romper },
        { "continuar", TokenTipo.Continuar },
        { "retornar", TokenTipo.Retornar },
        { "tentar", TokenTipo.Tentar },
        { "capturar", TokenTipo.Capturar },
        { "entao", TokenTipo.Entao },
        { "fim", TokenTipo.Fim },
        { "nulo", TokenTipo.Nulo },
        { "ou", TokenTipo.OperadorOu },
        { "e", TokenTipo.OperadorE },
        { "neg", TokenTipo.OperadorNeg },
        { "nao", TokenTipo.OperadorNeg }
    };

    public Tokenizador(string fonte, string nomeArquivo, string caminho)
    {
        _fonte = fonte.ReplaceLineEndings("\n");
        _tokens = new();
        _local = new LocalFonte(caminho, nomeArquivo, 1);
        _posicao = 0;
    }

    public List<Token> Tokenizar()
    {
        while (Atual() != '\0')
        {
            if (char.IsDigit(Atual()))
            {
                TokenizarNumero();
            }
            else if (char.IsLetter(Atual()) || Atual() == '_')
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

    private void TokenizarBinario()
    {
        var buffer = new StringBuilder();
        
        if(Atual() != '0' && Atual() != '1')
            throw new Erro("Esperado `0` ou `1`, recebido: " + Atual(), _local);

        while (Atual() == '0' || Atual() == '1')
        {
            buffer.Append(ConsumirChar());
            if(buffer.Length >= 31)
                break;
        }

        int resultado = 0;
        try 
        {
            resultado = Convert.ToInt32(buffer.ToString(), 2);
        }
        catch
        {
            throw new ErroAcessoNulo($" Não foi possível converter {buffer} para Int");
        }

        AdicionarTokenALista(TokenTipo.NumeroLiteral, resultado);
    }

    private bool DigitoHexadecimal(char c)
    {
        c = char.ToUpper(c);
        return (c >= '0' && c <= '9') || (c >= 'A' && c <= 'F');
    }
    
    private void TokenizarHexa()
    {
        var buffer = new StringBuilder();

        if (DigitoHexadecimal(Atual()))
            buffer.Append(ConsumirChar());
        else
            throw new Erro("Esperado digito Hexadecimal, recebido: " + Atual(), _local);

        while (DigitoHexadecimal(Atual()))
        {
            buffer.Append(ConsumirChar());
            if (buffer.Length >= 7)
                break;
        }

        int resultado = 0;
        try
        {
            resultado = Convert.ToInt32(buffer.ToString(), 16);
        }
        catch
        {
            throw new ErroAcessoNulo($" Não foi possível converter {buffer} para Int");
        }
        AdicionarTokenALista(TokenTipo.NumeroLiteral, resultado);
    }

    private void TokenizarNumero()
    {
        if (Atual() == '0')
        {
            if (Proximo() == 'b')
            {
                ConsumirChar();
                ConsumirChar();
                TokenizarBinario();
                return;
            }
            if (Proximo() == 'x')
            {
                ConsumirChar();
                ConsumirChar();
                TokenizarHexa();
                return;
            }
        }

        var buffer = new StringBuilder();
        var ponto = false;

        while (char.IsDigit(Atual()) || Atual() == '_')
        {
            // Ignorando underscores em números, facilita visualização
            // de números grandes. Ex: 1_000_000_000 = 1000000000
            if (Atual() == '_')
            {
                ConsumirChar();
                continue;
            }

            buffer.Append(ConsumirChar());
            if (Atual() == '.')
            {
                if (ponto)
                    throw new Erro("Número inválido!", _local);

                buffer.Append(ConsumirChar());
                ponto = true;
            }
        }
        try
        {
            if (ponto)
            {
                AdicionarTokenALista(TokenTipo.NumeroLiteral, double.Parse(buffer.ToString()));
            }
            else
            {
                AdicionarTokenALista(TokenTipo.NumeroLiteral, int.Parse(buffer.ToString()));
            }
        }
        catch
        {
            throw new ErroAcessoNulo($" Não foi possível converter {buffer} para" + (ponto ? " Real" : " Int"));
        }
    }

    private string TokenizarIdentificador()
    {
        var buffer = new StringBuilder();

        while (char.IsLetterOrDigit(Atual()) || Atual() == '_')
        {
            buffer.Append(ConsumirChar());
        }

        return buffer.ToString();
    }

    private void TokenizarPalavra()
    {
        var buffer = new StringBuilder();

        buffer.Append(TokenizarIdentificador());
        
        if(buffer.ToString() == "importar")
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
            var caminhoArquivo = new StringBuilder();

            while(Atual() != '"')
            {
                if(Atual() == '\n' || ConsumirEspacos() != 0)
                    throw new Erro("Esperado `\"`", _local);

                caminhoArquivo.Append(ConsumirChar());
            }
            ConsumirChar(); // Consumindo `"`

            ImportarArquivo(caminhoArquivo.ToString());

            return;
        }

        if(buffer.ToString() == "senao")
        {
            ConsumirEspacos();

            if(Atual() == 's' && Proximo() == 'e')
            {
                ConsumirChar();
                ConsumirChar();
                AdicionarTokenALista(TokenTipo.SenaoSe);
                return;
            }
        }
        
        if (_palavrasReservadas.ContainsKey(buffer.ToString()))
        {
            AdicionarTokenALista(_palavrasReservadas[buffer.ToString()]);
        }
        else
        {
            AdicionarTokenALista(TokenTipo.Identificador, buffer.ToString());
        }
    }

    private void TokenizarSimbolo()
    {
        var buffer = new StringBuilder();
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
                if (Proximo() == '=')
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
                if (Proximo() == '=')
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
                if (Proximo() == '=')
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
                    if (Atual() == '\\' && Proximo() == '"')
                    {
                        ConsumirChar();
                    }
                    buffer.Append(ConsumirChar());
                }

                AdicionarTokenALista(TokenTipo.TextoLiteral, buffer.ToString());
                buffer.Clear();
                ConsumirChar();
                break;
            case '\'':
                ConsumirChar();
                buffer.Append(ConsumirChar());
                AdicionarTokenALista(TokenTipo.CaractereLiteral, buffer.ToString());
                buffer.Clear();
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
            case '.':
                AdicionarTokenALista(TokenTipo.Ponto);
                ConsumirChar();
                break;
            case ':':
                AdicionarTokenALista(TokenTipo.DoisPontos);
                ConsumirChar();
                break;
            case '@':
                TokenizarAnotacao();
                break;
            case '\0':
                throw new ErroAcessoNulo(" Chegou ao fim do arquivo de forma inesperada.");
                break;
            default:
                throw new ErroTokenInvalido($"{Atual()} ASCII = {(int)Atual()}", _local);
                break;
        }
    }

    private void TokenizarAnotacao()
    {
        ConsumirChar();

        var ident = TokenizarIdentificador();

        AdicionarTokenALista(TokenTipo.Anotacao, ident);
    }

    private string ConsumirAte(char caractere)
    {
        var buffer = new StringBuilder();
        while(Atual() != caractere)
        {
            buffer.Append(ConsumirChar());

            if (Atual() == '\0')
                throw new Erro($"Esperado `{caractere}`", _local);
        }

        ConsumirChar();
        return buffer.ToString();
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
        while (Proximo() != '\0')
        {
            if (Atual() == '*' && Proximo() == '/')
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
        if (_arquivosImportados.Contains(caminho))
            return;

        string arquivoCompleto = Path.Combine(_local.CaminhoCompleto, caminho);

        if (!File.Exists(arquivoCompleto))
        {
            string appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Libra", caminho);

            if (File.Exists(appDataPath))
            {
                arquivoCompleto = appDataPath;
            }
            else
            {
                throw new ErroAcessoNulo($" Arquivo '{caminho}' não encontrado.");
            }
        }

        _arquivosImportados.Add(caminho);

        string codigoArquivo = File.ReadAllText(arquivoCompleto).ReplaceLineEndings("\n");

        var novosTokens = new Tokenizador(codigoArquivo, caminho, arquivoCompleto).Tokenizar();

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
            Console.WriteLine("    " + token.ToString());
        }
    }

    private char Atual() 
    {
        return Proximo(0);
    }
    
    private char Proximo(int offset = 1)
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