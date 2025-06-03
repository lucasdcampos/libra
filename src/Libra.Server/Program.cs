using Libra.Api;

class Program
{
    public record ExecutarRequest(string codigo);

    static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddOpenApi();
        builder.WebHost.ConfigureKestrel(serverOptions =>
        {
            serverOptions.ListenAnyIP(5102);
        });

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

        app.UseCors("LiberaGeral");

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        app.MapPost("/executar", (ExecutarRequest req) =>
        {
            var motor = new MotorLibra();
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

        app.Run();
    }
}
