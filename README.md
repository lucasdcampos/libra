<div align=center>
    <img src="https://avatars.githubusercontent.com/u/170988597?s=400&u=1aa68e42fb32ade404e8312e7b765d74578a57e3&v=4" width=180px>
</div>

# Libra, uma Linguagem de Programação simples em Português
&copy; 2024 - 2025 Lucas M. Campos

-   [Libra, uma Linguagem de Programação simples em Português](#libra-uma-linguagem-de-programacão-simples-em-português)
    -   [O que é a Libra](#o-que-é-a-libra)
    -   [O que eu consigo criar com a Libra?](#o-que-eu-consigo-criar-com-a-libra)
    -   [Exemplo de Código](#exemplo-de-código)
    -   [Compilando a Libra](#compilando-a-libra)
    -   [Executando um Programa Libra](#executando-um-programa-libra)
    -   [Mais exemplos](#mais-exemplos)
    -   [Como contribuir?](#como-contribuir)

## O que é a Libra?

Libra é uma linguagem de programação interpretada simples em português, ideal para quem está aprendendo.

> Experimente a linguagem agora direto do seu navegador: https://testar.libra.lucasof.com

## O que eu consigo criar com a Libra?

Essencialmente, qualquer programa pode ser escrito em Libra, mas ela foi projetada para servir de aprendizado para estudantes de programação
e pequenas automações, Libra não foi feita para grandes aplicações.

## Exemplo de Código

Aqui está um exemplo simples de um programa em Libra:

```js
// Jogo de adivinhar o número escolhido, enquanto o usuário não acertar, dar
// dicas se o número que ele tentou é maior ou menor do número escolihdo

importar matematica

const num = int(aleatorio(0, 100))

exibir("Digite um número entre 0 e 100:)
var escolha = int(entrada()) // Pedir um número ao usuário

// Enquanto o usuário não acertar o número
enquanto escolha != num repetir
    se escolha > num entao
        exibir("Número maior que o aleatório!")
    senao se escolha < num entao
        exibir("Número menor que o aleatório!")
    fim

    exibir("Tente denovo:")
    escolha = int(entrada()) // Pedindo outro número ao usuário
fim

// Loop terminou, o usuário acertou o número
exibir("Parabéns, o número era " + num)
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

## Mais exemplos
Olá Mundo?
```js
exibir("Olá, Mundo!")
```
Classes
```js
classe Pessoa
    var nome: Texto
    var idade: Int
    funcao Pessoa(auto, nome, idade)
        auto.nome = nome
        auto.idade = idade
    fim
fim
const p = Pessoa("John Doe", 30)
exibir(p.nome)
```
Vetores
```js
importar vetores
var x = {1, 2, 3}
x = v_incluir(x, 4) // x agora é {1, 2, 3, 4}

exibir(x[0]) // 1
```
Tipos
```ts
var nome: Texto            // Um Texto de tamanho qualquer
var idade: Int             // Número inteiro de 32 bits
var saldo: Real            // Número flutuante de 64 bits
var opcoes: Vetor          // Lista de tamanho fixo
var desconhecido: Objeto   // Aceita qualquer valor
```
## Como contribuir?

Se você deseja contribuir para o projeto, pode fazer um *fork* do repositório, depois cloná-lo para sua máquina local com o seguinte comando:
`git clone https://github.com/lucasdcampos/libra.git`

Contribua para o código, depois crie um *Pull Request*. Caso for uma contribuição válida, a aceitaremos ao projeto.

**Dúvidas?**

Me mande um e-mail: <a href="mailto:lucasm.campos@hotmail.com.br">lucasm.campos@hotmail.com.br</a>
ou entre no Discord: https://discord.gg/mnGkSD4CsA


