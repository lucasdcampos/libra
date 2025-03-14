# Diretórios
$sourceDir = "biblioteca"
$destinationDir = "bin/win-x64/biblioteca"

# Verifica se o diretório de destino existe, cria se não existir
if (-not (Test-Path -Path $destinationDir)) {
    New-Item -ItemType Directory -Path $destinationDir | Out-Null
}

# Copia os arquivos da pasta biblioteca para bin/Libra-CLI/net8.0
Copy-Item -Path "$sourceDir\*" -Destination $destinationDir -Recurse -Force

dotnet publish --runtime win-x64 --output bin/win-x64/