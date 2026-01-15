namespace ConsultaPeso.Domain;

public class PesoEmpleado
{
    public int Id { get; init; }
    public DateTime  Fecha { get; init; }
    public string Descripcion { get; init; }
    public decimal Valor { get; init; }
    public decimal Precio { get; init; }
}
