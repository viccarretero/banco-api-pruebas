using System.Collections.Concurrent;

// =====================================================================
// Banco API - API REST de ejemplo para pruebas de API con Newman y k6.
// Datos en memoria: cada vez que se reinicia vuelve a los datos iniciales.
// =====================================================================

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var cuentas = new ConcurrentDictionary<int, Cuenta>();
var siguienteId = 2;
cuentas[1] = new Cuenta { Id = 1, Titular = "Ana Torres", Saldo = 1500m };
cuentas[2] = new Cuenta { Id = 2, Titular = "Luis Ramos", Saldo = 320.50m };

app.MapGet("/health", () => Results.Ok(new { estado = "ok" }));

// Listar cuentas
app.MapGet("/api/cuentas", () => Results.Ok(cuentas.Values.OrderBy(c => c.Id)));

// Obtener una cuenta
app.MapGet("/api/cuentas/{id:int}", (int id) =>
    cuentas.TryGetValue(id, out var cuenta)
        ? Results.Ok(cuenta)
        : Results.NotFound(new { error = $"La cuenta {id} no existe" }));

// Crear una cuenta
app.MapPost("/api/cuentas", (CuentaNueva datos) =>
{
    if (string.IsNullOrWhiteSpace(datos.Titular))
        return Results.BadRequest(new { error = "El titular es obligatorio" });
    if (datos.SaldoInicial < 0)
        return Results.BadRequest(new { error = "El saldo inicial no puede ser negativo" });

    var id = Interlocked.Increment(ref siguienteId);
    var cuenta = new Cuenta { Id = id, Titular = datos.Titular.Trim(), Saldo = datos.SaldoInicial };
    cuentas[id] = cuenta;
    return Results.Created($"/api/cuentas/{id}", cuenta);
});

// Depositar dinero
app.MapPost("/api/cuentas/{id:int}/depositos", (int id, Movimiento mov) =>
{
    if (!cuentas.TryGetValue(id, out var cuenta))
        return Results.NotFound(new { error = $"La cuenta {id} no existe" });
    if (mov.Monto <= 0)
        return Results.BadRequest(new { error = "El monto debe ser mayor a cero" });

    lock (cuenta) { cuenta.Saldo += mov.Monto; }
    return Results.Ok(cuenta);
});

// Retirar dinero
app.MapPost("/api/cuentas/{id:int}/retiros", (int id, Movimiento mov) =>
{
    if (!cuentas.TryGetValue(id, out var cuenta))
        return Results.NotFound(new { error = $"La cuenta {id} no existe" });
    if (mov.Monto <= 0)
        return Results.BadRequest(new { error = "El monto debe ser mayor a cero" });

    lock (cuenta)
    {
        if (cuenta.Saldo < mov.Monto)
            return Results.UnprocessableEntity(new { error = "Saldo insuficiente" });
        cuenta.Saldo -= mov.Monto;
    }
    return Results.Ok(cuenta);
});

// Eliminar una cuenta
app.MapDelete("/api/cuentas/{id:int}", (int id) =>
    cuentas.TryRemove(id, out _)
        ? Results.NoContent()
        : Results.NotFound(new { error = $"La cuenta {id} no existe" }));

app.Run();

public class Cuenta
{
    public int Id { get; set; }
    public string Titular { get; set; } = "";
    public decimal Saldo { get; set; }
}

public record CuentaNueva(string? Titular, decimal SaldoInicial);

public record Movimiento(decimal Monto);

public partial class Program { }
