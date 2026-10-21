using Aula.Models;

namespace Aula.ViewModels
{
    public class LeccionGestionVM
    {
        public int Id { get; set; }
        public int Orden { get; set; }
        public string Titulo { get; set; } = "";
        public List<RecursoVM> Recursos { get; set; } = new();
        public List<EvaluacionVM> Evaluaciones { get; set; } = new();
    }
}
