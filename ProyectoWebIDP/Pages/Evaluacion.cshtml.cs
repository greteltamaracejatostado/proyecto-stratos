using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Supabase;
using System;
using System.Threading.Tasks;

namespace ProyectoWebIDP.Pages
{
    public class EvaluacionModel : PageModel
    {
        private readonly Supabase.Client _supabase; // ← Especificar Supabase.Client

        public EvaluacionModel(Supabase.Client supabase) // ← Especificar Supabase.Client
        {
            _supabase = supabase;
        }

        
        [BindProperty(SupportsGet = true)]
        public string UsuarioId { get; set; } = string.Empty;

        [BindProperty]
        public string ResultadoFinal { get; set; } = string.Empty;

        // Propiedades que el JavaScript del HTML llenará automáticamente
        [BindProperty] public int ZMas { get; set; }
        [BindProperty] public int EstrellaMas { get; set; }
        [BindProperty] public int CuadradoMas { get; set; }
        [BindProperty] public int TrianguloMas { get; set; }

        [BindProperty] public int ZMenos { get; set; }
        [BindProperty] public int EstrellaMenos { get; set; }
        [BindProperty] public int CuadradoMenos { get; set; }
        [BindProperty] public int TrianguloMenos { get; set; }

        public async Task<IActionResult> OnGet(string usuarioId)
        {
            UsuarioId = usuarioId;
            
            // VERIFICACIÓN CRÍTICA
            var existe = await _supabase
                .From<Usuario>()
                .Where(u => u.Id == usuarioId)
                .Get();
            
            if (existe.Models.Count == 0)
            {
                Console.WriteLine($"--- ERROR: Usuario {usuarioId} NO EXISTE en OnGet ---");
                // Redirigir al registro si no existe
                return RedirectToPage("/Index");
            }
            
            Console.WriteLine($"--- Usuario {usuarioId} existe en OnGet ---");
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {

            var existe = await _supabase
                .From<Usuario>()
                .Where(u => u.Id == UsuarioId)
                .Get();
            
            if (existe.Models.Count == 0)
            {
                Console.WriteLine($"--- ERROR: Usuario {UsuarioId} NO EXISTE en OnPost ---");
                ResultadoFinal = "Error: El usuario no está registrado correctamente. Por favor, vuelve a empezar.";
                return Page();
            }
            // 1. Cálculo de diferencias matemáticas (DIF = MÁS - MENOS)
            int zDif = ZMas - ZMenos;
            int estrellaDif = EstrellaMas - EstrellaMenos;
            int cuadradoDif = CuadradoMas - CuadradoMenos;
            int trianguloDif = TrianguloMas - TrianguloMenos;

            // LOGS DE DEPURACION
            Console.WriteLine("\n==========================================");
            Console.WriteLine($"PROCESANDO TEST PARA: {UsuarioId}");
            Console.WriteLine("------------------------------------------");
            Console.WriteLine($"Z (D)      | MÁS: {ZMas} | MENOS: {ZMenos} | DIF: {zDif}");
            Console.WriteLine($"Cuadrado(I)| MÁS: {CuadradoMas} | MENOS: {CuadradoMenos} | DIF: {cuadradoDif}");
            Console.WriteLine($"Triang.(S) | MÁS: {TrianguloMas} | MENOS: {TrianguloMenos} | DIF: {trianguloDif}");
            Console.WriteLine($"Estrella(C)| MÁS: {EstrellaMas} | MENOS: {EstrellaMenos} | DIF: {estrellaDif}");
            Console.WriteLine("==========================================\n");

            try
            {
                // MAPEO AL modelo ResultadoDisc para Supabase
                var resultadoFinalDisc = new ResultadoDisc
                {
                    UsuarioId = UsuarioId,
                    ZMas = ZMas,
                    EstrellaMas = EstrellaMas,
                    CuadradoMas = CuadradoMas,
                    TrianguloMas = TrianguloMas,
                    ZMenos = ZMenos,
                    EstrellaMenos = EstrellaMenos,
                    CuadradoMenos = CuadradoMenos,
                    TrianguloMenos = TrianguloMenos,
                    ZDif = zDif,
                    EstrellaDif = estrellaDif,
                    CuadradoDif = cuadradoDif,
                    TrianguloDif = trianguloDif,
                    LetraPrincipal = DeterminarLetraPrincipal(zDif, estrellaDif, cuadradoDif, trianguloDif)
                };

                // INSERCION EN LA DB
                await _supabase.From<ResultadoDisc>().Insert(resultadoFinalDisc);

                Console.WriteLine("--- INSERCIÓN EN SUPABASE EXITOSA ---");

                return RedirectToPage("/Resultados", new { usuarioId = UsuarioId });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"--- ERROR CRÍTICO EN EVALUACION: {ex.Message} ---");
                ResultadoFinal = "Hubo un problema al guardar tus resultados. Intenta de nuevo.";
                return Page();
            }
        }

        private string DeterminarLetraPrincipal(int z, int e, int c, int t)
        {
            // Comparamos los valores DIF para ver cuál es el más alto
            if (t >= z && t >= e && t >= c) return "D"; // Triángulo
            if (e >= z && e >= t && e >= c) return "I"; // Estrella
            if (z >= t && z >= e && z >= c) return "S"; // Z
            return "C"; // Cuadrado
        }
    }
}