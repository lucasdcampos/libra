#include <stdio.h>
#include <stdlib.h>
#include "vm.h"

#define DEBUG_VM 0

void libra_vm_iniciar(LibraVM* vm, const size_t tam_pilha)
{
    vm->tam_pilha = tam_pilha;
    vm->sp = 0;
    vm->pc = 0;
    vm->pilha = (int*)libra_alocar(tam_pilha * sizeof(int));
    vm->codigo = NULL;
    vm->tam_cod = 0;
}

void libra_vm_carregar_prog(LibraVM* vm, int* codigo, const size_t tam_cod)
{
    vm->codigo = codigo;
    vm->tam_cod = tam_cod;
    vm->pc = 0;
}

void libra_vm_exibir_estado(const LibraVM* vm)
{
    printf("\n===== Estado da LVM =====\n");
    printf("PC: %zu\n", vm->pc);
    printf("SP: %zu\n", vm->sp);

    printf("Pilha:\n");
    if (vm->sp == 0)
    {
        printf("  [Vazia]\n");
    }
    else
    {
        for (size_t i = 0; i < vm->sp; i++)
        {
            printf("  [%zu]: %d\n", i, vm->pilha[i]);
        }
    }

    printf("Código Carregado:\n");
    for (size_t i = 0; i < vm->tam_cod; i++)
    {
        const char* nome_instrucao = libra_nome_instrucao(vm->codigo[i]);
        if (nome_instrucao)
        {
            printf("  [%zu]: %s (%d)\n", i, nome_instrucao, vm->codigo[i]);
        }
        else
        {
            printf("  [%zu]: %d (Desconhecido)\n", i, vm->codigo[i]);
        }
    }

    printf("=====================================\n");
}


void libra_vm_salvar_bytecode(const char* nome_arquivo, int* codigo, size_t tam_cod)
{
    FILE* arquivo = fopen(nome_arquivo, "wb");
    if (!arquivo)
    {
        libra_erro("Não foi possível ler o arquivo");
    }

    if (fwrite(codigo, sizeof(int), tam_cod, arquivo) != tam_cod)
    {
        fclose(arquivo);
        libra_erro("Não foi possível escrever bytecode no arquivo");
    }

    fclose(arquivo);
}

int* libra_vm_carregar_bytecode(const char* nome_arquivo, size_t* tam_cod)
{
    FILE* arquivo = fopen(nome_arquivo, "rb");
    if (!arquivo)
    {
        libra_erro("Não foi possível ler o arquivo");
    }

    fseek(arquivo, 0, SEEK_END);
    size_t tamanho_arquivo = ftell(arquivo);
    rewind(arquivo);

    if (tamanho_arquivo % sizeof(int) != 0)
    {
        fclose(arquivo);
        libra_erro("Arquivo inválido");
        exit(EXIT_FAILURE);
    }

    *tam_cod = tamanho_arquivo / sizeof(int);
    int* codigo = (int*)libra_alocar(tamanho_arquivo);
    
    if (fread(codigo, sizeof(int), *tam_cod, arquivo) != *tam_cod)
    {
        fclose(arquivo);
        libra_erro("Erro ao ler bytecode do arquivo");
        libra_liberar(codigo);
    }

    fclose(arquivo);
    return codigo;
}

void libra_vm_limpar(LibraVM* vm)
{
    libra_liberar(vm->pilha);
    libra_liberar(vm->codigo);
}

int libra_vm_proximo_byte(LibraVM* vm)
{
    if (vm->pc >= vm->tam_cod)
    {
        return -1;
    }
    return vm->codigo[vm->pc++];
}

int libra_vm_topo_pilha(LibraVM* vm)
{
    if (vm->sp == 0)
    {
        libra_erro("Pilha vazia\n");
    }
    return vm->pilha[vm->sp - 1];
}

int libra_vm_pilha_cheia(LibraVM* vm)
{
    return vm->sp == vm->tam_pilha;
}

int libra_vm_pilha_vazia(LibraVM* vm)
{
    return vm->sp == 0;
}

void libra_vm_empilhar(LibraVM* vm, int valor)
{
    if (libra_vm_pilha_cheia(vm))
    {
        libra_erro("Pilha cheia\n");
    }
    vm->pilha[vm->sp++] = valor;
}

int libra_vm_desempilhar(LibraVM* vm)
{
    if (libra_vm_pilha_vazia(vm))
    {
        libra_erro("Pilha vazia\n");
    }
    return vm->pilha[--vm->sp];
}

void libra_vm_executar(LibraVM* vm)
{
    int instrucao;
    int a, b;

    while ((instrucao = libra_vm_proximo_byte(vm)) != -1)
    {
        if(DEBUG_VM)
            libra_vm_exibir_estado(vm);

        switch (instrucao)
        {
            case OP_PARAR:
                return;

            case OP_EMPILHAR:
                a = libra_vm_proximo_byte(vm);
                libra_vm_empilhar(vm, a);
                break;

            case OP_DESEMPILHAR:
                a = libra_vm_desempilhar(vm);
                break;

            case OP_SOMAR:
                if (libra_vm_pilha_vazia(vm))
                {
                    libra_erro("Pilha vazia para soma\n");
                }
                b = libra_vm_desempilhar(vm);
                a = libra_vm_desempilhar(vm);
                libra_vm_empilhar(vm, a + b);
                break;

            case OP_SUBTRAIR:
                if (libra_vm_pilha_vazia(vm))
                {
                    libra_erro("Pilha vazia para subtração\n");
                }
                b = libra_vm_desempilhar(vm);
                a = libra_vm_desempilhar(vm);
                libra_vm_empilhar(vm, a - b);
                break;

            case OP_MULTIPLICAR:
                if (libra_vm_pilha_vazia(vm))
                {
                    libra_erro("Pilha vazia para multiplicação\n");
                }
                b = libra_vm_desempilhar(vm);
                a = libra_vm_desempilhar(vm);
                libra_vm_empilhar(vm, a * b);
                break;

            case OP_EXIBIR:
                a = libra_vm_topo_pilha(vm);
                printf("%d\n", a);
                break;

            default:
                libra_erro("Instrução desconhecida");
                return;
        }
    }
}

const char* libra_nome_instrucao(int op)
{
    switch (op)
    {
        case OP_PARAR: return "PARAR";
        case OP_EMPILHAR: return "EMPILHAR";
        case OP_DESEMPILHAR: return "DESEMPILHAR";
        case OP_SOMAR: return "SOMAR";
        case OP_SUBTRAIR: return "SUBTRAIR";
        case OP_MULTIPLICAR: return "MULTIPLICAR";
        case OP_EXIBIR: return "EXIBIR";
        default: return NULL;
    }
}