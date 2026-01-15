
namespace ConsultaPeso.Domain;

public class ConsultaPesosRequest
{
    public DateOnly  FechaInicio { get; init; }
    public DateOnly  FechaFin { get; init; }
    public Empleado Empleado { get; init; }
    public IReadOnlyList<PesoEmpleado> Pesos { get; init; }
}
