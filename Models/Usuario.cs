using System;
using System.Collections.Generic;

namespace Aula.Models;

public partial class Usuario
{
    public int UsuarioId { get; set; }

    public string Nombre { get; set; } = null!;

    public string Correo { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public int RolId { get; set; }

    public bool Activo { get; set; }

    public virtual ICollection<Curso> Cursos { get; set; } = new List<Curso>();

    public virtual ICollection<IntentosEvaluacion> IntentosEvaluacions { get; set; } = new List<IntentosEvaluacion>();

    public virtual Role Rol { get; set; } = null!;
}
