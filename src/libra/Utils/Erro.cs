namespace Libra;
public class Erro
{
    public int Codigo { get; protected set; }
    public string Mensagem { get; protected set; }
    public int Linha { get; protected set; }
    public int Coluna { get; protected set; }

    public Erro(string mensagem, int linha = 0, int codigo = 1)
    {
        Codigo = codigo;
        Linha = linha;
        Mensagem = mensagem;
    }

    protected Erro(int codigo, string mensagem, int linha = 0, int coluna = 0)
    {
        Codigo = codigo;
        Mensagem = mensagem;
        Linha = linha;
        Coluna = coluna;
    }

    public void LancarErro()
    {
        Console.ForegroundColor = ConsoleColor.Red;

        Console.WriteLine(this.ToString());

        Console.ResetColor();

        Environment.Exit(this.Codigo);
    }

    // TODO: Arrumar essa feiura
    public override string ToString()
    {
        string msg = "";
        string categoria = "";

        if(Codigo >= 1000 && Codigo < 2000)
            categoria = "ErroSintaxe: ";
        else if(Codigo >= 2000 && Codigo < 3000)
            categoria = "ErroExecucao: ";
        else
            categoria = "Erro: ";

        msg += categoria;

        if(Linha == 0)
            msg += $"{Mensagem}";
        else
            msg +=$"{Mensagem} na linha {Linha}";
        
        string arrows = "";

        foreach(var c in Mensagem)
            arrows += "^";
        

        msg += Environment.NewLine + String.Concat(Enumerable.Repeat(' ', categoria.Length)) + arrows;

        return msg;
    }
}

public class ErroTokenInvalido : Erro
{
    public ErroTokenInvalido(string token, int linha = 0, int coluna = 0) 
        : base(1001, $"Token inválido `{token}`", linha, coluna) { }
}

public class ErroEsperado : Erro
{
    public ErroEsperado(string token, int linha = 0, int coluna = 0) 
        : base(1002, $"Esperado Token {token}", linha, coluna) { }
}

public class ErroDivisaoPorZero : Erro
{
    public ErroDivisaoPorZero(int linha = 0, int coluna = 0) 
        : base(2001, "Divisão por zero.", linha, coluna) { }
}

public class ErroVariavelNaoDeclarada : Erro
{
    public ErroVariavelNaoDeclarada(string variavel, int linha = 0, int coluna = 0) 
        : base(2002, $"Variável não declarada `{variavel}`.", linha, coluna) { }
}

public class ErroVariavelJaDeclarada : Erro
{
    public ErroVariavelJaDeclarada(string variavel, int linha = 0, int coluna = 0) 
        : base(2003, $"Variável já declarada `{variavel}`.", linha, coluna) { }
}

public class ErroFuncaoNaoDefinida : Erro
{
    public ErroFuncaoNaoDefinida(string variavel, int linha = 0, int coluna = 0) 
        : base(2004, $"Função não definida `{variavel}`.", linha, coluna) { }
}

public class ErroFuncaoJaDefinida : Erro
{
    public ErroFuncaoJaDefinida(string variavel, int linha = 0, int coluna = 0) 
        : base(2005, $"Função já definida `{variavel}`.", linha, coluna) { }
}

public class ErroModificacaoConstante : Erro
{
    public ErroModificacaoConstante(string variavel, int linha = 0, int coluna = 0) 
        : base(2006, $"Não é possível modificar a constante `{variavel}`.", linha, coluna) { }
}