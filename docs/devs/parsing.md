# Parsing (Análise Sintática)
O **Parser** é a etapa que transforma a lista plana de tokens gerada pelo Tokenizador em uma estrutura hierárquica chamada **Árvore de Sintaxe Abstrata (AST)**.

## AST (Abstract Syntax Tree)
A AST representa a estrutura lógica do programa. Cada nó da árvore corresponde a uma construção da linguagem (como uma declaração de variável, um loop `se`, ou uma chamada de função).

As definições dos nós da AST estão localizadas em `src/Libra/Arvore/`.

## O Parser (`Parser.cs`)
A Libra utiliza um parser de **Descida Recursiva** (Recursive Descent Parser). Ele percorre a lista de tokens e tenta "encaixá-los" em regras gramaticais, chamando métodos recursivamente para processar sub-expressões.

### Funcionamento Geral
O ponto de entrada é o método `Parse()`, que retorna um objeto `Programa` contendo uma lista de instruções.

```cs
public Programa Parse()
{
    return new Programa(Instrucoes(TokenTipo.FimDoArquivo));
}
```

### Regras de Precedência
O Parser lida com a precedência de operadores matemáticos e lógicos utilizando uma tabela de prioridades e uma técnica de escalonamento. Os operadores de maior prioridade (como `*` e `/`) são agrupados antes dos de menor prioridade (como `+` e `-`).

A tabela de precedência atual segue o padrão de mercado:
1.  **Potenciação** (`^`)
2.  **Multiplicação, Divisão e Resto** (`*`, `/`, `%`)
3.  **Soma e Subtração** (`+`, `-`)
4.  **Comparação** (`>`, `<`, `>=`, `<=`)
5.  **Igualdade** (`==`, `!=`)
6.  **Lógica E** (`e`)
7.  **Lógica OU** (`ou`)

### Principais Métodos
-   **`ParseInstrucao`**: O grande "maestro" que decide qual tipo de instrução processar com base no token atual (`var`, `se`, `enquanto`, etc).
-   **`Expressao`**: Lida com toda a lógica de expressões binárias e unárias, respeitando a precedência.
-   **`Primaria`**: Processa os elementos base das expressões, como literais, variáveis entre parênteses e chamadas de função.
-   **`ConsumirToken`**: Avança para o próximo token se o atual for do tipo esperado, ou lança um erro de sintaxe amigável caso contrário.

## Tratamento de Erros de Sintaxe
Graças às melhorias recentes, o Parser lança erros específicos que ajudam o desenvolvedor a identificar rapidamente o que está errado. Em vez de mensagens genéricas, o Parser agora informa exatamente qual token era esperado e o que foi encontrado, muitas vezes acompanhado de uma **Dica** de correção.

Após a geração da AST, o programa está pronto para ser executado pelo **Interpretador**. Veja [Interpretador](interpretador.md) para mais detalhes.
