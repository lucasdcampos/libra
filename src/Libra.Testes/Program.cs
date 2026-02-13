using Libra;
using Libra.VM;

string codigo = @"
se 0 entao
    ""a""
senao se 0 entao
    ""b""
senao
    ""c""
fim
";
var tokenizador = new Tokenizador(codigo);
var tokens = tokenizador.Tokenizar().ToArray();
var parser = new Parser(tokens);
var prog = parser.Parse();
var gerador = new GeradorBytecode(prog);
var instrucoes = gerador.Gerar();
var vm = new VM(instrucoes);

vm.Executar();
