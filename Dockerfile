# Etapa 1: Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

# Definir o diretório de trabalho
WORKDIR /app

# Copiar todos os arquivos para o contêiner
COPY . .

# Tornar o script shell executável
RUN chmod +x scripts/publicar.sh

# Executar o script de publicação
RUN ./scripts/publicar.sh

# Etapa 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime

# Definir o diretório de trabalho
WORKDIR /app

# Copiar todos os arquivos do contêiner de build para o contêiner de execução
COPY --from=build /app .

# Definir o comando para executar o programa, permitindo argumentos
ENTRYPOINT ["./bin/linux-x64/libra"]
