using System.ComponentModel.DataAnnotations;

namespace Aula.ViewModels;

public class LeccionFormViewModel
{
    public int Id { get; set; }
    public int CursoId { get; set; }
    public string CursoTitulo { get; set; } = "";

    [Required(ErrorMessage = "El título es obligatorio.")]
    [StringLength(200, ErrorMessage = "Máximo 200 caracteres.")]
    public string Titulo { get; set; } = "";

    [Range(1, 999, ErrorMessage = "El orden debe ser un número mayor a 0.")]
    public int Orden { get; set; }
}