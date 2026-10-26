
using System.Security.Claims;
using Aula.Data;
using Aula.Models;
using Aula.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Aula.Controllers;

[Authorize(Roles = "Administrador,Profesor")]
public class LeccionesController(EduLearnDbContext _context) : Controller
{
    // ---------- Helpers ----------

    private bool TryGetUserId(out int userId) =>
        int.TryParse(
            User.FindFirstValue(ClaimTypes.NameIdentifier),
            out userId);

    private Task<Curso?> ObtenerCursoPropioAsync(int cursoId, int userId)
    {
        if (User.IsInRole("Administrador"))
        {
            return _context.Cursos
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.CursoId == cursoId);
        }

        return _context.Cursos
            .AsNoTracking()
            .FirstOrDefaultAsync(c =>
                c.CursoId == cursoId &&
                c.ProfesorId == userId);
    }
    private Task<Leccione?> ObtenerLeccionPropiaAsync(int id, int userId) =>
        _context.Lecciones.FirstOrDefaultAsync(l =>
            l.Id == id &&
            _context.Cursos.Any(c =>
                c.CursoId == l.CursoId &&
                c.ProfesorId == userId));

    private IActionResult VolverAGestionarCurso(int cursoId) =>
        RedirectToAction("Gestionar", "Curso", new { id = cursoId });

    private static LeccionFormViewModel CrearFormulario(
        Leccione leccion,
        string cursoTitulo) =>
        new()
        {
            Id = leccion.Id,
            CursoId = leccion.CursoId,
            CursoTitulo = cursoTitulo,
            Titulo = leccion.Titulo,
            Orden = leccion.Orden
        };

    // ---------- CREAR ----------

    [HttpGet]
    public async Task<IActionResult> Crear(int cursoId)
    {
        if (!TryGetUserId(out var userId))
            return Forbid();

        var curso = await ObtenerCursoPropioAsync(cursoId, userId);

        if (curso is null)
            return NotFound();

        var siguienteOrden = (await _context.Lecciones
            .Where(l => l.CursoId == cursoId)
            .MaxAsync(l => (int?)l.Orden) ?? 0) + 1;

        return View(new LeccionFormViewModel
        {
            CursoId = cursoId,
            CursoTitulo = curso.Titulo,
            Orden = siguienteOrden
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(LeccionFormViewModel vm)
    {
        if (!TryGetUserId(out var userId))
            return Forbid();

        var curso = await ObtenerCursoPropioAsync(vm.CursoId, userId);

        if (curso is null)
            return NotFound();

        if (!ModelState.IsValid)
        {
            vm.CursoTitulo = curso.Titulo;
            return View(vm);
        }

        var leccion = new Leccione
        {
            CursoId = curso.CursoId,
            Titulo = vm.Titulo.Trim(),
            Orden = vm.Orden
        };

        _context.Lecciones.Add(leccion);
        await _context.SaveChangesAsync();

        TempData["Ok"] = "Lección creada correctamente.";

        return VolverAGestionarCurso(curso.CursoId);
    }

    // ---------- EDITAR ----------

    [HttpGet]
    public async Task<IActionResult> Editar(int id)
    {
        if (!TryGetUserId(out var userId))
            return Forbid();

        var leccion = await ObtenerLeccionPropiaAsync(id, userId);

        if (leccion is null)
            return NotFound();

        var curso = await ObtenerCursoPropioAsync(
            leccion.CursoId, userId);

        if (curso is null)
            return NotFound();

        return View(CrearFormulario(leccion, curso.Titulo));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(
        int id,
        LeccionFormViewModel vm)
    {
        if (id != vm.Id)
            return BadRequest();

        if (!TryGetUserId(out var userId))
            return Forbid();

        var leccion = await ObtenerLeccionPropiaAsync(id, userId);

        if (leccion is null)
            return NotFound();

        var curso = await ObtenerCursoPropioAsync(
            leccion.CursoId, userId);

        if (curso is null)
            return NotFound();

        if (!ModelState.IsValid)
        {
            vm.CursoId = leccion.CursoId;
            vm.CursoTitulo = curso.Titulo;

            return View(vm);
        }

        leccion.Titulo = vm.Titulo.Trim();
        leccion.Orden = vm.Orden;

        await _context.SaveChangesAsync();

        TempData["Ok"] = "Lección actualizada.";

        return VolverAGestionarCurso(leccion.CursoId);
    }

    // ---------- ELIMINAR ----------

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Eliminar(int id)
    {
        if (!TryGetUserId(out var userId))
            return Forbid();

        var leccion = await ObtenerLeccionPropiaAsync(id, userId);

        if (leccion is null)
            return NotFound();

        var cursoId = leccion.CursoId;

        _context.Lecciones.Remove(leccion);
        await _context.SaveChangesAsync();

        TempData["Ok"] = "Lección eliminada.";

        return VolverAGestionarCurso(cursoId);
    }
}
