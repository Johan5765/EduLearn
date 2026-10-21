namespace Aula.ViewModels
{
    public class CursoResumenVM
    {
        public int CursoId { get; set; }
        public string Titulo { get; set; } = "";
        public int Lecciones { get; set; }
        public int? Inscritos { get; set; }
        public string? Profesor { get; set; }
    }

}
