using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Supabase;
using System.Threading.Tasks;
using System.Linq;

namespace ProyectoWebIDP.Pages
{
    public class ResultadosModel : PageModel
    {
        private readonly Client _supabase;

        public ResultadosModel(Client supabase)
        {
            _supabase = supabase;
        }

        // Aquí guardaremos los datos que traigamos de la DB
        public ResultadoDisc Perfil { get; set; }

        public async Task<IActionResult> OnGetAsync(string usuarioId)
        {
            if (string.IsNullOrEmpty(usuarioId)) return RedirectToPage("/Index");

            try
            {
                // Consultamos Supabase buscando el registro de este usuario
                var result = await _supabase
                    .From<ResultadoDisc>()
                    .Where(x => x.UsuarioId == usuarioId)
                    .Get();

                // Asignamos el primer resultado encontrado a nuestra propiedad
                Perfil = result.Models.FirstOrDefault();

                if (Perfil == null)
                {
                    return RedirectToPage("/Index");
                }

                return Page();
            }
            catch (System.Exception)
            {
                return RedirectToPage("/Index");
            }
        }
    }
}