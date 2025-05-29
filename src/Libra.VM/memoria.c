#include <string.h>
#include <stdio.h>
#include <malloc.h>
#include "memoria.h"
#include "erro.h"

void* libra_alocar(size_t tamanho)
{
  libra_alocacoes++;
  void *ptr = malloc(tamanho);
  if (!ptr) libra_erro("Sistema sem memória disponível");
  return ptr;
}

void* libra_realocar(void* ptr, size_t size)
{
  void *novo_ptr = realloc(ptr, size);
  if (!novo_ptr) libra_erro("Sistema sem memória disponível");
  return novo_ptr;
}

void libra_copiar_mem(void* destino, void* origem, size_t num_bytes)
{
  memcpy(destino, origem, num_bytes);
}

void libra_liberar(void* ptr)
{
  libra_alocacoes--;
  free(ptr);
}