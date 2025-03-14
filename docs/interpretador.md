[Documentação](README.md) -> [Interpretador](#)

# Interpretador
O **Interpretador** é o programa responsável por ler o código-fonte escrito na linguagem **Libra** e executar as instruções contidas nesse código.
Ele interpreta o programa linha por linha, realizando as ações especificadas sem a necessidade de compilar o código antes da execução.

## Modo Interativo
No modo interativo, o Interpretador é aberto sem receber argumentos e permite que o usuário digite comandos diretamente no Shell da Libra. Esse modo é útil para testar trechos curtos de código e explorar a linguagem sem a necessidade de criar arquivos completos.

### Como usar o Modo Interativo
Abra o Interpretador sem passar nenhum argumento.
O Shell da Libra será iniciado, e você poderá digitar comandos diretamente.
Execute pequenos testes e veja o resultado imediatamente.

**Exemplo:**
```
$ libra
Bem-vindo à Libra 0.1.0.0
Digite "ajuda", "licenca" ou uma instruçao.
>>> exibir("Olá, Libra")
Olá, Libra!
>>> 2^10
1024
>>> raizq(64)
8
```
Você pode passar o caminho de um arquivo no modo interativo para ele ser Interpretado linha a linha:
```
>>> meuPrograma.libra
```

Há uma forma melhor de interpretar arquivos diretamente, fora do modo interativo, digite no terminal:
```
libra meuPrograma.libra
```
O Interpretador irá ler o arquivo **meuPrograma.libra** e executar as instruções definidas nele, sem entrar no modo interativo.

## Resumo
**Modo Interativo:** Inicie o Interpretador sem argumentos para digitar e testar código diretamente no Shell da Libra. <br>
**Interpretar Arquivo:** Para executar um arquivo de código-fonte, use libra nomeDoArquivo.libra ao iniciar o Interpretador. <br>
**Prático para testes:** O modo interativo é útil para realizar pequenos testes e experimentações rapidamente. <br>

Próximo Capítulo: [Tipos de Dados](valores.md)
