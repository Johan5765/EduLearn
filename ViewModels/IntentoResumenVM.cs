namespace Aula.ViewModels
{
    public class IntentoResumenVM
    {
        public int IntentoId { get; set; }
        public string? Evaluacion { get; set; }
        public DateTime Fecha { get; set; }
        public int Correctas { get; set; }
        public int Total { get; set; }
        public decimal Porcentaje { get; set; }
    }

}
