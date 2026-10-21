
namespace Aula.ViewModels
{
    public class UsuarioListVM
    {
        public int UsuarioId { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public string Correo { get; set; } = string.Empty;

        public string NombreRol { get; set; } = string.Empty;

        public bool Activo { get; set; }
    }
}
