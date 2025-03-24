[Documentação](README.md) -> [Instalação](#)

# Instalação
Para instalar Libra em seu computador, vá para https://linguagem-libra.github.io/ e escolha a versão correspondente ao seu sistema operacional.

Você baixará um arquivo compactado, basta descompactar esse arquivo no local de preferência. O executável estará dentro da pasta
extraída, **libra.exe** no caso de Windows e **libra** em Linux.

**Importante:**<br>
Antes de executar o programa, verifique se você possui o Runtime do dotnet instalado. Em Windows, instale-o com:
```ps1
winget install Microsoft.DotNet.SDK.9
```
Caso não queira usar WinGet, baixe diretamente do site oficial da Microsoft https://dotnet.microsoft.com/pt-br/download/dotnet/8.0

Para testar se tudo está funcionando, abra o executável, você deverá ver uma tela de console aberta parecida com isso:
```
Bem-vindo à Libra 0.1.0.0
Digite "ajuda", "licenca" ou uma instruçao.
>>>
```
Caso isso não acontecer, tente executar o programa pelo terminal, use `./libra` e veja se algum erro aparece na tela. O mais comum é problemas ao tentar
encontrar as bibliotecas, que devem ser carregadas junto com o Interpretador.

Se você conseguir abrir o programa sem problemas, pode avançar ao próximo passo.

## Adicionando Libra ao PATH do Sistema (Variáveis de Ambiente)
Libra foi projetada para ser chamada principalmente de um terminal, e para o processo ficar mais conveniente, precisamos adicionar o caminho do executável nas variáveis
de ambiente do sistema.

Em Windows, abra o PowerShell e digite:
```ps1
[Environment]::SetEnvironmentVariable("Path", $([Environment]::GetEnvironmentVariable("Path", [System.EnvironmentVariableTarget]::Machine) + ";C:\Caminho\Do\Diretorio"), [System.EnvironmentVariableTarget]::Machine)
```

Em Linux, use: (substitua *.bashrc* pelo seu Shell de preferência)
```sh
echo 'export PATH=$PATH:/caminho/do/diretorio' >> ~/.bashrc
source ~/.bashrc
```

## Testando se tudo ocorreu bem
Abra um novo terminal, e digite apenas `libra`, isso deve abrir a mesma tela de Console que aparece quando se abre o executável diretamente. Agora sempre que
quiser chamar a Libra, basta digitar `libra` no terminal, muito mais prático.

Próximo Capítulo: [Interpretador](interpretador.md)
