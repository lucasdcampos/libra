// Entrada padrão exigida pelo SDK WebAssembly (WasmMainJSPath).
// No playground, o app React importa "_framework/dotnet.js" diretamente e chama
// dotnet.create() por conta própria, então este arquivo não executa lógica —
// serve apenas para satisfazer o empacotamento do runtime.
import { dotnet } from './_framework/dotnet.js'

await dotnet.create();
