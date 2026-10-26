using Aula.Data;
using Aula.Models;
using Aula.Services;
using Aula.ViewModels;
using Aula.Views;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Aula.Controllers;

// AJUSTA: AulaDbContext, Evaluacione, Leccione, Pregunta y los DbSet
// (Evaluaciones, Lecciones, Preguntas) según tu scaffold.
// Se asume que Evaluacione tiene la colección "Pregunta" (como en tu consulta de Gestionar).
[Authorize]
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

    private async Task<EvaluacionEditViewModel> ConstruirVmAsync(Evaluacione ev, Leccione leccion, string? titulo = null)
    {
        var preguntas = await _context.Preguntas
            .Where(p => p.EvaluacionId == ev.Id)
            .OrderBy(p => p.Id)
            .ToListAsync();

        var preguntasVm = new List<PreguntaItemVM>();

        foreach (var p in preguntas)
        {
            var opciones = await _context.OpcionesRespuesta
                .Where(o => o.PreguntaId == p.Id)
                .OrderBy(o => o.Id)
                .Select(o => new OpcionItemVM
                {
                    Id = o.Id,
                    Texto = o.Texto,
                    EsCorrecta = o.EsCorrecta
                })
                .ToListAsync();

            preguntasVm.Add(new PreguntaItemVM
            {
                Id = p.Id,
                Enunciado = p.Enunciado,
                Opciones = opciones
            });
        }

        return new EvaluacionEditViewModel
        {
            Id = ev.Id,
            LeccionId = ev.LeccionId,
            CursoId = leccion.CursoId,
            LeccionTitulo = leccion.Titulo,
            Titulo = titulo ?? ev.Titulo,
            Preguntas = preguntasVm
        };
    }

    // ---------- CREAR ----------
    // GET /Evaluaciones/Crear?leccionId=3
    [Authorize(Roles = "Administrador,Profesor")]
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
    [Authorize(Roles = "Administrador,Profesor")]
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
    [Authorize(Roles = "Administrador,Profesor")]
    [HttpGet]
    public async Task<IActionResult> Editar(int id)
    {
        var (ev, leccion) = await CargarEditableAsync(id);
        if (ev == null || leccion == null) return NotFound();

        return View(await ConstruirVmAsync(ev, leccion));
    }

    // POST /Evaluaciones/Editar/5  (guarda solo el título)
    [Authorize(Roles = "Administrador,Profesor")]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(int id, EvaluacionFormViewModel vm)
    {
        var (ev, leccion) = await CargarEditableAsync(id);
        if (ev == null || leccion == null) return NotFound();

        if (!ModelState.IsValid)
            return View(await ConstruirVmAsync(ev, leccion, vm.Titulo));

        ev.Titulo = vm.Titulo.Trim();
        await _context.SaveChangesAsync();

        TempData["Ok"] = "Título actualizado.";
        return RedirectToAction(nameof(Editar), new { id });
    }

    // ---------- PREGUNTAS ----------
    // POST /Evaluaciones/AgregarPregunta
    [Authorize(Roles = "Administrador,Profesor")]
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
    [Authorize(Roles = "Administrador,Profesor")]
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
    // ---------- OPCIONES DE RESPUESTA ----------
    [Authorize(Roles = "Administrador,Profesor")]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> AgregarOpcion(int preguntaId, string? texto, bool esCorrecta = false)
    {
        var pregunta = await _context.Preguntas.FirstOrDefaultAsync(p => p.Id == preguntaId);
        if (pregunta == null) return NotFound();

        var (ev, _) = await CargarEditableAsync(pregunta.EvaluacionId);
        if (ev == null) return NotFound();

        texto = texto?.Trim();
        if (string.IsNullOrWhiteSpace(texto) || texto.Length > 300)
        {
            TempData["Error"] = "La opción es obligatoria y no puede superar los 300 caracteres.";
            return RedirectToAction(nameof(Editar), new { id = ev.Id });
        }

        if (esCorrecta)
        {
            var opciones = await _context.OpcionesRespuesta
                .Where(o => o.PreguntaId == preguntaId)
                .ToListAsync();

            foreach (var opcion in opciones)
                opcion.EsCorrecta = false;
        }

        _context.OpcionesRespuesta.Add(new OpcionesRespuestum
        {
            PreguntaId = preguntaId,
            Texto = texto,
            EsCorrecta = esCorrecta
        });

        await _context.SaveChangesAsync();
        TempData["Ok"] = "Opción agregada correctamente.";

        return RedirectToAction(nameof(Editar), new { id = ev.Id });
    }
    
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> MarcarOpcionCorrecta(int id)
    {
        var opcion = await _context.OpcionesRespuesta
            .FirstOrDefaultAsync(o => o.Id == id);
        if (opcion == null) return NotFound();

        var pregunta = await _context.Preguntas
            .FirstOrDefaultAsync(p => p.Id == opcion.PreguntaId);
        if (pregunta == null) return NotFound();

        var (ev, _) = await CargarEditableAsync(pregunta.EvaluacionId);
        if (ev == null) return NotFound();

        var opciones = await _context.OpcionesRespuesta
            .Where(o => o.PreguntaId == pregunta.Id)
            .ToListAsync();

        foreach (var item in opciones)
            item.EsCorrecta = item.Id == id;

        await _context.SaveChangesAsync();
        TempData["Ok"] = "Respuesta correcta actualizada.";

        return RedirectToAction(nameof(Editar), new { id = ev.Id });
    }

    [Authorize(Roles = "Administrador,Profesor")]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> EliminarOpcion(int id)
    {
        var opcion = await _context.OpcionesRespuesta.FirstOrDefaultAsync(o => o.Id == id);
        if (opcion == null) return NotFound();

        var pregunta = await _context.Preguntas
            .FirstOrDefaultAsync(p => p.Id == opcion.PreguntaId);
        if (pregunta == null) return NotFound();

        var (ev, _) = await CargarEditableAsync(pregunta.EvaluacionId);
        if (ev == null) return NotFound();

        _context.OpcionesRespuesta.Remove(opcion);
        await _context.SaveChangesAsync();

        TempData["Ok"] = "Opción eliminada.";
        return RedirectToAction(nameof(Editar), new { id = ev.Id });
    }

    // ---------- ELIMINAR EVALUACIÓN ----------
    // POST /Evaluaciones/Eliminar/5
    [Authorize(Roles = "Administrador,Profesor")]
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

    [Authorize(Roles = "Estudiante")]
    [HttpGet]
    public async Task<IActionResult> Resolver(int id)
    {
        var evaluacion = await _context.Evaluaciones
            .Include(e => e.Pregunta)
                .ThenInclude(p => p.OpcionesRespuesta)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (evaluacion == null)
            return NotFound();

        return View(evaluacion);
    }

    [Authorize(Roles = "Estudiante")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Resolver(int id, Dictionary<int, int> respuestas)
    {
        var evaluacion = await _context.Evaluaciones
            .Include(e => e.Pregunta)
                .ThenInclude(p => p.OpcionesRespuesta)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (evaluacion == null)
            return NotFound();

        int total = evaluacion.Pregunta.Count;
        int correctas = 0;

        foreach (var pregunta in evaluacion.Pregunta)
        {
            if (respuestas.TryGetValue(pregunta.Id, out int opcionId))
            {
                var opcion = pregunta.OpcionesRespuesta
                    .FirstOrDefault(o => o.Id == opcionId);

                if (opcion?.EsCorrecta == true)
                    correctas++;
            }
        }

        ViewBag.Calificacion = total > 0
            ? Math.Round((decimal)correctas / total * 100, 2)
            : 0;

        ViewBag.Correctas = correctas;
        ViewBag.Total = total;
        ViewBag.Resuelta = true;

        return View(evaluacion);
    }
}