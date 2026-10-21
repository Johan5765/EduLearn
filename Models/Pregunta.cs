using System;
using System.Collections.Generic;

namespace Aula.Models;

public partial class Pregunta
{
    public int Id { get; set; }

    public int EvaluacionId { get; set; }

    public string Enunciado { get; set; } = null!;

    public virtual Evaluacione Evaluacion { get; set; } = null!;
}
