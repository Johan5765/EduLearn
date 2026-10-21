using System;
using System.Collections.Generic;

namespace Aula.Models;

public partial class Recurso
{
    public int Id { get; set; }

    public int LeccionId { get; set; }

    public string Nombre { get; set; } = null!;

    public byte Tipo { get; set; }

    public string Url { get; set; } = null!;

    public virtual Leccione Leccion { get; set; } = null!;
}

public enum TipoRecurso : byte
{
    Documento = 0,
    Video = 1
}
