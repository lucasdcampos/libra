#!/bin/bash

# Diretórios
sourceDir="biblioteca"
destinationDir="$HOME/.local/share/Libra"

# Verifica se o diretório de destino existe, cria se não existir
if [ ! -d "$destinationDir" ]; then
    mkdir -p "$destinationDir"
fi

# Copia os arquivos da pasta biblioteca para ~/.local/share/Libra/
cp -r "$sourceDir/"* "$destinationDir"

# Publicação do projeto .NET para Linux x64
dotnet publish --runtime linux-x64 --output bin/linux-x64/
