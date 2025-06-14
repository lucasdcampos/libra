namespace Libra;

public enum ErroCategoria
{
    Erro, // Erro Genérico
    ErroSintaxe,
    ErroExecucao
}

public class Erro : Exception
{
    public int Codigo { get; protected set; }
    public string Mensagem { get; protected set; }
    public LocalFonte Local { get; protected set; }
    public ErroCategoria Categoria { get; protected set; }
    protected string dica;
    public Erro(string mensagem, LocalFonte local = new LocalFonte(), int codigo = 1, string dica = "")
    {
        Codigo = codigo;
        Local = local;
        Mensagem = mensagem;
        this.dica = dica;

        AtribuirCategoria();
    }

    protected Erro(int codigo, string mensagem, LocalFonte local = new LocalFonte(), string dica = "")
    {
        Codigo = codigo;
        Mensagem = mensagem;
        Local = local;
        this.dica = dica;

        AtribuirCategoria();
    }

    public void ExibirFormatado()
    {
        Console.ForegroundColor = ConsoleColor.Red;
        
        Ambiente.Msg(Categoria.ToString()+"", ": ");
        Console.ResetColor();
        Ambiente.Msg(Mensagem);
        Ambiente.Msg(string.IsNullOrEmpty(Local.Arquivo) ? "" : $"  Arquivo \"{Local.Arquivo}\", linha {Local.Linha}");

        string callStack =  Ambiente.Pilha.ObterCallStack();
        Ambiente.Msg(string.IsNullOrEmpty(callStack) ? "\n": $"  Pilha de Chamadas:\n{callStack}", "");

        if(!String.IsNullOrEmpty(dica))
        {
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Ambiente.Msg("Dica:", " ");
            Console.ResetColor();
            Ambiente.Msg(dica.Replace("\n", "\n      ")+"\n");
        }
        
    }
    
    private void AtribuirCategoria()
    {
        if (Codigo >= 1000 && Codigo < 2000)
            Categoria = ErroCategoria.ErroSintaxe;
        else if (Codigo >= 2000 && Codigo < 3000)
            Categoria = ErroCategoria.ErroExecucao;
        else
            Categoria = ErroCategoria.Erro; // Genérico
    }

    public override string ToString()
    {
        if(Local.Linha == 0)
            Local = Interpretador.LocalAtual;

        string msg = "";
        string categoria = $"{Categoria}: ";

        msg += categoria;

        if(Local.Linha == 0 || string.IsNullOrEmpty(Local.Arquivo))
            msg += $"{Mensagem}";
        else
            msg +=$"{Mensagem}\n--> `{Local.Arquivo}`, Linha: {Local.Linha}";
        
        msg += String.Concat(Enumerable.Repeat(' ', categoria.Length));

        return msg;
    }
}

public class ErroTokenInvalido : Erro
{
    public ErroTokenInvalido(string token, LocalFonte local = new LocalFonte(), string dica = "") 
        : base(1001, $"Token inválido `{token}`", local, dica) { }
}

public class ErroEsperado : Erro
{
    public ErroEsperado(TokenTipo esperado, TokenTipo recebido, LocalFonte local = new LocalFonte(), string dica = "") 
        : base(1002, $"Esperado Token {Token.TipoParaString(esperado)}, recebido {Token.TipoParaString(recebido)}", local, dica) { }
}

public class ErroDivisaoPorZero : Erro
{
    public ErroDivisaoPorZero(LocalFonte local = new LocalFonte(), string dica = "") 
        : base(2001, "Divisão por zero.", local, dica) { }
}

public class ErroVariavelNaoDeclarada : Erro
{
    public ErroVariavelNaoDeclarada(string variavel, LocalFonte local = new LocalFonte(), string dica = "Use `var {variavel}` para declarará-la, variáveis só são acessíveis no mesmo escopo.") 
        : base(2002, $"Variável não declarada `{variavel}`.", local, dica) 
        {
        }
}

public class ErroVariavelJaDeclarada : Erro
{
    public ErroVariavelJaDeclarada(string variavel, LocalFonte local = new LocalFonte(), string dica = "Não use 'var' caso queira apenas modificar o valor de uma variável.") 
        : base(2003, $"Variável já declarada `{variavel}`.", local, dica) 
        { 
        }
}

public class ErroFuncaoNaoDefinida : Erro
{
    public ErroFuncaoNaoDefinida(string variavel, LocalFonte local = new LocalFonte(), string dica = "") 
        : base(2004, $"Função não definida `{variavel}`.", local, dica) { }
}

public class ErroFuncaoJaDefinida : Erro
{
    public ErroFuncaoJaDefinida(string variavel, LocalFonte local = new LocalFonte(), string dica = "") 
        : base(2005, $"Função já definida `{variavel}`.", local, dica) { }
}

public class ErroModificacaoConstante : Erro
{
    
    public ErroModificacaoConstante(string variavel, LocalFonte local = new LocalFonte(), string dica = "Use 'var' ao invés de 'const' para declarar variáveis modificáveis.") 
        : base(2006, $"Não é possível modificar a constante `{variavel}`.", local, dica) 
        { 
        }
}

public class ErroAcessoNulo : Erro
{
    public ErroAcessoNulo(string msg = "", LocalFonte local = new LocalFonte(), string dica = "Você tentou acessar um objeto sem valor (Nulo).")
        : base(2007, $"Tentando acessar um valor Nulo.{msg}", local, dica) 
        { 
        }
}

public class ErroIndiceForaVetor : Erro
{
    public ErroIndiceForaVetor(string msg = "", LocalFonte local = new LocalFonte(), string dica = "Certifique-se de que o indice esteja entre 0 e tamanho(vetor) - 1.")
        : base(2008, $"O indice se encontra fora dos limites do Vetor. {msg}", local, dica) { }
}

public class ErroTipoIncompativel : Erro
{
    public ErroTipoIncompativel(string identificador, LocalFonte local = new LocalFonte(), string dica = "Não é possível modificar o tipo de uma variável durante a execução.")
        : base(2009, $"Atribuindo um tipo incompatível à `{identificador}`.", local, dica) 
        { 
        }
}

public class ErroConversao : Erro
{
    public ErroConversao(string tipo1, string tipo2, LocalFonte local = new LocalFonte(), string dica = "Tente adicionar uma conversão explicita.")
        : base(2010, $"Não é possível converter {tipo1} para {tipo2}.", local, dica) 
        { 
        }
}

public class ErroTransbordoDePilha : Erro
{
    public ErroTransbordoDePilha(string causador = "", LocalFonte local = new LocalFonte(), string dica = "Verifique se não há nenhuma recursão infinita.")
        : base(2011, $"Transbordo de Pilha (StackOverflow) causado por: {causador}()", local, dica) 
        { 
        }
}

public class ErroEsperadoNArgumentos : Erro
{
    public ErroEsperadoNArgumentos(string ident, int esperado, int recebido, LocalFonte local = new LocalFonte(), string dica = "")
        : base(2012, $"{ident}() esperava: {esperado} argumento(s), mas recebeu {recebido}.", local, dica) 
        { 
        }
}