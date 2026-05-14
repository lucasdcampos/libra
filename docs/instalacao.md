[Documentação](README.md) -> [Instalação](#)

# Instalação
A Libra pode ser utilizada de duas formas: instalando localmente no seu sistema ou utilizando um container Docker (recomendado para testes rápidos).

## 1. Usando Docker (Recomendado)
Se você tem o Docker instalado, pode rodar a Libra sem configurar o .NET nativamente:

1. **Construa a imagem:**
   ```bash
   docker build -t libra .
   ```

2. **Abra o REPL (Modo Interativo):**
   ```bash
   docker run -it --rm libra
   ```

3. **Rode um script local:**
   ```bash
   docker run --rm -v ${PWD}:/dados libra seu_script.libra
   ```

## 2. Instalação Local
Para instalar a Libra nativamente:

1. Vá para o repositório oficial no GitHub e baixe a versão correspondente ao seu sistema operacional.
2. Descompacte o arquivo. O executável será **libra.exe** (Windows) ou **libra** (Linux/macOS).

### Pré-requisitos
Antes de executar, você precisa do **.NET 9 Runtime** instalado.
- **Windows**: `winget install Microsoft.DotNet.SDK.9`
- **Geral**: Baixe em [dotnet.microsoft.com](https://dotnet.microsoft.com/download/dotnet/9.0)

### Testando a Instalação
Abra o terminal e execute `./libra`. Você deverá ver:
```text
Bem-vindo à Libra 0.1.1
Digite "ajuda", "licenca" ou uma instrução.
>>>
```

## Adicionando Libra ao PATH
Para facilitar o uso, adicione o diretório do executável às variáveis de ambiente do seu sistema.

**Windows (PowerShell):**
```ps1
[Environment]::SetEnvironmentVariable("Path", $([Environment]::GetEnvironmentVariable("Path", [System.EnvironmentVariableTarget]::Machine) + ";C:\Caminho\Para\Libra"), [System.EnvironmentVariableTarget]::Machine)
```

**Linux/macOS:**
```bash
echo 'export PATH=$PATH:/caminho/para/libra' >> ~/.bashrc
source ~/.bashrc
```

Próximo Capítulo: [Interpretador](interpretador.md)
