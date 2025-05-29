using Libra.Api;

var motor = new MotorLibra(new OpcoesMotorLibra
{
    NivelDebug = 0,
    ModoExecucao = ModoExecucao.Compilar
});

var saida = motor.Executar("42");

// DEVE COMPILAR:
// EMPILHAR, 42
// 1, 42