#include <stdlib.h>
#include <stdio.h>
#include "erro.h"

void libra_erro(char* msg)
{
    printf("Erro: %s", msg);
    exit(1);
}