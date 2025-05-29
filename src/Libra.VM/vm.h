#ifndef L_VM_H
#define L_VM_H

#include "erro.h"
#include "memoria.h"

typedef struct
{
    int* pilha;
    int* codigo;
    size_t sp; // stack pointer
    size_t pc; // program counter
    size_t tam_cod;
    size_t tam_pilha;
} LibraVM;

typedef enum
{
    OP_PARAR = 0,
    OP_EMPILHAR,
    OP_DESEMPILHAR,
    OP_SOMAR,
    OP_SUBTRAIR,
    OP_MULTIPLICAR,
    OP_EXIBIR
} LibraVM_OP;


void libra_vm_iniciar(LibraVM* vm, const size_t tam_pilha);
void libra_vm_carregar_prog(LibraVM* vm, int* codigo, const size_t tam_cod);
void libra_vm_limpar(LibraVM* vm);
void libra_vm_executar(LibraVM* vm);
int libra_vm_proximo_byte(LibraVM* vm);

int libra_vm_topo_pilha(LibraVM* vm);
int libra_vm_pilha_cheia(LibraVM* vm);
int libra_vm_pilha_vazia(LibraVM* vm);
const char* libra_nome_instrucao(int op);
void libra_vm_salvar_bytecode(const char* nome_arquivo, int* codigo, size_t tam_cod);
int* libra_vm_carregar_bytecode(const char* nome_arquivo, size_t* tam_cod);

#endif // L_VM_H