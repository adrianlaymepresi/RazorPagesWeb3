using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text;
using System.Text.Json;
using WebRazorPages1.Models;

namespace WebRazorPages1.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        public List<Tarea> ListaCompletaDeTareas { get; set; } = new();
        public List<Tarea> ListaDeTareasDeLaPagina { get; set; } = new();

        public int NumeroDePaginaActual { get; set; }
        public int CantidadTotalDePaginas { get; set; }
        public int CantidadDeRegistrosPorPagina { get; set; } = 5;

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public void OnGet(int pagina = 1)
        {
            string rutaDelArchivoJson = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "tareas.json");
            string contenidoJson = System.IO.File.ReadAllText(rutaDelArchivoJson, Encoding.UTF8);

            var opciones = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            ListaCompletaDeTareas = JsonSerializer.Deserialize<List<Tarea>>(contenidoJson, opciones) ?? new();

            NumeroDePaginaActual = pagina < 1 ? 1 : pagina;
            CantidadTotalDePaginas = (int)Math.Ceiling(ListaCompletaDeTareas.Count / (double)CantidadDeRegistrosPorPagina);

            ListaDeTareasDeLaPagina = ListaCompletaDeTareas
                .Skip((NumeroDePaginaActual - 1) * CantidadDeRegistrosPorPagina)
                .Take(CantidadDeRegistrosPorPagina)
                .ToList();
        }
    }
}
