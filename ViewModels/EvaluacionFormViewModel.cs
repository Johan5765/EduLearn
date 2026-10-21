using System.ComponentModel.DataAnnotations;

namespace Aula.ViewModels;

// Se usa para Crear y para guardar el título en Editar
public class EvaluacionFormViewModel
{
    public int Id { get; set; }
    public int LeccionId { get; set; }
    public int CursoId { get; set; }
    public string? LeccionTitulo { get; set; }

    [Required(ErrorMessage = "El título es obligatorio.")]
    [StringLength(200, ErrorMessage = "Máximo 200 caracteres.")]
    public string Titulo { get; set; } = "";
}


