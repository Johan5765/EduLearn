namespace Aula.ViewModels
{
    public class InscritosVM
    {
        public int CursoId { get; set; }
        public string CursoTitulo { get; set; } = "";
        public List<InscritoVM> Inscritos { get; set; } = new();
        public List<EstudianteOpcionVM> Disponibles { get; set; } = new();
    }

}
