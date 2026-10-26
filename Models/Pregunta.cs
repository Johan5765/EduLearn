using System;
using System.Collections.Generic;

namespace Aula.Models;

public partial class Pregunta
{
    public int Id { get; set; }

    public int EvaluacionId { get; set; }

    public string Enunciado { get; set; } = null!;

    public virtual Evaluacione Evaluacion { get; set; } = null!;

    public virtual ICollection<OpcionesRespuestum> OpcionesRespuesta { get; set; } = new List<OpcionesRespuestum>();

    public virtual ICollection<RespuestasEstudiante> RespuestasEstudiantes { get; set; } = new List<RespuestasEstudiante>();
}
