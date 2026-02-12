using Libra.Motor;

public class ConfiguracaoProjetoLibra
{
    public string? NomeProjeto { get; set; }
    public string? Versao { get; set; }
    public List<string>? Autores { get; set; }
    public string? CodigoPrincipal { get; set; }
    public OpcoesMotorLibra? OpcoesPadraoMotor { get; set; }
    public string Descricao { get; internal set; } = "";
    public string Licenca { get; internal set; } = "";
}