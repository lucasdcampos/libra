public enum Opcode : byte
{
    // Literais
    EMPILHAR,

    // Operações aritméticas
    SOMAR,
    SUBTRAIR,
    MULTIPLICAR,
    DIVIDIR,
    POTENCIA,
    RESTO,

    // Variáveis
    ARMAZENAR,
    CARREGAR,

    // Controle de fluxo
    SALTAR_SE_FALSO,   // Pula se o topo da pilha for falso (0 ou nulo)
    SALTAR,            // Pula incondicionalmente para um endereço

    // Encerramento do programa
    ENCERRAR
}
