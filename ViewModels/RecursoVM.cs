using Aula.Models;

namespace Aula.ViewModels
{
    public class RecursoVM
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = "";
        public byte Tipo { get; set; }
        public string Url { get; set; } = "";
    }
}
