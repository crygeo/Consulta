using ConsultaPeso.Domain;
using ConsultaPeso.Infrastructure.Api;

namespace ConsultaPeso.Application
{
    public sealed class ConsultaPesosService
    {
        private readonly ApiClient _apiClient;

        public ConsultaPesosService(ApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<ConsultaPesosRequest> ConsultarSemanaAsync(string codigoEmpleado, DateOnly fechaSeleccionada, CancellationToken ct = default)
        {
            // Validaciones mínimas (negocio)
            if (string.IsNullOrWhiteSpace(codigoEmpleado))
                throw new ArgumentException("El código del empleado es obligatorio.", nameof(codigoEmpleado));

            // Normalizar semana (lunes → domingo)
            var lunes = ObtenerLunes(fechaSeleccionada);

            // Llamada a infraestructura
            var resultado = await _apiClient.ObtenerConsultaPesosAsync(
                codigoEmpleado,
                lunes,
                ct);

            return resultado;
        }

        private static DateOnly ObtenerLunes(DateOnly fecha)
        {
            var diff = fecha.DayOfWeek - DayOfWeek.Monday;
            if (diff < 0)
                diff += 7;

            return fecha.AddDays(-diff);
        }
    }
}