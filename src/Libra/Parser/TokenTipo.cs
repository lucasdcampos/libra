public enum TokenTipo
{
    
    // Tipos
    NumeroLiteral,
    CaractereLiteral,
    TextoLiteral,
    Identificador,
    Anotacao,
    Vetor,
    Nulo,                       // Nulo
    TokenInvalido,

    // Operadores
    OperadorSoma,               // +
    OperadorSub,                // -
    OperadorMult,               // *
    OperadorDiv,                // /
    OperadorPot,                // ^
    OperadorComparacao,         // ==
    OperadorDefinir,            // =
    OperadorMaiorQue,           // >
    OperadorMenorQue,           // <
    OperadorMaiorIgualQue,      // >=
    OperadorMenorIgualQue,      // <=
    OperadorE,                  // e
    OperadorOu,                 // ou
    OperadorDiferente,          // !=
    OperadorNeg,                // nao, neg, !
    OperadorResto,              // %

    // Simbolos
    AbrirParen,                 // (
    FecharParen,                // )
    AbrirCol,                   // [
    FecharCol,                  // ]
    AbrirChave,                 // {
    FecharChave,                // }
    PontoEVirgula,              // ;
    Virgula,                    // ,
    Ponto,                      // .
    DoisPontos,                 // :
    FimDoArquivo,               // \0

    // Palavras Reservadas
    Var,                        // var
    Const,                      // const
    Funcao,                     // funcao
    Classe,                     // classe
    Se,                         // se
    Senao,                      // senao
    SenaoSe,                    // senao se
    Entao,                      // entao
    Enquanto,                   // enquanto
    Para,                       // para
    Cada,                       // cada
    Em,                         // em
    Repetir,                    // repetir
    Romper,                     // romper
    Continuar,                  // continuar
    Retornar,                   // retornar
    Tentar,                     // tentar
    Capturar,                   // capturar
    Fim,                        // fim
}