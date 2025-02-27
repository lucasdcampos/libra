# Libra
&copy; 2024 - 2025 Lucas M. Campos

Libra é uma linguagem de programação simples com a sintaxe em **português**! 

Libra foi projetada para ser fácil de entender e utilizar, especialmente para iniciantes. A sintaxe da linguagem é inspirada em linguagens populares, mas com um diferencial: as palavras-chave e comandos estão em português, facilitando ainda mais o aprendizado para falantes da língua portuguesa.

## Exemplo de Código

Aqui está um exemplo simples de um programa em Libra:

```js
// Declarando uma função
funcao fibonacci(n)
    var a = 0
    var b = 1
    var contador = 0
    var temp = 0
    enquanto contador < n faca
        exibir(a)
        temp = a
        a = b
        b = temp + b
        contador = contador+1
    fim
fim
// Exibir os 10 primeiros números de Fibonacci
fibonacci(10)
```

## Compilando a Libra

Para compilar o projeto, siga as instruções abaixo:

1. **Garanta que você tenha o .NET SDK instalado:** Antes de compilar o projeto, certifique-se de que você tenha o .NET Core SDK instalado em sua máquina. Você pode baixar o SDK no seguinte link: [https://dotnet.microsoft.com/en-us/download](https://dotnet.microsoft.com/en-us/download).

2. **Compilando o Projeto:**:

```
git clone https://github.com/lucasdcampos/libra.git
cd libra
./scripts/build.ps1 (ou build.sh em Linux)
```

## Executando um Programa Libra

Você pode usar o executável `libra.exe` ou `libra` gerado durante a compilação do projeto para executar um programa Libra.
Para isso, use o seguinte comando:

> **Nota:** Coloque o caminho para o executável nas variáveis de ambiente do seu sistema para facilitar a execução de programas Libra.

## Melhorando a Experiência de Desenvolvimento
Para facilitar a vida do desenvolvedor, há uma extensão no Visual Studio Code que adiciona Suporte à Libra, adicionando Syntax Highlighting e outras melhorias. 
Baixe-a em https://marketplace.visualstudio.com/items?itemName=LucasMCampos.libra.

## Como contribuir?

Se você deseja contribuir para o projeto, pode fazer um *fork* do repositório, depois cloná-lo para sua máquina local com o seguinte comando:
`git clone https://github.com/<SEU_USUARIO>/libra.git`

Contribua para o código, depois crie um *Pull Request*. Caso for uma contribuição válida, a aceitaremos ao projeto.

**Dúvidas?**

Me mande um e-mail: <a href="mailto:lucas.campos44@fatec.sp.gov.br">lucasm.campos@hotmail.com.br</a>
ou entre no Discord: https://discord.gg/mnGkSD4CsA
