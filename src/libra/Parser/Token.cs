public enum TokenTipo
{
    
    // Tipos
    NumeroLiteral,
    StringLiteral,
    Identificador,
    TokenInvalido,

    // Operadores
    OperadorSoma,
    OperadorSub,
    OperadorMult, 
    OperadorDiv,
    OperadorComparacao,
    OperadorDefinir,

    // Simbolos
    AbrirParen, 
    FecharParen,
    PontoEVirgula,
    FimDoArquivo,

    // Palavras Reservadas
    Sair, 
    Var,
    Exibir
    
}

public class Token 
{
    public Token(TokenTipo tipo, object valor = null) 
    {
        Tipo = tipo;
        Valor = valor;
    }

    public TokenTipo Tipo { get; private set; }
    public object Valor { get; private set; }

    public override string ToString()
    {
        if(Valor != null)
        {
            return $"Token: {Tipo.ToString()} | Valor: {Valor}";
        }

        return $"Token: {Tipo.ToString()}";
    }

}