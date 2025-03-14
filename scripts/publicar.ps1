# Excluir a pasta bin/
Remove-Item -Recurse -Force -ErrorAction SilentlyContinue bin/

& ./scripts/publicar_win-x64.ps1
& ./scripts/publicar_linux-x64.ps1
& ./scripts/publicar_linux-x64_selfc.ps1

Remove-Item -Recurse -Force -ErrorAction SilentlyContinue bin/Libra/
Remove-Item -Recurse -Force -ErrorAction SilentlyContinue bin/Libra-CLI/