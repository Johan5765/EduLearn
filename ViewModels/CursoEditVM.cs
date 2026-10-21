using Microsoft.AspNetCore.Mvc.Rendering;

namespace Aula.ViewModels
{
    public class CursoEditVM
    {
        public int CursoId { get; set; }

        public string Titulo { get; set; } = null!;

        public string? Descripcion { get; set; }

        public int ProfesorId { get; set; }

        public bool Activo { get; set; }

        public IEnumerable<SelectListItem>? Profesores { get; set; }
    }
}