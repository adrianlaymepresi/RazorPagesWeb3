using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text;
using System.Text.Json;
using System.Globalization;
using WebRazorPages1.Models;


namespace WebRazorPages1.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

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

        [BindProperty(SupportsGet = true, Name = "q")]
        public string TextoDeBusqueda { get; set; } = "";

        public void OnGet()
        {
            CantidadDeRegistrosPorPagina = Math.Clamp(CantidadDeRegistrosPorPagina <= 0 ? 5 : CantidadDeRegistrosPorPagina, 1, 99);

            CargarTareasDesdeJson();
            AplicarBusqueda();
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
        // CREAR NUEVA TAREA
        public async Task<IActionResult> OnPostCrearAsync(string NuevoNombreDeLaTarea, string NuevaFechaDeVencimiento)
        {
            try
            {
                var nombre = (NuevoNombreDeLaTarea ?? "").Trim();
                var fecha = DateTime.ParseExact(NuevaFechaDeVencimiento, "yyyy-MM-dd", CultureInfo.InvariantCulture);

                var nuevaTarea = new Tarea
                {
                    IdTarea = Guid.NewGuid().ToString(),
                    NombreDeLaTarea = nombre,
                    FechaDeVencimiento = fecha.ToString("dd/MM/yyyy"),
                    EstadoDeLaTarea = "Pendiente"
                };

                var ruta = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "tareas.json");
                var json = await System.IO.File.ReadAllTextAsync(ruta, Encoding.UTF8);
                var opciones = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var lista = JsonSerializer.Deserialize<List<Tarea>>(json, opciones) ?? new List<Tarea>();

                lista.Insert(0, nuevaTarea);

                var jsonSalida = JsonSerializer.Serialize(lista, new JsonSerializerOptions { WriteIndented = true });
                await System.IO.File.WriteAllTextAsync(ruta, jsonSalida, new UTF8Encoding(false));

                return RedirectToPage("./Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"No se pudo crear la tarea: {ex.Message}");
                CargarTareasDesdeJson();
                CalcularPaginacion();
                ObtenerTareasPaginaActual();
                return Page();
            }
        }

        // EDITAR TAREA
        public string ConvertirAFechaISO(string fechaDDMMYYYY)
        {
            if (DateTime.TryParseExact(fechaDDMMYYYY, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var fecha))
                return fecha.ToString("yyyy-MM-dd");
            return DateTime.Today.ToString("yyyy-MM-dd");
        }

        public async Task<IActionResult> OnPostEditarAsync(string IdTareaEditar, string NombreEditadoDeLaTarea, string FechaEditadaDeVencimiento)
        {
            try
            {
                var nombre = (NombreEditadoDeLaTarea ?? "").Trim();
                var fecha = DateTime.ParseExact(FechaEditadaDeVencimiento, "yyyy-MM-dd", CultureInfo.InvariantCulture);

                var ruta = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "tareas.json");
                var json = await System.IO.File.ReadAllTextAsync(ruta, Encoding.UTF8);
                var opciones = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var lista = JsonSerializer.Deserialize<List<Tarea>>(json, opciones) ?? new List<Tarea>();

                var tarea = lista.FirstOrDefault(t => t.IdTarea == IdTareaEditar);
                if (tarea == null)
                    throw new InvalidOperationException("No se encontró la tarea a editar.");

                tarea.NombreDeLaTarea = nombre;
                tarea.FechaDeVencimiento = fecha.ToString("dd/MM/yyyy");

                var jsonSalida = JsonSerializer.Serialize(lista, new JsonSerializerOptions { WriteIndented = true });
                await System.IO.File.WriteAllTextAsync(ruta, jsonSalida, new UTF8Encoding(false));

                return RedirectToPage("./Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"No se pudo editar la tarea: {ex.Message}");
                CargarTareasDesdeJson();
                AplicarBusqueda();
                CalcularPaginacion();
                ObtenerTareasPaginaActual();
                return Page();
            }
        }

        // BUSQUEDA
        private void AplicarBusqueda()
        {
            var termino = (TextoDeBusqueda ?? "").Trim();
            if (termino.Length == 0) return;

            var terminoNormalizado = NormalizarTexto(termino);

            ListaCompletaDeTareas = FiltrarPorBusqueda(ListaCompletaDeTareas, terminoNormalizado)
                .OrderBy(t => CalcularRelevancia(NormalizarTexto(t.NombreDeLaTarea ?? ""), terminoNormalizado))
                .ToList();

            Pagina = 1;
        }

        private IEnumerable<Tarea> FiltrarPorBusqueda(IEnumerable<Tarea> tareas, string terminoNormalizado)
        {
            foreach (var t in tareas)
            {
                var nombreNormalizado = NormalizarTexto(t.NombreDeLaTarea ?? "");
                if (CoincideTexto(nombreNormalizado, terminoNormalizado))
                    yield return t;
            }
        }

        private (int empieza, int indice, int diferenciaLongitud, string nombre) CalcularRelevancia(string nombreNormalizado, string terminoNormalizado)
        {
            var comienzaCon = nombreNormalizado.StartsWith(terminoNormalizado, StringComparison.Ordinal) ? 0 : 1;
            var indice = nombreNormalizado.IndexOf(terminoNormalizado, StringComparison.Ordinal);
            var diferenciaLongitud = Math.Abs(nombreNormalizado.Length - terminoNormalizado.Length);
            if (indice < 0) indice = int.MaxValue;
            return (comienzaCon, indice, diferenciaLongitud, nombreNormalizado);
        }

        private static bool CoincideTexto(string nombreNormalizado, string terminoNormalizado)
        {
            if (string.IsNullOrEmpty(terminoNormalizado)) return true;
            return nombreNormalizado.IndexOf(terminoNormalizado, StringComparison.Ordinal) >= 0;
        }

        private static string NormalizarTexto(string texto)
        {
            var descompuesto = texto.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder(descompuesto.Length);
            foreach (var c in descompuesto)
            {
                var categoria = CharUnicodeInfo.GetUnicodeCategory(c);
                if (categoria != UnicodeCategory.NonSpacingMark) sb.Append(c);
            }
            return sb.ToString().Normalize(NormalizationForm.FormC).ToLowerInvariant();
        }
    }
}