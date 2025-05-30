# Diretórios
$sourceDir = "biblioteca"
$destinationDir = "$env:LOCALAPPDATA\Libra"

# Verifica se o diretório de destino existe, cria se não existir
if (-not (Test-Path -Path $destinationDir)) {
    New-Item -ItemType Directory -Path $destinationDir -Force | Out-Null
}

# Copia os arquivos da pasta biblioteca para AppData\Local\Libra\
Copy-Item -Path "$sourceDir\*" -Destination $destinationDir -Recurse -Force

# Publicação do projeto .NET
dotnet publish --runtime win-x64 --output bin/win-x64/