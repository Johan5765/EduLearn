namespace Aula.ViewModels
{
    public class MiCursoVM
    {
        public int CursoId { get; set; }
        public string Titulo { get; set; } = "";
        public string? Descripcion { get; set; }
        public string? Profesor { get; set; }
        public int TotalLecciones { get; set; }
        public DateTime FechaInscripcion { get; set; }
    }

}
