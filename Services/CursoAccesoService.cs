using System.Security.Claims;
using Aula.Data;
using Aula.Models;
using Microsoft.EntityFrameworkCore;

namespace Aula.Services;

// Centraliza quién puede VER y quién puede EDITAR un curso.
// AJUSTA: AulaDbContext y los nombres de rol ("Administrador", "Profesor", "Estudiante").
public class CursoAccesoService(EduLearnDbContext _context)
{
    public static bool TryGetUserId(ClaimsPrincipal user, out int userId) =>
        int.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out userId);

    // Administrador: cualquier curso. Profesor: solo los suyos.
    public async Task<bool> PuedeEditarAsync(ClaimsPrincipal user, int cursoId)
    {
        if (!TryGetUserId(user, out var userId)) return false;

        if (user.IsInRole("Administrador"))
            return await _context.Cursos.AnyAsync(c => c.CursoId == cursoId);

        if (user.IsInRole("Profesor"))
            return await _context.Cursos.AnyAsync(c => c.CursoId == cursoId && c.ProfesorId == userId);

        return false;
    }

    // Quien puede editar, o el estudiante inscrito.
    public async Task<bool> PuedeVerAsync(ClaimsPrincipal user, int cursoId)
    {
        if (await PuedeEditarAsync(user, cursoId)) return true;
        if (!TryGetUserId(user, out var userId)) return false;

        if (user.IsInRole("Estudiante"))
            return await _context.Inscripciones
                .AnyAsync(i => i.CursoId == cursoId && i.EstudianteId == userId);

        return false;
    }
}