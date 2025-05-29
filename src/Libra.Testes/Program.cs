using Libra.Api;

var motor = new MotorLibra();

motor.DefinirGlobal("x", 40);

var saida = motor.Executar("x + 2");

Console.WriteLine(saida); // Deve imprimir 42