namespace Aula.ViewModels
{
    public class CursoGestionViewModel
    {
        public int CursoId { get; set; }
        public string Titulo { get; set; } = "";
        public string? Descripcion { get; set; }

        public int TotalLecciones { get; set; }
        public int TotalRecursos { get; set; }
        public int TotalEvaluaciones { get; set; }
        public bool PuedeEditar { get; set; }      // Admin o Profesor dueño
        public string RolEtiqueta { get; set; } = "";

        public List<LeccionGestionVM> Lecciones { get; set; } = new();

    }
}
