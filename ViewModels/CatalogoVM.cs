namespace Aula.ViewModels
{
    public class CatalogoVM
    {
        public string? Busqueda { get; set; }
        public List<CursoCatalogoVM> Cursos { get; set; } = new();
    }

}
