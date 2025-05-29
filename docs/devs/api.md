## Documentação da API - MotorLibra

Este documento descreve a API pública para interagir com o motor da linguagem Libra a partir de código C#. A classe principal para esta interação é MotorLibra.

## Sumário
1.  [Visão Geral](#visao-geral)
2.  [Tipos Auxiliares](#tipos-auxiliares)
    * [Enum `ModoExecucao`](#enum-modoexecucao)
    * [Classe `OpcoesMotorLibra`](#classe-opcoesmotorlibra)
3.  [Classe `MotorLibra`](#classe-motorlibra)
    * [Construtores](#construtores)
    * [Métodos](#metodos)
        * [`DefinirGlobal`](#definirglobal)
        * [`ObterGlobal`](#obterglobal)
        * [`RegistrarFuncaoNativa`](#registrarfuncaonativa)
        * [`Executar`](#executar)
4.  [Exemplo de Uso](#exemplo-de-uso)
5.  [Tratamento de Erros](#tratamento-de-erros)

## Visão Geral

A API `MotorLibra` permite embutir o interpretador da linguagem Libra em aplicações .NET. Com ela, é possível executar scripts Libra, trocar dados entre C# e Libra, e estender a linguagem com funcionalidades nativas escritas em C#.

## Tipos Auxiliares

### Enum `ModoExecucao`

Define como o `MotorLibra` deve processar os scripts.

```csharp
public enum ModoExecucao
{
    /// <summary>
    /// O script será interpretado diretamente.
    /// </summary>
    Interpretar,

    /// <summary>
    /// (Não implementado) O script seria compilado para um formato intermediário.
    /// </summary>
    Compilar,

    /// <summary>
    /// (Não implementado) O script seria compilado e então executado.
    /// </summary>
    CompilarEExecutar
}
```

### Classe OpcoesMotorLibra

Configurações para personalizar o comportamento de uma instância de MotorLibra.

```csharp
// Definição inferida baseada no uso em MotorLibra
public class OpcoesMotorLibra
{
/// &lt;summary>
/// Nível de detalhamento para logs de debug internos do motor.
/// (O uso específico deste campo não está detalhado no código de MotorLibra fornecido).
/// &lt;/summary>
public int NivelDebug { get; set; }

/// <summary>
/// Define o modo de execução para os scripts.
/// </summary>
public ModoExecucao ModoExecucao { get; set; }
}
```

### Classe MotorLibra

É a classe central da API para interagir com a linguagem Libra.

```csharp
namespace Libra.Api { public class MotorLibra
{ 
    // ... membros privados ... 
}
```
### Construtores

```csharp 
public MotorLibra()
```

Inicializa uma nova instância de `MotorLibra` com configurações padrão:

- NivelDebug é definido como 0.
- ModoExecucao é definido como ModoExecucao.Interpretar.

```csharp 
public MotorLibra(OpcoesMotorLibra opcoes)
```

Inicializa uma nova instância de `MotorLibra` com as opções fornecidas pelo usuário.

Parâmetros:
- opcoes (OpcoesMotorLibra): Um objeto contendo as configurações desejadas para o motor.
- Observação: No código fornecido, este construtor apenas armazena as opcoes e não inicializa o campo _ambiente. Isso pode ser uma omissão, já que RegistrarFuncaoNativa verifica se _ambiente foi inicializado.

### Métodos
`DefinirGlobal`
```csharp
public void DefinirGlobal(string identificador, object valor)
```

Define uma variável global no ambiente de execução da linguagem Libra, tornando-a acessível em todos os scripts subsequentes executados por qualquer instância do motor que compartilhe este ambiente estático.

Parâmetros:
- identificador (string): O nome da variável como será conhecida nos scripts Libra.
- valor (object): O valor C# a ser atribuído à variável global. O motor (ou a classe Ambiente) tentará lidar com a conversão para um tipo Libra apropriado.
- Retorno: void

`ObterGlobal`

```csharp
public object? ObterGlobal(string identificador)
```
Obtém o valor de uma variável global previamente definida no ambiente de execução da linguagem Libra.

Parâmetros:
- identificador (string): O nome da variável global.
Retorno: object? - O valor da variável global como um objeto C#, ou null se a variável não existir ou seu valor for nulo.

`RegistrarFuncaoNativa`

```csharp
public void RegistrarFuncaoNativa(string nomeNoScript, Func&lt;object?[], object?> funcaoCSharp)
```
Registra uma função `C#` para que possa ser chamada de dentro dos scripts Libra. Permite estender a linguagem com código nativo.

Parâmetros:
- nomeNoScript (string): O nome pelo qual a função será conhecida e chamada dentro dos scripts Libra.
- funcaoCSharp (Func<object?[], object?>): Um delegado C#. A função recebe um array de objetos (argumentos da chamada no script, convertidos para tipos C#) e deve retornar um object? (o resultado para o script, que será convertido para um tipo Libra).
- Retorno: void

`Executar`

```csharp 
public object? Executar(string codigo)
```
Analisa (tokeniza e faz o parse) e executa um trecho de código Libra fornecido como uma string.

Parâmetros:
- codigo (string): O código Libra a ser executado.
- Retorno: object? - O resultado da execução do script. Baseado no código fornecido, este valor é obtido através de Interpretador.Saida.


### Exemplo de Uso
```csharp
using Libra.Api;

var motor = new MotorLibra();

// Definindo uma variável global
motor.DefinirGlobal("mensagemGlobal", "Olá do C# para Libra!");

// Registrando uma função C#
motor.RegistrarFuncaoNativa("somarEmCs", (argumentos) =>
{
    if (argumentos != null && argumentos.Length == 2)
    {
        // É recomendável tratar conversões de tipo de forma robusta
        try
        {
            double arg1 = Convert.ToDouble(argumentos[0]);
            double arg2 = Convert.ToDouble(argumentos[1]);
            return arg1 + arg2;
        }
        catch
        {
            return null; // Ou lançar um erro específico da função nativa
        }
    }
    return null;
});

string scriptLibra = @"
    exibir(mensagemGlobal);
    var resultadoSoma = somarEmCs(10.5, 20);
    exibir('Resultado da soma em C#: ', resultadoSoma);
    resultadoSoma + 5; // Script retorna um valor ao C#
";

Console.WriteLine("Executando script Libra...");
object? resultadoDoScript = motor.Executar(scriptLibra);

if (resultadoDoScript != null)
{
    Console.WriteLine($"O script Libra retornou para C#: {resultadoDoScript} (Tipo: {resultadoDoScript})");
}
else
{
    Console.WriteLine("O script Libra não retornou um valor ou ocorreu um erro interno não lançado.");
}
```