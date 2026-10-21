using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Aula.ViewModels
{
    public class UsuarioEditVM
    {
        public int UsuarioId { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El correo es obligatorio")]
        [EmailAddress(ErrorMessage = "Ingrese un correo válido")]
        public string Correo { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 6,
            ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
        public string? Password { get; set; }

        [Required(ErrorMessage = "Seleccione un rol")]
        [Range(1, int.MaxValue, ErrorMessage = "Seleccione un rol válido")]
        public int RolId { get; set; }

        public bool Activo { get; set; }
        public IEnumerable<SelectListItem>? Roles { get; set; }

    }
}
