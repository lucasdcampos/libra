using Libra.Api;

var motor = new MotorLibra(new OpcoesMotorLibra
{
    NivelDebug = 0,
});

var saida = motor.Executar("1+2*3");

// DEVE COMPILAR:
// EMPILHAR, 42
// 1, 42