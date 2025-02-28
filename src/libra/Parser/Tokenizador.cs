using System;
using System.Text.RegularExpressions;
using System.Linq;

namespace Libra;

public class Tokenizador 
{
    private int _posicao;
    private string? _fonte;
    private List<Token>? _tokens;
    private List<string> _arquivosImportados;
    private int _linha;
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
        { "retornar", TokenTipo.Retornar },
        { "entao", TokenTipo.Entao },
        { "fim", TokenTipo.Fim },
        { "nulo", TokenTipo.Nulo },
        { "ou", TokenTipo.OperadorOu },
        { "e", TokenTipo.OperadorE },
        { "neg", TokenTipo.OperadorNeg },
        { "nao", TokenTipo.OperadorNeg }
    };

    public List<Token> Tokenizar(string source) 
    {
        _fonte = source;
        _tokens = new();
        _arquivosImportados = new();
        _linha = 1;
        _posicao = 0;
        
        PreTokenizar();

        var texto = "";
        try
        {
            while (Atual() != '\0')
            {
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
            
        }

        return null;
    }

    private void PreTokenizar()
    {
        // Expressão regular para encontrar as declarações de importação
        var regex = new Regex(@"importar\s+""([^""]+)""");

        // Obtém o caminho do diretório do executável/biblioteca
        string caminhoExecutavel = AppDomain.CurrentDomain.BaseDirectory;

        // Obtém o caminho do diretório atual do usuário
        string caminhoAtual = Directory.GetCurrentDirectory();
        // Enquanto houver declarações de importação na string _fonte
        Match match;
        while ((match = regex.Match(_fonte)).Success)
        {
            // Captura o nome do arquivo a ser importado
            string nomeArquivo = match.Groups[1].Value;

            // Verifica se o arquivo já foi importado
            if (!_arquivosImportados.Contains(nomeArquivo))
            {
                // Tenta encontrar o arquivo no caminho do executável/biblioteca
                string caminhoCompleto = Path.Combine(caminhoExecutavel+"/biblioteca/", nomeArquivo);

                // Se o arquivo não existir no caminho do executável, tenta no caminho atual do usuário
                if (!File.Exists(caminhoCompleto))
                {
                    caminhoCompleto = Path.Combine(caminhoAtual, nomeArquivo);
                }

                // Verifica se o arquivo existe em algum dos caminhos
                if (File.Exists(caminhoCompleto))
                {
                    // Lê o conteúdo do arquivo
                    string conteudoArquivo = File.ReadAllText(caminhoCompleto);

                    // Substitui a declaração de importação pelo conteúdo do arquivo
                    _fonte = _fonte.Replace(match.Value, conteudoArquivo);

                    // Adiciona o arquivo à lista de arquivos importados
                    _arquivosImportados.Add(nomeArquivo);
                }
                else
                {
                    // Se o arquivo não for encontrado, remove a declaração de importação
                    _fonte = _fonte.Replace(match.Value, string.Empty);

                    // Opcional: Lançar uma exceção ou registrar um aviso
                    throw new FileNotFoundException($"Arquivo não encontrado: {nomeArquivo}");
                }
            }
            else
            {
                // Se o arquivo já foi importado, remove a declaração de importação
                _fonte = _fonte.Replace(match.Value, string.Empty);
            }
        }
    }

    private void TokenizarNumero()
    {
        string buffer = "";
        var ponto = false;
        while (char.IsDigit(Atual()))
        {
            buffer += ConsumirChar();
            if (Atual() == '.')
            {
                if (ponto)
                    throw new Erro("Número inválido!", _linha);

                buffer += ConsumirChar();
                ponto = true;
            }
        }

        if (ponto)
            AdicionarTokenALista(TokenTipo.NumeroLiteral, double.Parse(buffer));
        else
            AdicionarTokenALista(TokenTipo.NumeroLiteral, int.Parse(buffer));
    }

    private void TokenizarPalavra()
    {
        string buffer = "" + ConsumirChar();

        while (char.IsLetterOrDigit(Atual()) || Atual() == '_')
        {
            buffer += ConsumirChar();
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
                Passar();
                break;
            case '\n':
                _linha++;
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
            case '{':
                ConsumirChar();
                AdicionarTokenALista(TokenTipo.AbrirChave);
                break;
            case '}':
                AdicionarTokenALista(TokenTipo.FecharChave);
                Passar();
                break;
            case '>':
                if (Proximo(1) == '=')
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
                if (Proximo(1) == '=')
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
                if (Proximo(1) == '=')
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
            case '%':
                AdicionarTokenALista(TokenTipo.OperadorResto);
                Passar();
                break;
            case ')':
                AdicionarTokenALista(TokenTipo.FecharParen);
                Passar();
                break;
            case '=':
                ConsumirChar();

                if (Atual() == '=')
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

                while (Atual() != '"')
                {
                    if (Atual() == '\\' && Proximo(1) == '"')
                    {
                        Passar();
                    }
                    buffer += ConsumirChar();
                }

                AdicionarTokenALista(TokenTipo.TextoLiteral, buffer);
                buffer = "";
                Passar();
                break;
            case '\'':
                Passar();
                buffer += ConsumirChar();
                AdicionarTokenALista(TokenTipo.CaractereLiteral, buffer);
                buffer = "";
                if (Atual() != '\'')
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
                throw new ErroTokenInvalido($"{Atual()} - ASCII = {(int)Atual()}", _linha);
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
                throw new Erro($"Esperado `{caractere}`", _linha);
        }

        Passar();
        return buffer;
    }

    private void ConsumirEspacosVazios()
    {
        while(Atual() == ' ' || Atual() == '\n')
        {
            Passar();
        }
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