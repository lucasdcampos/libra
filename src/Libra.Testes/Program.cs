using Libra.Api;

string codigo = "1+2*3";
var motor = new MotorLibra();
var saida = motor.Executar(codigo);

Console.WriteLine("Código: " + codigo);
Console.WriteLine($"Saída: {saida}");