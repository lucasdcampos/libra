#!/bin/bash

# Script para gerar binários de produção
PLATAFORMA=${1:-"tudo"}
platforms=("win-x64" "linux-x64" "osx-x64")

# Cores para o terminal
CYAN='\033[0;36m'
YELLOW='\033[1;33m'
GREEN='\033[0;32m'
NC='\033[0m' # No Color

if [[ "$PLATAFORMA" != "tudo" ]]; then
    # Verifica se a plataforma é válida
    valid=false
    for p in "${platforms[@]}"; do
        if [[ "$p" == "$PLATAFORMA" ]]; then
            valid=true
            break
        fi
    done

    if [ "$valid" = false ]; then
        echo -e "Erro: Plataforma '$PLATAFORMA' inválida. Use: win-x64, linux-x64, osx-x64 ou tudo."
        exit 1
    fi

    platforms=("$PLATAFORMA")
    echo -e "${CYAN}Iniciando publicação para a plataforma: $PLATAFORMA...${NC}"
else
    echo -e "${CYAN}Iniciando publicação multiplataforma...${NC}"
fi

outBase="bin/release-dist"

for rid in "${platforms[@]}"
do
    echo -e "${YELLOW}Publicando para $rid...${NC}"
    dotnet publish src/Libra.CLI/Libra.CLI.csproj \
        -c Release \
        -r "$rid" \
        -o "$outBase/$rid" \
        --self-contained true \
        -p:PublishSingleFile=true \
        -p:IncludeNativeLibrariesForSelfExtract=true \
        -p:DebugType=none \
        -p:DebugSymbols=false

    # Remove arquivos de debug (PDB) para economizar espaço
    rm -f "$outBase/$rid/"*.pdb
done

echo -e "${GREEN}Publicação concluída! Artefatos disponíveis em: $outBase${NC}"
