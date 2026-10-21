using System.Security.Claims;
using Aula.Data;
using Aula.Models;
using Aula.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Aula.Controllers;

// AJUSTA: AulaDbContext, Leccione (entidad), _db.Lecciones y _db.Cursos
// según los nombres que generó tu scaffold.
[Authorize(Roles = "Profesor")]
public class LeccionesController(EduLearnDbContext _context) : Controller
{
    

    // ---------- Helpers ----------
    private bool TryGetUserId(out int userId) =>
        int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out userId);

    // Devuelve el curso solo si pertenece al profesor autenticado
    private Task<Curso?> ObtenerCursoPropioAsync(int cursoId, int userId) =>
        _context.Cursos.AsNoTracking()
            .FirstOrDefaultAsync(c => c.CursoId == cursoId && c.ProfesorId == userId);

    // ---------- CREAR ----------
    // GET /Lecciones/Crear?cursoId=5
    [HttpGet]
    public async Task<IActionResult> Crear(int cursoId)
    {
        if (!TryGetUserId(out var userId)) return Forbid();

        var curso = await ObtenerCursoPropioAsync(cursoId, userId);
        if (curso == null) return NotFound();

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

    // POST /Lecciones/Crear
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(LeccionFormViewModel vm)
    {
        if (!TryGetUserId(out var userId)) return Forbid();

        var curso = await ObtenerCursoPropioAsync(vm.CursoId, userId);
        if (curso == null) return NotFound();

        if (!ModelState.IsValid)
        {
            vm.CursoTitulo = curso.Titulo;
            return View(vm);
        }

        _context.Lecciones.Add(new Leccione
        {
            CursoId = vm.CursoId,
            Titulo = vm.Titulo.Trim(),
            Orden = vm.Orden
        });
        await _context.SaveChangesAsync();

        TempData["Ok"] = "Lección creada correctamente.";
        return RedirectToAction("Gestionar", "Curso", new { id = vm.CursoId });
    }

    // ---------- EDITAR ----------
    // GET /Lecciones/Editar/3
    [HttpGet]
    public async Task<IActionResult> Editar(int id)
    {
        if (!TryGetUserId(out var userId)) return Forbid();

        var leccion = await _context.Lecciones.AsNoTracking().FirstOrDefaultAsync(l => l.Id == id);
        if (leccion == null) return NotFound();

        var curso = await ObtenerCursoPropioAsync(leccion.CursoId, userId);
        if (curso == null) return NotFound();

        return View(new LeccionFormViewModel
        {
            Id = leccion.Id,
            CursoId = leccion.CursoId,
            CursoTitulo = curso.Titulo,
            Titulo = leccion.Titulo,
            Orden = leccion.Orden
        });
    }

    // POST /Lecciones/Editar/3
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(int id, LeccionFormViewModel vm)
    {
        if (id != vm.Id) return BadRequest();
        if (!TryGetUserId(out var userId)) return Forbid();

        var leccion = await _context.Lecciones.FirstOrDefaultAsync(l => l.Id == id);
        if (leccion == null) return NotFound();

        var curso = await ObtenerCursoPropioAsync(leccion.CursoId, userId);
        if (curso == null) return NotFound();

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
        return RedirectToAction("Gestionar", "Curso", new { id = leccion.CursoId });
    }

    // ---------- ELIMINAR ----------
    // POST /Lecciones/Eliminar/3
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Eliminar(int id)
    {
        if (!TryGetUserId(out var userId)) return Forbid();

        var leccion = await _context.Lecciones.FirstOrDefaultAsync(l => l.Id == id);
        if (leccion == null) return NotFound();

        var curso = await ObtenerCursoPropioAsync(leccion.CursoId, userId);
        if (curso == null) return NotFound();

        var cursoId = leccion.CursoId;
        _context.Lecciones.Remove(leccion);   // recursos y evaluaciones se borran por ON DELETE CASCADE
        await _context.SaveChangesAsync();

        TempData["Ok"] = "Lección eliminada.";
        return RedirectToAction("Gestionar", "Curso", new { id = cursoId });
    }
}