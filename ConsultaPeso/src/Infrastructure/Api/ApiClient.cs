using ConsultaPeso.Domain;

namespace ConsultaPeso.Infrastructure.Api;

public sealed class ApiClient
{
    private readonly HttpClient _http;

    public ApiClient(HttpClient http)
    {
        _http = http;
    }

    // Por ahora: GET mockeado
    public async Task<ConsultaPesosRequest> ObtenerConsultaPesosAsync(string codigoEmpleado, DateOnly lunes, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(codigoEmpleado))
            throw new ArgumentException("El código del empleado es obligatorio.", nameof(codigoEmpleado));
        
        // Simula latencia real de red
        await Task.Delay(300, ct);

        // Semana completa
        var domingo = lunes.AddDays(6);

        return new ConsultaPesosRequest
        {
            FechaInicio = lunes,
            FechaFin = domingo,
            Empleado = new Empleado
            {
                Id = 204,
                Codigo = codigoEmpleado,
                NombreCompleto = "Luis Alberto Rodríguez"
            },
            Pesos = new List<PesoEmpleado>
            {
                new()
                {
                    Id = 1,
                    Fecha = DateTime.Now.AddDays(0),
                    Descripcion = "Descabezado",
                    Valor = 125.40m,
                    Precio = 1.85m
                },
                new()
                {
                    Id = 2,
                    Fecha = DateTime.Now.AddDays(1),
                    Descripcion = "PYD Grande",
                    Valor = 98.75m,
                    Precio = 2.10m
                },
                new()
                {
                    Id = 3,
                    Fecha = DateTime.Now.AddDays(2),
                    Descripcion = "PPO Mediano",
                    Valor = 110.20m,
                    Precio = 1.95m
                },
                new()
                {
                    Id = 4,
                    Fecha = DateTime.Now.AddDays(4),
                    Descripcion = "PYP Pequeño",
                    Valor = 87.60m,
                    Precio = 1.70m
                },
                new()
                {
                    Id = 5,
                    Fecha = DateTime.Now.AddDays(5),
                    Descripcion = "Limpieza Maquina",
                    Valor = 15.00m,
                    Precio = 0.99m
                }
            }
        };
    }
}