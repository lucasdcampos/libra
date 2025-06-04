using Libra.Api;

class Program
{
    public record ExecutarRequest(string codigo);

    static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddOpenApi();

        // Configura Kestrel para usar os endpoints definidos no appsettings.json (HTTP e HTTPS)
        builder.WebHost.ConfigureKestrel(serverOptions =>
        {
            serverOptions.Configure(builder.Configuration.GetSection("Kestrel"));
        });

        // CORS
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("LiberaGeral", policy =>
            {
                policy.AllowAnyOrigin()
                      .AllowAnyHeader()
                      .AllowAnyMethod();
            });
        });

        var app = builder.Build();

        // Redireciona HTTP para HTTPS
        app.UseHttpsRedirection();

        app.UseCors("LiberaGeral");

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        app.MapPost("/executar", (ExecutarRequest req) =>
        {
            var opcoes = new OpcoesMotorLibra();
            opcoes.ModoSeguro = true;
            opcoes.PermitirEntrada = false;

            var motor = new MotorLibra(opcoes);
            try
            {
                var resultado = motor.Executar(req.codigo).Saida;
                return Results.Text(resultado?.ToString() ?? string.Empty, "text/plain");
            }
            catch (Exception ex)
            {
                return Results.Problem($"Erro ao executar o código: {ex.Message}");
            }
        })
        .WithName("ExecutarCodigo");

        app.MapGet("/", () => Results.Text("API Libra rodando!", "text/plain"));

        app.Run();
    }
}
