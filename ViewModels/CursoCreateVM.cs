using Microsoft.AspNetCore.Mvc.Rendering;

namespace Aula.ViewModels
{
    public class CursoCreateVM
    {
        public string Titulo { get; set; } = null!;

        public string? Descripcion { get; set; }

        public int ProfesorId { get; set; }

        public bool Activo { get; set; } = true;

        public IEnumerable<SelectListItem>? Profesores { get; set; }
    }
}