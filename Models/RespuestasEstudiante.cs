using System;
using System.Collections.Generic;

namespace Aula.Models;

public partial class RespuestasEstudiante
{
    public int Id { get; set; }

    public int IntentoId { get; set; }

    public int PreguntaId { get; set; }

    public int? OpcionSeleccionadaId { get; set; }

    public string? RespuestaTexto { get; set; }

    public bool? EsCorrecta { get; set; }

    public decimal? PuntosObtenidos { get; set; }

    public virtual IntentosEvaluacion Intento { get; set; } = null!;

    public virtual OpcionesRespuestum? OpcionSeleccionada { get; set; }

    public virtual Pregunta Pregunta { get; set; } = null!;
}
