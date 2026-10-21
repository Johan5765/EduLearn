using System;
using System.Collections.Generic;

namespace Aula.Models;

public partial class Inscripcione
{
    public int Id { get; set; }

    public int CursoId { get; set; }

    public int EstudianteId { get; set; }

    public DateTime FechaInscripcion { get; set; }

    public virtual Curso Curso { get; set; } = null!;
}
