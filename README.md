<div align="center">
    <img src="https://avatars.githubusercontent.com/u/170988597?s=400&u=1aa68e42fb32ade404e8312e7b765d74578a57e3&v=4" width="180px">
    <h1>Libra</h1>
    <p><b>Uma Linguagem de Programação simples, moderna e em Português.</b></p>
    <a href="https://testar.linguagemlibra.site"><b>Testar no Navegador</b></a> |
    <a href="https://marketplace.visualstudio.com/items?itemName=LucasMCampos.libra"><b>Extensão VS Code</b></a>
</div>

---

## O que é a Libra?

Libra é uma linguagem de programação interpretada projetada para ser simples, intuitiva e totalmente em português. Ela é ideal para:
- **Estudantes**: Aprender lógica de programação sem a barreira do inglês.
- **Automações**: Criar scripts rápidos para o dia a dia.
- **Algoritmos**: Praticar estruturas de dados de forma clara.

---

## Exemplo de Código

```js
// Jogo de Adivinhação
importar matematica

const numeroSecreto = int(matematica.aleatorio(0, 100))
exibir("Adivinhe o número entre 0 e 100!")

var palpite = int(entrada())

enquanto palpite != numeroSecreto repetir
    se palpite > numeroSecreto entao
        exibir("Muito alto! Tente um menor:")
    senao
        exibir("Muito baixo! Tente um maior:")
    fim
    palpite = int(entrada())
fim

exibir("Parabéns! Você acertou.")
```

---

## Início Rápido com Docker

A forma mais fácil de testar a Libra sem instalar nada no seu sistema é via Docker:

1. **Construir a imagem:**
   ```bash
   docker build -t libra .
   ```

2. **Abrir o REPL (Interativo):**
   ```bash
   docker run -it --rm libra
   ```

3. **Rodar um script local:**
   ```bash
   docker run --rm -v ${PWD}:/dados libra seu_script.libra
   ```

---

## Instalação e Build Local

Se você deseja compilar a Libra nativamente, precisará do **.NET 9 SDK**.

1. **Clone o repositório:**
   ```bash
   git clone https://github.com/linguagem-libra/libra.git
   cd libra
   ```

2. **Publique os binários:**
   - **Windows (PowerShell):** `./scripts/publicar_tudo.ps1`
   - **Linux/macOS (Bash):** `./scripts/publicar_tudo.sh`

Os executáveis estarão disponíveis na pasta `bin/release-dist/`.

---

## Testes

A Libra possui uma bateria completa de testes de funcionalidade e performance.

```bash
# Testes de Funcionalidade
dotnet run --project src/Libra.CLI -- testes/bateria/runner.libra

# Benchmarks de Performance
dotnet run --project src/Libra.CLI -- testes/performance/runner.libra
```

---

## Sintaxe em Resumo

| Funcionalidade | Exemplo |
| :--- | :--- |
| **Variáveis** | `var nome = "Libra"` |
| **Constantes** | `const PI = 3.14` |
| **Tipos** | `var x: Int`, `var y: Real`, `var s: Texto` |
| **Vetores** | `var lista = {1, 2, 3}` |
| **Classes** | `classe Animal ... fim` |
| **Loops** | `enquanto condicao repetir ... fim` |
| **Para Cada** | `para cada item em lista ... fim` |

---

## Como contribuir?

Toda ajuda é bem-vinda! 
1. Faça um **Fork** do projeto.
2. Crie uma **Branch** para sua modificação (`git checkout -b feature/nova-funcionalidade`).
3. Faça o **Commit** (`git commit -am 'Adiciona nova funcionalidade'`).
4. Dê um **Push** (`git push origin feature/nova-funcionalidade`).
5. Abra um **Pull Request**.

---

## Comunidade e Suporte

- **Discord**: [Entre no nosso servidor](https://discord.gg/mnGkSD4CsA)
- **E-mail**: [lucasm.campos@hotmail.com.br](mailto:lucasm.campos@hotmail.com.br)

---
<p align="center">by Lucas M. Campos</p>
