using Aula.Data;
using Aula.Models;
using Aula.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Runtime.ConstrainedExecution;
using System.Security.Claims;

namespace Aula.Controllers;
[Authorize(Roles = "Administrador,Profesor")]
public class CursoController(EduLearnDbContext _context) : Controller
{
    [Authorize(Roles = "Administrador,Profesor")]
    public async Task<IActionResult> Index()
    {

        var query = _context.Cursos.AsQueryable();

        if (User.IsInRole("Profesor"))
        {
            var usuarioId = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!
            );

            query = query.Where(c => c.ProfesorId == usuarioId);
        }

        var cursos = await query
            .Select(c => new CursoListVM
            {
                CursoId = c.CursoId,
                Titulo = c.Titulo,
                Descripcion = c.Descripcion,
                NombreProfesor = c.Profesor.Nombre,
                Activo = c.Activo
            })
            .ToListAsync();

        return View(cursos);
    }

    [Authorize(Roles = "Administrador,Profesor")]
    public async Task<IActionResult> Create()
    {
        var vm = new CursoCreateVM();

        await CargarProfesoresAsync(vm.ProfesorId);

        return View(vm);
    }

    [Authorize(Roles = "Administrador,Profesor")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CursoCreateVM vm)
    {
        
        if (!await EsProfesorAsync(vm.ProfesorId))
        {
            ModelState.AddModelError(
                nameof(vm.ProfesorId),
                "Debes seleccionar un profesor válido.");
        }

        if (!ModelState.IsValid)
        {
            await CargarProfesoresAsync(vm.ProfesorId);
            return View(vm);
        }

        var curso = new Curso
        {
            Titulo = vm.Titulo,
            Descripcion = vm.Descripcion,
            ProfesorId = vm.ProfesorId,
            Activo = vm.Activo
        };

        _context.Cursos.Add(curso);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Administrador,Profesor")]
    public async Task<IActionResult> Edit(int id)
    {
        var curso = await _context.Cursos
            .FirstOrDefaultAsync(c => c.CursoId == id);

        if (curso == null)
        {
            return NotFound();
        }

        var vm = new CursoEditVM
        {
            CursoId = curso.CursoId,
            Titulo = curso.Titulo,
            Descripcion = curso.Descripcion,
            ProfesorId = curso.ProfesorId,
            Activo = curso.Activo
        };

        await CargarProfesoresAsync(vm.ProfesorId);

        return View(vm);
    }

    [Authorize(Roles = "Administrador,Profesor")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, CursoEditVM vm)
    {
        if (id != vm.CursoId)
        {
            return NotFound();
        }

        if (!await EsProfesorAsync(vm.ProfesorId))
        {
            ModelState.AddModelError(
                nameof(vm.ProfesorId),
                "Debes seleccionar un profesor válido.");
        }

        if (!ModelState.IsValid)
        {
            await CargarProfesoresAsync(vm.ProfesorId);
            return View(vm);
        }

        var curso = await _context.Cursos.FirstOrDefaultAsync(c => c.CursoId == id);

        if (curso == null)
            return NotFound();

        curso.Titulo = vm.Titulo;
        curso.Descripcion = vm.Descripcion;
        curso.ProfesorId = vm.ProfesorId;
        curso.Activo = vm.Activo;

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Administrador,Profesor")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var curso = await _context.Cursos
            .FirstOrDefaultAsync(c => c.CursoId == id);

        if (curso == null)
        {
            return NotFound();
        }
        var usuarioId = int.Parse( User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var usuarioRol = User.FindFirstValue(ClaimTypes.Role);

        if (curso.ProfesorId != usuarioId && usuarioRol != "Administrador")
        {
            return NotFound();
        }

        _context.Cursos.Remove(curso);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Administrador,Profesor,Estudiante")]
    public async Task<IActionResult> Gestionar(int id)
    {
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
            return Forbid();

        var esAdmin = User.IsInRole("Administrador");
        var esProfesor = User.IsInRole("Profesor");
        var esEstudiante = User.IsInRole("Estudiante");

        var query = _context.Cursos
            .AsNoTracking()
            .Where(c => c.CursoId == id);

        if (esAdmin)
        {
            // el administrador ve cualquier curso
        }
        else if (esProfesor)
        {
            query = query.Where(c => c.ProfesorId == userId);          // solo sus cursos
        }
        else if (esEstudiante)
        {
            // AJUSTA: nombre del DbSet y de las columnas de tu tabla de inscripciones
            query = query.Where(c => _context.Inscripciones.Any(i => i.CursoId == c.CursoId && i.EstudianteId == userId));
        }
        else
        {
            return Forbid();
        }

        var vm = await query
            .Select(c => new CursoGestionViewModel
            {
                CursoId = c.CursoId,
                Titulo = c.Titulo,
                Descripcion = c.Descripcion,
                TotalLecciones = c.Lecciones.Count,
                TotalRecursos = c.Lecciones.SelectMany(l => l.Recursos).Count(),
                TotalEvaluaciones = c.Lecciones.SelectMany(l => l.Evaluaciones).Count(),
                Lecciones = c.Lecciones.OrderBy(l => l.Orden).Select(l => new LeccionGestionVM
                {
                    Id = l.Id,
                    Orden = l.Orden,
                    Titulo = l.Titulo,
                    Recursos = l.Recursos.Select(r => new RecursoVM
                    {
                        Id = r.Id,
                        Nombre = r.Nombre,
                        Tipo = r.Tipo,
                        Url = r.Url
                    }).ToList(),
                    Evaluaciones = l.Evaluaciones.Select(e => new EvaluacionVM
                    {
                        Id = e.Id,
                        Titulo = e.Titulo,
                        CantidadPreguntas = e.Pregunta.Count
                    }).ToList()
                }).ToList()
            })
            .FirstOrDefaultAsync();

        if (vm == null) return NotFound();

        vm.PuedeEditar = esAdmin || esProfesor;
        vm.RolEtiqueta = esAdmin ? "Administrador" : esProfesor ? "Profesor" : "Estudiante";

        return View(vm);
    }



    //public async Task<IActionResult> Gestionar(int id)
    //{
    //    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

    //    var vm = await _context.Cursos
    //        .AsNoTracking()
    //        .Where(c => c.CursoId == id && c.ProfesorId == int.Parse(userId))   // solo su propio curso
    //        .Select(c => new CursoGestionViewModel
    //        {
    //            CursoId = c.CursoId,
    //            Titulo = c.Titulo,
    //            Descripcion = c.Descripcion,
    //            TotalLecciones = c.Lecciones.Count,
    //            TotalRecursos = c.Lecciones.SelectMany(l => l.Recursos).Count(),
    //            TotalEvaluaciones = c.Lecciones.SelectMany(l => l.Evaluaciones).Count(),
    //            Lecciones = c.Lecciones.OrderBy(l => l.Orden).Select(l => new LeccionGestionVM
    //            {
    //                Id = l.Id,
    //                Orden = l.Orden,
    //                Titulo = l.Titulo,
    //                Recursos = l.Recursos.Select(r => new RecursoVM
    //                {
    //                    Id = r.Id,
    //                    Nombre = r.Nombre,
    //                    Tipo = r.Tipo,
    //                    Url = r.Url
    //                }).ToList(),
    //                Evaluaciones = l.Evaluaciones.Select(e => new EvaluacionVM
    //                {
    //                    Id = e.Id,
    //                    Titulo = e.Titulo,
    //                    CantidadPreguntas = e.Pregunta.Count
    //                }).ToList()
    //            }).ToList()
    //        })
    //        .FirstOrDefaultAsync();

    //    if (vm == null) return NotFound();
    //    return View(vm);
    //}


    private async Task CargarProfesoresAsync(int? profesorIdSeleccionado)
    {
        ViewBag.Profesores = new SelectList(
            await _context.Usuarios
                .Where(u => u.Rol.Nombre == "Profesor" && u.Activo)
                .OrderBy(u => u.Nombre)
                .ToListAsync(),
            "UsuarioId",
            "Nombre",
            profesorIdSeleccionado);
    }

    private async Task<bool> EsProfesorAsync(int profesorId)
    {
        return await _context.Usuarios.AnyAsync(u =>
            u.UsuarioId == profesorId &&
            u.Rol.Nombre == "Profesor" &&
            u.Activo);
    }
}
