## Documentação da API - MotorLibra

Este documento descreve a API pública para interagir com o motor da linguagem Libra a partir de código C#. A classe principal para esta interação é `MotorLibra`.

---

## Sumário
1.  [Visão Geral](#visão-geral)
2.  [Configurações (`OpcoesMotorLibra`)](#configurações-opcoesmotorlibra)
3.  [Classe `MotorLibra`](#classe-motorlibra)
    * [Construtores](#construtores)
    * [Métodos Principais](#métodos)
4.  [Exemplo de Uso](#exemplo-de-uso)

---

## Visão Geral

A API `MotorLibra` permite embutir o interpretador da linguagem Libra em aplicações .NET. Com ela, é possível executar scripts, trocar dados entre C# e Libra, e estender a linguagem com funcionalidades nativas.

---

## Configurações (`OpcoesMotorLibra`)

Você pode personalizar o comportamento do motor através da classe `OpcoesMotorLibra`:

-   **`NivelDebug`**: Define o detalhamento dos logs (`Nenhum`, `Basico`, `Detalhado`, `Verboso`).
-   **`ModoEstrito`**: Habilita verificações de tipo mais rigorosas.
-   **`ModoSeguro`**: Restringe o acesso a recursos do sistema.
-   **`CaminhosBiblioteca`**: Lista de caminhos onde o motor deve procurar por arquivos importados.

---

## Classe `MotorLibra`

### Construtores

```csharp
// Inicializa com opções padrão
public MotorLibra()

// Inicializa com opções customizadas
public MotorLibra(OpcoesMotorLibra opcoes)
```

### Métodos

#### `Executar`
Analisa e executa um código Libra.
```csharp
public LibraResultado Executar(string codigo, string arquivo = "", string caminho = "")
```

#### `DefinirGlobal`
Define uma variável global acessível pelo script Libra.
```csharp
public void DefinirGlobal(string identificador, object valor)
```

#### `ObterGlobal`
Obtém o valor de uma variável do ambiente Libra.
```csharp
public object ObterGlobal(string identificador)
```

#### `RegistrarFuncaoNativa`
Registra uma função C# que pode ser chamada diretamente do Libra.
```csharp
public void RegistrarFuncaoNativa(string nomeNoScript, Func<object[], object> funcaoCSharp)
```

---

## Exemplo de Uso

```csharp
using Libra.Motor;

var motor = new MotorLibra();

// 1. Definindo uma variável para o script
motor.DefinirGlobal("usuario", "Lucas");

// 2. Registrando uma função C#
motor.RegistrarFuncaoNativa("somar", (args) => {
    double a = Convert.ToDouble(args[0]);
    double b = Convert.ToDouble(args[1]);
    return a + b;
});

// 3. Executando o script
string codigo = @"
    exibir('Olá, ' + usuario);
    var res = somar(10, 5);
    exibir('Resultado: ' + res);
";

motor.Executar(codigo);
```
