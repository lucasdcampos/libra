#!/bin/bash

# Diretórios
sourceDir="biblioteca"
destinationDir="bin/linux-x64/biblioteca"

# Verifica se o diretório de destino existe, cria se não existir
if [ ! -d "$destinationDir" ]; then
    mkdir -p "$destinationDir"
fi

# Copia os arquivos da pasta biblioteca para bin/linux-x64/biblioteca
cp -r "$sourceDir/"* "$destinationDir/"

# Executa o comando dotnet publish
dotnet publish --runtime linux-x64 --output bin/linux-x64/

