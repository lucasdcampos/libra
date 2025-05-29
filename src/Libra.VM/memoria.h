#ifndef L_MEMORIA_H
#define L_MEMORIA_H

#include <stddef.h>
extern size_t libra_alocacoes;

void* libra_alocar(size_t tamanho);
void* libra_realocar(void* ptr, size_t tamanho);
void libra_liberar(void* ptr);
void libra_copiar_mem(void* de, void* para, size_t num_bytes);

#endif // L_MEMORIA_H