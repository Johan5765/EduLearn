using Aula.Data;
using Aula.Models;
using Aula.Services;
using Aula.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Aula.Controllers;

// AJUSTA: AulaDbContext, Evaluacione, Leccione, Pregunta y los DbSet
// (Evaluaciones, Lecciones, Preguntas) según tu scaffold.
// Se asume que Evaluacione tiene la colección "Pregunta" (como en tu consulta de Gestionar).
[Authorize(Roles = "Administrador,Profesor")]
public class EvaluacionesController(EduLearnDbContext _context,CursoAccesoService _acceso) : Controller
{
    
    // ---------- Helpers ----------
    // Carga la evaluación (con preguntas) y su lección solo si el usuario puede editar el curso
    private async Task<(Evaluacione? Ev, Leccione? Leccion)> CargarEditableAsync(int evaluacionId)
    {
        var ev = await _context.Evaluaciones
            .Include(e => e.Pregunta)
            .FirstOrDefaultAsync(e => e.Id == evaluacionId);
        if (ev == null) return (null, null);

        var leccion = await _context.Lecciones.AsNoTracking()
            .FirstOrDefaultAsync(l => l.Id == ev.LeccionId);
        if (leccion == null || !await _acceso.PuedeEditarAsync(User, leccion.CursoId))
            return (null, null);

        return (ev, leccion);
    }

    private static EvaluacionEditViewModel ConstruirVm(Evaluacione ev, Leccione leccion, string? titulo = null) => new()
    {
        Id = ev.Id,
        LeccionId = ev.LeccionId,
        CursoId = leccion.CursoId,
        LeccionTitulo = leccion.Titulo,
        Titulo = titulo ?? ev.Titulo,
        Preguntas = ev.Pregunta
            .OrderBy(p => p.Id)
            .Select(p => new PreguntaItemVM { Id = p.Id, Enunciado = p.Enunciado })
            .ToList()
    };

    // ---------- CREAR ----------
    // GET /Evaluaciones/Crear?leccionId=3
    [HttpGet]
    public async Task<IActionResult> Crear(int leccionId)
    {
        var leccion = await _context.Lecciones.AsNoTracking().FirstOrDefaultAsync(l => l.Id == leccionId);
        if (leccion == null || !await _acceso.PuedeEditarAsync(User, leccion.CursoId))
            return NotFound();

        return View(new EvaluacionFormViewModel
        {
            LeccionId = leccionId,
            CursoId = leccion.CursoId,
            LeccionTitulo = leccion.Titulo
        });
    }

    // POST /Evaluaciones/Crear
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(EvaluacionFormViewModel vm)
    {
        var leccion = await _context.Lecciones.AsNoTracking().FirstOrDefaultAsync(l => l.Id == vm.LeccionId);
        if (leccion == null || !await _acceso.PuedeEditarAsync(User, leccion.CursoId))
            return NotFound();

        if (!ModelState.IsValid)
        {
            vm.CursoId = leccion.CursoId;
            vm.LeccionTitulo = leccion.Titulo;
            return View(vm);
        }

        var ev = new Evaluacione { LeccionId = vm.LeccionId, Titulo = vm.Titulo.Trim() };
        _context.Evaluaciones.Add(ev);
        await _context.SaveChangesAsync();

        TempData["Ok"] = "Evaluación creada. Ahora agrega las preguntas.";
        return RedirectToAction(nameof(Editar), new { id = ev.Id });   // directo a agregar preguntas
    }

    // ---------- EDITAR ----------
    // GET /Evaluaciones/Editar/5
    [HttpGet]
    public async Task<IActionResult> Editar(int id)
    {
        var (ev, leccion) = await CargarEditableAsync(id);
        if (ev == null || leccion == null) return NotFound();

        return View(ConstruirVm(ev, leccion));
    }

    // POST /Evaluaciones/Editar/5  (guarda solo el título)
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(int id, EvaluacionFormViewModel vm)
    {
        var (ev, leccion) = await CargarEditableAsync(id);
        if (ev == null || leccion == null) return NotFound();

        if (!ModelState.IsValid)
            return View(ConstruirVm(ev, leccion, vm.Titulo));

        ev.Titulo = vm.Titulo.Trim();
        await _context.SaveChangesAsync();

        TempData["Ok"] = "Título actualizado.";
        return RedirectToAction(nameof(Editar), new { id });
    }

    // ---------- PREGUNTAS ----------
    // POST /Evaluaciones/AgregarPregunta
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> AgregarPregunta(int evaluacionId, string? enunciado)
    {
        var (ev, _) = await CargarEditableAsync(evaluacionId);
        if (ev == null) return NotFound();

        enunciado = enunciado?.Trim();
        if (string.IsNullOrEmpty(enunciado))
        {
            TempData["Error"] = "Escribe el enunciado de la pregunta.";
        }
        else if (enunciado.Length > 500)
        {
            TempData["Error"] = "El enunciado no puede superar los 500 caracteres.";
        }
        else
        {
            _context.Preguntas.Add(new Pregunta { EvaluacionId = evaluacionId, Enunciado = enunciado });
            await _context.SaveChangesAsync();
            TempData["Ok"] = "Pregunta agregada.";
        }

        return RedirectToAction(nameof(Editar), new { id = evaluacionId });
    }

    // POST /Evaluaciones/EliminarPregunta/9
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> EliminarPregunta(int id)
    {
        var pregunta = await _context.Preguntas.FirstOrDefaultAsync(p => p.Id == id);
        if (pregunta == null) return NotFound();

        var (ev, _) = await CargarEditableAsync(pregunta.EvaluacionId);
        if (ev == null) return NotFound();

        _context.Preguntas.Remove(pregunta);
        await _context.SaveChangesAsync();

        TempData["Ok"] = "Pregunta eliminada.";
        return RedirectToAction(nameof(Editar), new { id = pregunta.EvaluacionId });
    }

    // ---------- ELIMINAR EVALUACIÓN ----------
    // POST /Evaluaciones/Eliminar/5
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Eliminar(int id)
    {
        var (ev, leccion) = await CargarEditableAsync(id);
        if (ev == null || leccion == null) return NotFound();

        _context.Evaluaciones.Remove(ev);   // las preguntas se borran por ON DELETE CASCADE
        await _context.SaveChangesAsync();

        TempData["Ok"] = "Evaluación eliminada.";
        return RedirectToAction("Gestionar", "Curso", new { id = leccion.CursoId });
    }
}