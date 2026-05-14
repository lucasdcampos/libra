namespace Libra;

public struct LocalFonte
{
    public string Arquivo = "";
    public int Linha = 1;
    public string CaminhoCompleto = "";
    public LocalFonte(string caminho, string arquivo, int linha)
    {
        Arquivo = arquivo;
        Linha = linha;
        CaminhoCompleto = caminho;
    }

    public override string ToString()
    {
        return $"{Arquivo}:{Linha}";
    }
}

public class Token
{
    public Token(TokenTipo tipo, LocalFonte local, object valor = null)
    {
        Tipo = tipo;
        Valor = valor;
        Local = local;
    }

    public TokenTipo Tipo { get; private set; }
    public object Valor { get; internal set; }
    public LocalFonte Local { get; private set; }

    public override string ToString()
    {
        if(Valor != null)
        {
            return $"Token: {Tipo} | Valor: {Valor} | Arquivo {Local.Arquivo} - Linha {Local.Linha}";
        }

        return $"Token: {Tipo}";
    }

    public static string TipoParaString(TokenTipo tipo)
    {
        switch(tipo)
        {
            case TokenTipo.PontoEVirgula: return "';'";
            case TokenTipo.AbrirParen: return "'('";
            case TokenTipo.FecharParen: return "')'";
            case TokenTipo.AbrirCol: return "'['";
            case TokenTipo.FecharCol: return "']'";
            case TokenTipo.AbrirChave: return "'{'";
            case TokenTipo.FecharChave: return "'}'";
            case TokenTipo.Virgula: return "','";
            case TokenTipo.Ponto: return "'.'";
            case TokenTipo.DoisPontos: return "':'";
            case TokenTipo.OperadorComparacao: return "'=='";
            case TokenTipo.OperadorDefinir: return "'='";
            case TokenTipo.OperadorSoma: return "'+'";
            case TokenTipo.OperadorSub: return "'-'";
            case TokenTipo.OperadorMult: return "'*'";
            case TokenTipo.OperadorDiv: return "'/'";
            case TokenTipo.OperadorPot: return "'^'";
            case TokenTipo.OperadorMaiorQue: return "'>'";
            case TokenTipo.OperadorMenorQue: return "'<'";
            case TokenTipo.OperadorMaiorIgualQue: return "'>='";
            case TokenTipo.OperadorMenorIgualQue: return "'<='";
            case TokenTipo.OperadorE: return "'e'";
            case TokenTipo.OperadorOu: return "'ou'";
            case TokenTipo.OperadorDiferente: return "'!='";
            case TokenTipo.OperadorNeg: return "'! (nao)'";
            case TokenTipo.OperadorResto: return "'%'";
            case TokenTipo.TextoLiteral: return "Texto";
            case TokenTipo.NumeroLiteral: return "Número";
            case TokenTipo.CaractereLiteral: return "Caractere";
            case TokenTipo.Identificador: return "Identificador";
            case TokenTipo.Nulo: return "Nulo";
            case TokenTipo.Var: return "'var'";
            case TokenTipo.Const: return "'const'";
            case TokenTipo.Funcao: return "'funcao'";
            case TokenTipo.Classe: return "'classe'";
            case TokenTipo.Se: return "'se'";
            case TokenTipo.Senao: return "'senao'";
            case TokenTipo.Entao: return "'entao'";
            case TokenTipo.Enquanto: return "'enquanto'";
            case TokenTipo.Para: return "'para'";
            case TokenTipo.Cada: return "'cada'";
            case TokenTipo.Em: return "'em'";
            case TokenTipo.Repetir: return "'repetir'";
            case TokenTipo.Romper: return "'romper'";
            case TokenTipo.Continuar: return "'continuar'";
            case TokenTipo.Retornar: return "'retornar'";
            case TokenTipo.Tentar: return "'tentar'";
            case TokenTipo.Capturar: return "'capturar'";
            case TokenTipo.Fim: return "'fim'";
            case TokenTipo.Importar: return "'importar'";
            case TokenTipo.Como: return "'como'";
            case TokenTipo.FimDoArquivo: return "Fim do Arquivo";
            default: return tipo.ToString();
        }
    }
}