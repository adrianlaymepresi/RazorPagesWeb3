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

        // Estas variables se usan en el OnGet para manejar la paginación y los filtros
        [BindProperty(SupportsGet = true, Name = "cantidadRegistrosPorPagina")]
        public int CantidadDeRegistrosPorPagina { get; set; } = 5;

        [BindProperty(SupportsGet = true, Name = "filtroEstado")]
        public string FiltroEstado { get; set; } = "Pendiente";

        [BindProperty(SupportsGet = true, Name = "pagina")]
        public int Pagina { get; set; } = 1;

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {
            CantidadDeRegistrosPorPagina = Math.Clamp(CantidadDeRegistrosPorPagina <= 0 ? 5 : CantidadDeRegistrosPorPagina, 1, 99);

            CargarTareasDesdeJson();

            /*
            YA NO SERIA NECESARIO REORDENAR POR ESTADO
            if (!string.IsNullOrWhiteSpace(FiltroEstado))
            {
                ReordenarTareasPorEstado();
            } 
            */

            CalcularPaginacion();
            ObtenerTareasPaginaActual();
        }

        private void CargarTareasDesdeJson()
        {
            try
            {
                var ruta = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "tareas.json");
                var json = System.IO.File.ReadAllText(ruta, Encoding.UTF8);
                var opt = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                ListaCompletaDeTareas = JsonSerializer.Deserialize<List<Tarea>>(json, opt) ?? new();
                ListaCompletaDeTareas = ListaCompletaDeTareas
                .Where(t => t.EstadoDeLaTarea == "Pendiente" || t.EstadoDeLaTarea == "En curso")
                .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "No se pudo leer/deserializar tareas.json");
                ListaCompletaDeTareas = new();
            }
        }

        private void ReordenarTareasPorEstado()
        {
            var priorizadas = ListaCompletaDeTareas
                .Where(t => t.EstadoDeLaTarea.Equals(FiltroEstado, StringComparison.OrdinalIgnoreCase))
                .ToList();

            /*
            var resto = ListaCompletaDeTareas
                .Where(t => !t.EstadoDeLaTarea.Equals(FiltroEstado, StringComparison.OrdinalIgnoreCase))
                .ToList();
            ListaCompletaDeTareas = priorizadas.Concat(resto).ToList();
            */
        }

        private void CalcularPaginacion()
        {
            NumeroDePaginaActual = Math.Max(1, Pagina);
            CantidadTotalDePaginas = (int)Math.Ceiling(ListaCompletaDeTareas.Count / (double)CantidadDeRegistrosPorPagina);

            if (NumeroDePaginaActual > CantidadTotalDePaginas && CantidadTotalDePaginas > 0)
            {
                NumeroDePaginaActual = CantidadTotalDePaginas;
            }
        }

        private void ObtenerTareasPaginaActual()
        {
            ListaDeTareasDeLaPagina = ListaCompletaDeTareas
                .Skip((NumeroDePaginaActual - 1) * CantidadDeRegistrosPorPagina)
                .Take(CantidadDeRegistrosPorPagina)
                .ToList();
        }
    }
}