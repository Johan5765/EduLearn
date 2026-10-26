using System;
using System.Collections.Generic;

namespace Aula.Models;

public partial class OpcionesRespuestum
{
    public int Id { get; set; }

    public int PreguntaId { get; set; }

    public string Texto { get; set; } = null!;

    public bool EsCorrecta { get; set; }

    public virtual Pregunta Pregunta { get; set; } = null!;

    public virtual ICollection<RespuestasEstudiante> RespuestasEstudiantes { get; set; } = new List<RespuestasEstudiante>();
}
