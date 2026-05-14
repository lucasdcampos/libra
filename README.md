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

Para compilar o projeto e gerar os binários de produção para Windows, Linux e macOS, siga as instruções abaixo:

1. **Pré-requisitos:** Certifique-se de ter o [.NET 9 SDK](https://dotnet.microsoft.com/download) instalado.
2. **Executar o Script de Build:**
   - No Windows: `./scripts/publicar_tudo.ps1`
   - No Linux/macOS: `./scripts/publicar_tudo.sh`

Os binários prontos para uso estarão disponíveis na pasta `bin/release-dist/`.

---

## Usando com Docker

Se você prefere não instalar o .NET localmente, pode usar a Libra através do Docker.

### 1. Construir a imagem
```bash
docker build -t libra .
```

### 2. Abrir o REPL (Modo Interativo)
```bash
docker run -it --rm libra
```

### 3. Rodar um script local
Para rodar um arquivo `.libra` que está no seu computador:
```bash
# No Linux ou PowerShell
docker run --rm -v ${PWD}:/app libra /app/seu_script.libra
```

---

## Executando um Programa Libra

Você pode usar o executável `libra` gerado para rodar seus programas. 

> **Dica:** Adicione a pasta do executável ao seu `PATH` ou use a flag `-I` para incluir diretórios de bibliotecas customizados: `libra -I ./minhas_libs script.libra`.

## Melhorando a Experiência de Desenvolvimento
Para facilitar a vida do desenvolvedor, há uma extensão no Visual Studio Code que adiciona Suporte à Libra, adicionando Syntax Highlighting e outras melhorias. 
Baixe-a em https://marketplace.visualstudio.com/items?itemName=LucasMCampos.libra.

### Rodando com Docker (Sem Instalação)

Se você tem o Docker instalado, pode rodar a Libra sem precisar configurar o .NET:

1. **Construa a imagem:**
   ```bash
   docker build -t libra .
   ```

2. **Rode o REPL (Interativo):**
   ```bash
   docker run -it libra
   ```

3. **Rode um script local:**
   ```bash
   docker run -v .:/dados libra seu_script.libra
   ```

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


