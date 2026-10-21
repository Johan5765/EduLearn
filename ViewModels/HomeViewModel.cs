namespace Aula.ViewModels
{
    public class HomeViewModel
    {
        public string Rol { get; set; } = "";
        public List<StatCardVM> Stats { get; set; } = new();
        public List<CursoResumenVM> Cursos { get; set; } = new();
    }

}
