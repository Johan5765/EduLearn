namespace Aula.ViewModels
{
    public class CursoListVM
    {
        public int CursoId { get; set; }

        public string Titulo { get; set; } = null!;

        public string? Descripcion { get; set; }

        public string NombreProfesor { get; set; } = null!;

        public bool Activo { get; set; }
    }
}