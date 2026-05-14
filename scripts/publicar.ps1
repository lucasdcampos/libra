param (
    [Parameter(Mandatory=$false)]
    [ValidateSet("win-x64", "linux-x64", "osx-x64", "tudo")]
    [string]$Plataforma = "tudo"
)

# Script para gerar binários de produção
$platforms = @("win-x64", "linux-x64", "osx-x64")

if ($Plataforma -ne "tudo") {
    $platforms = @($Plataforma)
    Write-Host "Iniciando publicação para a plataforma: $Plataforma..." -ForegroundColor Cyan
} else {
    Write-Host "Iniciando publicação multiplataforma..." -ForegroundColor Cyan
}

$outBase = "bin/release-dist"

foreach ($rid in $platforms) {
    Write-Host "Publicando para $rid..." -ForegroundColor Yellow
    dotnet publish src/Libra.CLI/Libra.CLI.csproj `
        -c Release `
        -r $rid `
        -o "$outBase/$rid" `
        --self-contained true `
        -p:PublishSingleFile=true `
        -p:IncludeNativeLibrariesForSelfExtract=true `
        -p:DebugType=none `
        -p:DebugSymbols=false

    # Remove arquivos desnecessários que o dotnet às vezes deixa
    Remove-Item "$outBase/$rid/*.pdb" -ErrorAction SilentlyContinue
}

Write-Host "Publicação concluída! Artefatos disponíveis em: $outBase" -ForegroundColor Green
