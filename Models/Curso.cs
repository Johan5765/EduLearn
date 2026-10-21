using System;
using System.Collections.Generic;

namespace Aula.Models;

public partial class Curso
{
    public int CursoId { get; set; }

    public string Titulo { get; set; } = null!;

    public string? Descripcion { get; set; }

    public int ProfesorId { get; set; }

    public bool Activo { get; set; }

    public virtual ICollection<Inscripcione> Inscripciones { get; set; } = new List<Inscripcione>();

    public virtual ICollection<Leccione> Lecciones { get; set; } = new List<Leccione>();

    public virtual Usuario Profesor { get; set; } = null!;
}
