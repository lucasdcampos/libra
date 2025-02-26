# Diretórios
$sourceDir = "biblioteca"
$destinationDir = "bin/Libra-CLI/net8.0/biblioteca"

# Verifica se o diretório de destino existe, cria se não existir
if (-not (Test-Path -Path $destinationDir)) {
    New-Item -ItemType Directory -Path $destinationDir | Out-Null
}

# Copia os arquivos da pasta biblioteca para bin/Libra-CLI/net8.0
Copy-Item -Path "$sourceDir\*" -Destination $destinationDir -Recurse -Force

# Executa o comando dotnet run
dotnet build --configuration Release
