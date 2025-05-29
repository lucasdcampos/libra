// Importante manter paridade com src/Libra.VM/opcodes.h
// Definir um local centralizado para os códigos de operação da VM Libra
// Talvez um JSON?
namespace Libra;

public enum LibraVM_OP
{
    OP_PARAR = 0,
    OP_EMPILHAR,
    OP_DESEMPILHAR,
    OP_SOMAR,
    OP_SUBTRAIR,
    OP_MULTIPLICAR,
    OP_EXIBIR
}
