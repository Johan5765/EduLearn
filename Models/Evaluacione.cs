using System;
using System.Collections.Generic;

namespace Aula.Models;

public partial class Evaluacione
{
    public int Id { get; set; }

    public int LeccionId { get; set; }

    public string Titulo { get; set; } = null!;

    public virtual ICollection<IntentosEvaluacion> IntentosEvaluacions { get; set; } = new List<IntentosEvaluacion>();

    public virtual Leccione Leccion { get; set; } = null!;

    public virtual ICollection<Pregunta> Pregunta { get; set; } = new List<Pregunta>();
}
