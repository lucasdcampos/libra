# --- Estágio de Compilação ---
FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build
WORKDIR /src

# Restaura as dependências (otimiza cache do Docker)
COPY ["src/Libra/Libra.csproj", "src/Libra/"]
COPY ["src/Libra.CLI/Libra.CLI.csproj", "src/Libra.CLI/"]
RUN dotnet restore "src/Libra.CLI/Libra.CLI.csproj"

# Compila a aplicação como um executável único e nativo para Linux
COPY . .
RUN dotnet publish "src/Libra.CLI/Libra.CLI.csproj" \
    -c Release \
    -r linux-musl-x64 \
    -o /app/publish \
    --self-contained true \
    -p:PublishSingleFile=true \
    -p:DebugType=none

# --- Estágio de Execução ---
FROM mcr.microsoft.com/dotnet/runtime-deps:9.0-alpine AS runtime
WORKDIR /libra

# Configurações de Globalização (necessário para .NET no Alpine)
RUN apk add --no-cache icu-libs
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

# Copia o executável e a biblioteca padrão
COPY --from=build /app/publish .

# Cria um diretório para o usuário mapear seus scripts
WORKDIR /dados
VOLUME /dados

# Define o binário da Libra como ponto de entrada
# Isso permite usar a imagem como um comando: 'docker run libra meu_script.libra'
ENTRYPOINT ["/libra/libra"]

# Por padrão, inicia o REPL se nenhum argumento for passado
CMD []
