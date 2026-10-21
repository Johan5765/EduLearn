namespace Aula.ViewModels
{
    public class CursoCatalogoVM
    {
        public int CursoId { get; set; }
        public string Titulo { get; set; } = "";
        public string? Descripcion { get; set; }
        public string? Profesor { get; set; }
        public int TotalLecciones { get; set; }
        public bool Inscrito { get; set; }
    }

}
