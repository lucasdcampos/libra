using Libra.Motor;

public class ConfiguracaoProjetoLibra
{
    public string? nome { get; set; }
    public string? versao { get; set; }
    public List<string>? autores { get; set; }
    public string? codigoPrincipal { get; set; }
    public OpcoesMotorLibra? opcoesPadraoMotor { get; set; }
    public string descricao { get; internal set; } = "";
    public string licenca { get; internal set; } = "";
}
