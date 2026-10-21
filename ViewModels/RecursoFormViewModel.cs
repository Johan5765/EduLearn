using System.ComponentModel.DataAnnotations;
using Aula.Models;

namespace Aula.ViewModels;

public class RecursoFormViewModel
{
    public int LeccionId { get; set; }
    public int CursoId { get; set; }
    public string? LeccionTitulo { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(200, ErrorMessage = "Máximo 200 caracteres.")]
    public string Nombre { get; set; } = "";

    public TipoRecurso Tipo { get; set; } = TipoRecurso.Documento;

    // Solo para videos / enlaces
    [StringLength(500, ErrorMessage = "Máximo 500 caracteres.")]
    public string? Url { get; set; }

    // Solo para documentos
    public IFormFile? Archivo { get; set; }
}