<h1 align=center>Libra 🇧🇷</h1>
<p align=center><strong>A linguagem brasileira</strong></p>
<br>

> **⚠️ Aviso:** Libra ainda está em desenvolvimento e não há nenhuma versão disponível publicamente. É possível criar apenas programas extremamentes simples por enquanto.

<hr>

Libra é uma linguagem de programação simples, criada exclusivamente para ser um projeto acadêmico. Este projeto tem como objetivo principal o aprendizado e a experimentação no desenvolvimento de linguagens de programação. O código fonte da linguagem estará disponível publicamente para consulta, uso e contribuição.

## Visão Geral

Libra foi projetada para ser fácil de entender e utilizar, especialmente para iniciantes. A sintaxe da linguagem é inspirada em linguagens populares, mas com um diferencial: as palavras-chave e comandos estão em português, facilitando ainda mais o aprendizado para falantes da língua portuguesa.

## Características

- **Sintaxe Simples:** A linguagem possui uma sintaxe intuitiva e limpa.
- **Palavras-Chave em Português:** Todas as palavras-chave e comandos estão em português.
- **Código Aberto:** O código fonte da Libra está disponível publicamente.

## Construíndo a DLL e o Interpretador

Para compilar o projeto, siga as instruções abaixo:

1. **Garanta que você tenha o .NET SDK instalado:** Antes de compilar o projeto, certifique-se de que você tenha o .NET Core SDK instalado em sua máquina. Você pode baixar o SDK no seguinte link: [https://dotnet.microsoft.com/en-us/download](https://dotnet.microsoft.com/en-us/download).

2. **Clonar o Repositório:** Clone o repositório para sua máquina local usando o seguinte comando:

```
git clone https://github.com/lucasdcampos/libra.git
```

3. **Compilar o Projeto:** Navegue até o diretório do projeto e execute o seguinte comando para compilar o projeto:

```
dotnet build
```

4. **Verificar a Compilação:** Após a compilação, verifique se o projeto foi compilado corretamente. Você deve ver uma mensagem indicando que a compilação foi bem-sucedida. Além disso, você deve ver se os arquivos a seguir foram gerados:

- `libra\src\libra\bin\Debug\net8.0\libra-interpretador.dll`
- `libra\src\Libra-CLI\bin\Debug\net8.0\Libra.exe`

> **Nota:** Esse tutorial foi feito para Windows, mas você pode compilar o projeto em outros sistemas operacionais, como Linux e macOS.

## Exemplo de Código

Aqui está um exemplo simples de um programa em Libra:

```js
// Declarando uma função
funcao fibonacci(n):
    var a = 0
    var b = 1
    var contador = 0
    enquanto (contador < n) faca
        escrever(a)
        var temp = a
        a = b
        b = temp + b
        contador = contador+1 // incrementando o contador
    fim
fim

// Escreve os primeiros 10 números de Fibonacci
fibonacci(10)
```

## Executando um Programa Libra

Você pode usar o executável `libra.exe` gerado durante a compilação do projeto para executar um programa Libra.
Para isso, use o seguinte comando:

```
src\Libra-CLI\bin\Debug\net8.0\libra exemplos\teste.libra
```

> **Nota:** Coloque o caminho para o executável `libra.exe` nas variáveis de ambiente do sistema para facilitar a execução de programas Libra.

## Como contribuir?

Se você deseja contribuir para o projeto, pode fazer um *fork* do repositório, depois cloná-lo para sua máquina local com o seguinte comando:
`git clone https://github.com/<SEU_USUARIO>/libra.git`

Contribua para o código, depois crie um *Pull Request*. Caso for uma contribuição válida, a aceitaremos ao projeto.

## Planos

Seria interessante criar uma IDE simples pra Libra, onde fosse possível escrever e analisar código Libra de forma mais conveniente, com syntax highligthing e IntelliSense.
Isso fica pra uma outra hora, ou até o momento que alguém tiver a boa vontade de fazer.

## FAQ

- **Libra é Turing Completa?** R: Está no caminho
- **Compilada ou Interpretada?** R: A Libra possui apenas um Interpretador. Talvez no futuro eu pense em desenvolver uma VM para rodar a linguagem.
- **Por que C# para o Interpretador?** R: A ideia era criar uma linguagem simples, na época eu não tinha muito conhecimento de C ou C++, e C# era minha linguagem mais avançada.

**Dúvidas?**

Me mande um e-mail: <a href="mailto:lucas.campos44@fatec.sp.gov.br">lucasm.campos@hotmail.com.br</a>
ou entre no Discord: https://discord.gg/mnGkSD4CsA
