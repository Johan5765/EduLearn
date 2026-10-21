using System;
using System.Collections.Generic;

namespace Aula.Models;

public partial class Leccione
{
    public int Id { get; set; }

    public int CursoId { get; set; }

    public string Titulo { get; set; } = null!;

    public int Orden { get; set; }

    public virtual Curso Curso { get; set; } = null!;

    public virtual ICollection<Evaluacione> Evaluaciones { get; set; } = new List<Evaluacione>();

    public virtual ICollection<Recurso> Recursos { get; set; } = new List<Recurso>();
}
