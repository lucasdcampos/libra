# Testes da Linguagem Libra

Este diretório contém as baterias de testes automatizados para a linguagem Libra, divididas entre testes de funcionalidade (core) e testes de desempenho (performance).

## Estrutura do Diretório

-   **`bateria/`**: Conjunto de testes funcionais para garantir que a gramática, o motor e as bibliotecas padrão estão funcionando conforme o esperado.
    -   `runner.libra`: Orquestrador principal dos testes core.
    -   `core/`: Testes de variáveis, loops, condicionais e expressões.
    -   `objetos/`: Testes de classes, métodos, escopo e recursividade.
    -   `biblioteca/`: Testes das funções nativas (`matematica.libra`, `vetores.libra`, etc).
-   **`performance/`**: Benchmarks focados em medir a velocidade de execução do interpretador.
    -   `runner.libra`: Orquestrador principal dos benchmarks.
    -   `bench_loops.libra`: Desempenho de laços e aritmética.
    -   `bench_recursao.libra`: Desempenho de chamadas recursivas profundas.
    -   `bench_objetos.libra`: Desempenho de criação de instâncias e acesso a membros.

## Como Executar os Testes

Todos os testes são executados através do CLI da Libra.

### 1. Testes Funcionais (Recomendado para cada mudança)
Para garantir que nenhuma funcionalidade foi quebrada:
```bash
dotnet run --project src/Libra.CLI -- testes/bateria/runner.libra
```

### 2. Testes de Performance
Para medir o impacto de mudanças na eficiência do motor:
```bash
dotnet run --project src/Libra.CLI -- testes/performance/runner.libra
```

## Adicionando Novos Testes

### Testes Funcionais
1. Crie um novo arquivo `.libra` dentro da subpasta apropriada em `bateria/`.
2. Defina suas funções de teste e uma função `executar()`.
3. Importe seu novo arquivo no `testes/bateria/runner.libra` e adicione a chamada `seu_modulo.executar()` dentro do bloco `tentar`.

### Testes de Performance
1. Crie um novo arquivo em `performance/` seguindo o padrão de medir o tempo com `tempo.agora()`.
2. Adicione-o ao `testes/performance/runner.libra`.

---
*Nota: Estes testes utilizam o interpretador atual. Os resultados de performance podem variar drasticamente após a implementação completa da VM de Bytecode.*
