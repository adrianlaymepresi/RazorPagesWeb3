using System.Text.Json.Serialization;

namespace WebRazorPages1.Models
{
    public class Tarea
    {
        [JsonPropertyName("idTarea")]
        public string? IdTarea { get; set; }

        [JsonPropertyName("nombreTarea")]
        public string NombreDeLaTarea { get; set; } = "";

        [JsonPropertyName("fechaVencimiento")]
        public string FechaDeVencimiento { get; set; } = "";

        [JsonPropertyName("estado")]
        public string EstadoDeLaTarea { get; set; } = "";
    }
}
