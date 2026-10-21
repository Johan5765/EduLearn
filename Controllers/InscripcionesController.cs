using Aula.Data;
using Aula.Models;
using Aula.Services;
using Aula.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Aula.Controllers;

// AJUSTA: AulaDbContext y los nombres de tu scaffold:
//   Inscripcione (Id, CursoId, EstudianteId, FechaInscripcion) | DbSet Inscripciones
//   Usuarios (UsuarioId, Nombre, Activo, Rol.Nombre)           | DbSet Usuarios
// Los nombres de rol deben coincidir con Rol.Nombre en tu BD.
[Authorize]
public class InscripcionesController(EduLearnDbContext _context,CursoAccesoService _acceso) : Controller
{
    private const string RolEstudiante = "Estudiante";
    private const string Staff = "Administrador,Profesor";


    // =====================================================
    //  ESTUDIANTE
    // =====================================================

    // GET /Inscripciones/Catalogo?q=algoritmos
    [HttpGet]
    [Authorize(Roles = RolEstudiante)]
    public async Task<IActionResult> Catalogo(string? q)
    {
        if (!CursoAccesoService.TryGetUserId(User, out var userId)) return Forbid();

        q = q?.Trim();
        var query = _context.Cursos.AsNoTracking().AsQueryable();
        if (!string.IsNullOrEmpty(q))
            query = query.Where(c => c.Titulo.Contains(q));

        var cursos = await query
            .OrderBy(c => c.Titulo)
            .Select(c => new CursoCatalogoVM
            {
                CursoId = c.CursoId,
                Titulo = c.Titulo,
                Descripcion = c.Descripcion,
                Profesor = _context.Usuarios
                    .Where(u => u.UsuarioId == c.ProfesorId)
                    .Select(u => u.Nombre)
                    .FirstOrDefault(),
                TotalLecciones = c.Lecciones.Count,
                Inscrito = _context.Inscripciones
                    .Any(i => i.CursoId == c.CursoId && i.EstudianteId == userId)
            })
            .ToListAsync();

        return View(new CatalogoVM { Busqueda = q, Cursos = cursos });
    }

    // GET /Inscripciones/MisCursos
    [HttpGet]
    [Authorize(Roles = RolEstudiante)]
    public async Task<IActionResult> MisCursos()
    {
        if (!CursoAccesoService.TryGetUserId(User, out var userId)) return Forbid();

        var cursos = await (
            from i in _context.Inscripciones.AsNoTracking()
            join c in _context.Cursos.AsNoTracking() on i.CursoId equals c.CursoId
            where i.EstudianteId == userId
            orderby i.FechaInscripcion descending
            select new MiCursoVM
            {
                CursoId = c.CursoId,
                Titulo = c.Titulo,
                Descripcion = c.Descripcion,
                Profesor = _context.Usuarios
                    .Where(u => u.UsuarioId == c.ProfesorId)
                    .Select(u => u.Nombre)
                    .FirstOrDefault(),
                TotalLecciones = c.Lecciones.Count,
                FechaInscripcion = i.FechaInscripcion
            }).ToListAsync();

        return View(cursos);
    }

    // POST /Inscripciones/Inscribir
    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Roles = RolEstudiante)]
    public async Task<IActionResult> Inscribir(int cursoId)
    {
        if (!CursoAccesoService.TryGetUserId(User, out var userId)) return Forbid();

        if (!await _context.Cursos.AnyAsync(c => c.CursoId == cursoId))
            return NotFound();

        var yaInscrito = await _context.Inscripciones
            .AnyAsync(i => i.CursoId == cursoId && i.EstudianteId == userId);

        if (yaInscrito)
        {
            TempData["Ok"] = "Ya estabas inscrito en este curso.";
        }
        else
        {
            _context.Inscripciones.Add(new Inscripcione
            {
                CursoId = cursoId,
                EstudianteId = userId,
                FechaInscripcion = DateTime.Now
            });

            try
            {
                await _context.SaveChangesAsync();
                TempData["Ok"] = "¡Te inscribiste correctamente!";
            }
            catch (DbUpdateException)
            {
                // doble clic o dos pestañas: la restricción única UQ_Inscripcion lo detuvo
                TempData["Ok"] = "Ya estabas inscrito en este curso.";
            }
        }

        return RedirectToAction("Gestionar", "Curso", new { id = cursoId });
    }

    // POST /Inscripciones/CancelarInscripcion
    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Roles = RolEstudiante)]
    public async Task<IActionResult> CancelarInscripcion(int cursoId)
    {
        if (!CursoAccesoService.TryGetUserId(User, out var userId)) return Forbid();

        var inscripcion = await _context.Inscripciones
            .FirstOrDefaultAsync(i => i.CursoId == cursoId && i.EstudianteId == userId);
        if (inscripcion == null) return NotFound();

        _context.Inscripciones.Remove(inscripcion);
        await _context.SaveChangesAsync();   // sus intentos de evaluaciones se conservan

        TempData["Ok"] = "Cancelaste tu inscripción.";
        return RedirectToAction(nameof(MisCursos));
    }

    // =====================================================
    //  PROFESOR / ADMINISTRADOR
    // =====================================================

    // GET /Inscripciones/Inscritos?cursoId=5
    [HttpGet]
    [Authorize(Roles = Staff)]
    public async Task<IActionResult> Inscritos(int cursoId)
    {
        if (!await _acceso.PuedeEditarAsync(User, cursoId)) return NotFound();

        var curso = await _context.Cursos.AsNoTracking().FirstOrDefaultAsync(c => c.CursoId == cursoId);
        if (curso == null) return NotFound();

        var inscritos = await (
            from i in _context.Inscripciones.AsNoTracking()
            join u in _context.Usuarios.AsNoTracking() on i.EstudianteId equals u.UsuarioId
            where i.CursoId == cursoId
            orderby u.Nombre
            select new InscritoVM
            {
                InscripcionId = i.Id,
                EstudianteId = u.UsuarioId,
                Nombre = u.Nombre,
                Fecha = i.FechaInscripcion
            }).ToListAsync();

        var disponibles = await _context.Usuarios.AsNoTracking()
            .Where(u => u.Rol.Nombre == RolEstudiante && u.Activo &&
                        !_context.Inscripciones.Any(i => i.CursoId == cursoId && i.EstudianteId == u.UsuarioId))
            .OrderBy(u => u.Nombre)
            .Select(u => new EstudianteOpcionVM { Id = u.UsuarioId, Nombre = u.Nombre })
            .ToListAsync();

        return View(new InscritosVM
        {
            CursoId = cursoId,
            CursoTitulo = curso.Titulo,
            Inscritos = inscritos,
            Disponibles = disponibles
        });
    }

    // POST /Inscripciones/AgregarEstudiante
    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Roles = Staff)]
    public async Task<IActionResult> AgregarEstudiante(int cursoId, int estudianteId)
    {
        if (!await _acceso.PuedeEditarAsync(User, cursoId)) return NotFound();

        var esEstudianteValido = await _context.Usuarios.AnyAsync(u =>
            u.UsuarioId == estudianteId && u.Rol.Nombre == RolEstudiante && u.Activo);
        var yaInscrito = await _context.Inscripciones
            .AnyAsync(i => i.CursoId == cursoId && i.EstudianteId == estudianteId);

        if (!esEstudianteValido)
        {
            TempData["Error"] = "El estudiante seleccionado no es válido.";
        }
        else if (yaInscrito)
        {
            TempData["Error"] = "Ese estudiante ya está inscrito.";
        }
        else
        {
            _context.Inscripciones.Add(new Inscripcione
            {
                CursoId = cursoId,
                EstudianteId = estudianteId,
                FechaInscripcion = DateTime.Now
            });
            await _context.SaveChangesAsync();
            TempData["Ok"] = "Estudiante inscrito.";
        }

        return RedirectToAction(nameof(Inscritos), new { cursoId });
    }

    // POST /Inscripciones/QuitarEstudiante/12  (id = Id de la inscripción)
    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Roles = Staff)]
    public async Task<IActionResult> QuitarEstudiante(int id)
    {
        var inscripcion = await _context.Inscripciones.FirstOrDefaultAsync(i => i.Id == id);
        if (inscripcion == null) return NotFound();

        if (!await _acceso.PuedeEditarAsync(User, inscripcion.CursoId)) return NotFound();

        var cursoId = inscripcion.CursoId;
        _context.Inscripciones.Remove(inscripcion);
        await _context.SaveChangesAsync();

        TempData["Ok"] = "Estudiante removido del curso.";
        return RedirectToAction(nameof(Inscritos), new { cursoId });
    }
}