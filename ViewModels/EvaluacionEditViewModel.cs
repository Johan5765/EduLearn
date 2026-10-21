namespace Aula.ViewModels
{
    // Pantalla de edición: título + lista de preguntas
    public class EvaluacionEditViewModel : EvaluacionFormViewModel
    {
        public List<PreguntaItemVM> Preguntas { get; set; } = new();
    }
}
