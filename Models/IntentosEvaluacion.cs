using System;
using System.Collections.Generic;

namespace Aula.Models;

public partial class IntentosEvaluacion
{
    public int Id { get; set; }

    public int EvaluacionId { get; set; }

    public int EstudianteId { get; set; }

    public DateTime FechaInicio { get; set; }

    public DateTime? FechaFinalizacion { get; set; }

    public decimal? Calificacion { get; set; }

    public bool Completado { get; set; }

    public virtual Usuario Estudiante { get; set; } = null!;

    public virtual Evaluacione Evaluacion { get; set; } = null!;

    public virtual ICollection<RespuestasEstudiante> RespuestasEstudiantes { get; set; } = new List<RespuestasEstudiante>();
}
