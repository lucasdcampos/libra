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
    public LocalToken Local { get; protected set; }
    public ErroCategoria Categoria { get; protected set; }

    public Erro(string mensagem, LocalToken local = new LocalToken(), int codigo = 1)
    {
        Codigo = codigo;
        Local = local;
        Mensagem = mensagem;

        AtribuirCategoria();
        LancarErro();
    }

    protected Erro(int codigo, string mensagem, LocalToken local = new LocalToken(), int coluna = 0)
    {
        Codigo = codigo;
        Mensagem = mensagem;
        Local = local;

        AtribuirCategoria();
        LancarErro();
    }

    internal void LancarErro()
    {
        Console.ForegroundColor = ConsoleColor.Red;
        
        Ambiente.Msg(this.ToString());
        
        Console.ResetColor();
        
        Ambiente.Encerrar(this.Codigo);
    }

    public static void MensagemBug(Exception e)
    {
        if (e is ExcecaoSaida)
            return;
            
        if (e is Erro)
        {
            Ambiente.Msg(e.ToString());
            return;
        }

        string logsDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
        string logFile = Path.Combine(logsDir, $"erro-{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.txt");

        if (!Directory.Exists(logsDir))
        {
            Directory.CreateDirectory(logsDir);
        }

        string mensagemLog = "Ocorreu um erro interno na Libra, veja a descrição para mais detalhes:\n";
        mensagemLog += "Versão: Libra 1.0.0-Beta\n";
        mensagemLog += $"Ultima local do Script Libra executada: {Interpretador.LocalAtual}\n";
        mensagemLog += $"Problema:\n{e.ToString()}\n";
        mensagemLog += "Por favor reportar em https://github.com/lucasdcampos/libra/issues/ (se possível incluir script que causou o problema)\n";

        File.WriteAllText(logFile, mensagemLog);

        Ambiente.Msg("\nHouve um problema, mas não foi culpa sua :(");
        Ambiente.Msg($"Uma descrição do erro foi salva em: {logFile}");
        Ambiente.Msg("Por favor reportar em https://github.com/lucasdcampos/libra/issues/");
        Ambiente.Msg($"Versão: Libra {LibraUtil.VersaoAtual()}"); // TODO: Não deixar a versão hardcoded dessa forma
        Ambiente.Msg("\nImpossível continuar, encerrando a execução do programa.\n");

    }

    internal void Dica(string msg)
    {
        Ambiente.Msg($"Dica: {msg}");
    }

    private void AtribuirCategoria()
    {
        if(Codigo >= 1000 && Codigo < 2000)
            Categoria = ErroCategoria.ErroSintaxe;
        else if(Codigo >= 2000 && Codigo < 3000)
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
    public ErroTokenInvalido(string token, LocalToken local = new LocalToken(), int coluna = 0) 
        : base(1001, $"Token inválido `{token}`", local, coluna) { }
}

public class ErroEsperado : Erro
{
    public ErroEsperado(TokenTipo esperado, TokenTipo recebido, LocalToken local = new LocalToken(), int coluna = 0) 
        : base(1002, $"Esperado Token {Token.TipoParaString(esperado)}, recebido {Token.TipoParaString(recebido)}", local, coluna) { }
}

public class ErroDivisaoPorZero : Erro
{
    public ErroDivisaoPorZero(LocalToken local = new LocalToken(), int coluna = 0) 
        : base(2001, "Divisão por zero.", local, coluna) { }
}

public class ErroVariavelNaoDeclarada : Erro
{
    public ErroVariavelNaoDeclarada(string variavel, LocalToken local = new LocalToken(), int coluna = 0) 
        : base(2002, $"Variável não declarada `{variavel}`.", local, coluna) 
        {
            Dica($"Use `var {variavel}` para declarará-la, variáveis só são acessíveis no mesmo escopo.");
        }
}

public class ErroVariavelJaDeclarada : Erro
{
    public ErroVariavelJaDeclarada(string variavel, LocalToken local = new LocalToken(), int coluna = 0) 
        : base(2003, $"Variável já declarada `{variavel}`.", local, coluna) 
        { 
            Dica("Não use 'var' caso queira apenas modificar o valor de uma variável.");
        }
}

public class ErroFuncaoNaoDefinida : Erro
{
    public ErroFuncaoNaoDefinida(string variavel, LocalToken local = new LocalToken(), int coluna = 0) 
        : base(2004, $"Função não definida `{variavel}`.", local, coluna) { }
}

public class ErroFuncaoJaDefinida : Erro
{
    public ErroFuncaoJaDefinida(string variavel, LocalToken local = new LocalToken(), int coluna = 0) 
        : base(2005, $"Função já definida `{variavel}`.", local, coluna) { }
}

public class ErroModificacaoConstante : Erro
{
    public ErroModificacaoConstante(string variavel, LocalToken local = new LocalToken(), int coluna = 0) 
        : base(2006, $"Não é possível modificar a constante `{variavel}`.", local, coluna) 
        { 
            Dica("Use 'var' ao invés de 'const' para declarar variáveis modificáveis.");
        }
}

public class ErroAcessoNulo : Erro
{
    public ErroAcessoNulo(string msg = "", LocalToken local = new LocalToken(), int coluna = 0)
        : base(2007, $"Tentando acessar um valor Nulo.{msg}", local, coluna) 
        { 
            Dica("Você tentou acessar um objeto sem referência (Nulo).");
        }
}

public class ErroIndiceForaVetor : Erro
{
    public ErroIndiceForaVetor(string msg = "", LocalToken local = new LocalToken(), int coluna = 0)
        : base(2008, $"O indice se encontra fora dos limites do Vetor. {msg}", local, coluna) { }
}

public class ErroTipoIncompativel : Erro
{
    public ErroTipoIncompativel(string identificador, LocalToken local = new LocalToken(), int coluna = 0)
        : base(2009, $"Atribuindo um tipo incompatível à `{identificador}`.", local, coluna) 
        { 
            Dica("Não é possível modificar o tipo de uma variável durante a execução.");
        }
}

public class ErroTransbordoDePilha : Erro
{
    public ErroTransbordoDePilha(string causador = "", LocalToken local = new LocalToken(), int coluna = 0)
        : base(2010, $"Transbordo de Pilha (StackOverflow) causado por: {causador}()", local, coluna) 
        { 
            Dica("Verifique se não há nenhuma recursão infinita.");
        }
}

public class ErroEsperadoNArgumentos : Erro
{
    public ErroEsperadoNArgumentos(string ident, int esperado, int recebido, LocalToken local = new LocalToken(), int coluna = 0)
        : base(2010, $"{ident}() esperava: {esperado} argumento(s), mas recebeu {recebido}.", local, coluna) 
        { 
        }
}