using System.Diagnostics;
using Aula.Data;
using Aula.Models;
using Aula.Services;
using Aula.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Aula.Controllers
{
    // AJUSTA: AulaDbContext y los nombres de tu scaffold (Cursos, Inscripciones,
    // Usuarios) y los nombres de rol.
    public class HomeController(EduLearnDbContext _context) : Controller
    {
        private const string RolEstudiante = "Estudiante";

        
        // Pública: sin sesión muestra la portada; con sesión, el panel según el rol
        public async Task<IActionResult> Index()
        {
            var vm = new HomeViewModel();

            if (User.Identity?.IsAuthenticated != true ||
                !CursoAccesoService.TryGetUserId(User, out var userId))
                return View(vm);

            if (User.IsInRole("Estudiante"))
            {
                vm.Rol = "Estudiante";
                await CargarEstudianteAsync(vm, userId);
            }
            else if (User.IsInRole("Profesor"))
            {
                vm.Rol = "Profesor";
                await CargarProfesorAsync(vm, userId);
            }
            else if (User.IsInRole("Administrador"))
            {
                vm.Rol = "Administrador";
                await CargarAdminAsync(vm);
            }

            return View(vm);
        }

        // ---------- Estudiante ----------
        private async Task CargarEstudianteAsync(HomeViewModel vm, int userId)
        {
            var inscripciones = _context.Inscripciones.AsNoTracking()
                .Where(i => i.EstudianteId == userId);

            var cursos = _context.Cursos.AsNoTracking()
                .Where(c => inscripciones.Any(i => i.CursoId == c.CursoId));

            var totalCursos = await cursos.CountAsync();
            var totalLecciones = await cursos.SelectMany(c => c.Lecciones).CountAsync();
            var totalEvaluaciones = await cursos
                .SelectMany(c => c.Lecciones).SelectMany(l => l.Evaluaciones).CountAsync();

            vm.Stats = new List<StatCardVM>
            {
                new() { Titulo = "Cursos inscritos", Valor = totalCursos.ToString(), Emoji = "📚", Color = "primary" },
                new() { Titulo = "Lecciones", Valor = totalLecciones.ToString(), Emoji = "📁", Color = "success" },
                new() { Titulo = "Evaluaciones", Valor = totalEvaluaciones.ToString(), Emoji = "📝", Color = "warning" }
            };

            vm.Cursos = await (
                from i in inscripciones
                join c in _context.Cursos.AsNoTracking() on i.CursoId equals c.CursoId
                orderby i.FechaInscripcion descending
                select new CursoResumenVM
                {
                    CursoId = c.CursoId,
                    Titulo = c.Titulo,
                    Lecciones = c.Lecciones.Count
                }).Take(3).ToListAsync();
        }

        // ---------- Profesor ----------
        private async Task CargarProfesorAsync(HomeViewModel vm, int userId)
        {
            var cursos = _context.Cursos.AsNoTracking().Where(c => c.ProfesorId == userId);

            var totalCursos = await cursos.CountAsync();
            var totalLecciones = await cursos.SelectMany(c => c.Lecciones).CountAsync();
            var totalEvaluaciones = await cursos
                .SelectMany(c => c.Lecciones).SelectMany(l => l.Evaluaciones).CountAsync();
            var totalEstudiantes = await _context.Inscripciones.AsNoTracking()
                .Where(i => cursos.Any(c => c.CursoId == i.CursoId))
                .Select(i => i.EstudianteId)
                .Distinct()
                .CountAsync();

            vm.Stats = new List<StatCardVM>
            {
                new() { Titulo = "Mis cursos", Valor = totalCursos.ToString(), Emoji = "📚", Color = "primary" },
                new() { Titulo = "Lecciones", Valor = totalLecciones.ToString(), Emoji = "📁", Color = "success" },
                new() { Titulo = "Evaluaciones", Valor = totalEvaluaciones.ToString(), Emoji = "📝", Color = "warning" },
                new() { Titulo = "Estudiantes", Valor = totalEstudiantes.ToString(), Emoji = "🎓", Color = "info" }
            };

            vm.Cursos = await cursos
                .OrderByDescending(c => c.CursoId)
                .Take(5)
                .Select(c => new CursoResumenVM
                {
                    CursoId = c.CursoId,
                    Titulo = c.Titulo,
                    Lecciones = c.Lecciones.Count,
                    Inscritos = _context.Inscripciones.Count(i => i.CursoId == c.CursoId)
                }).ToListAsync();
        }

        // ---------- Administrador ----------
        private async Task CargarAdminAsync(HomeViewModel vm)
        {
            var totalCursos = await _context.Cursos.CountAsync();
            var totalProfesores = await _context.Usuarios
                .CountAsync(u => u.Rol.Nombre == "Profesor" && u.Activo);
            var totalEstudiantes = await _context.Usuarios
                .CountAsync(u => u.Rol.Nombre == RolEstudiante && u.Activo);
            var totalInscripciones = await _context.Inscripciones.CountAsync();

            vm.Stats = new List<StatCardVM>
            {
                new() { Titulo = "Cursos", Valor = totalCursos.ToString(), Emoji = "📚", Color = "primary" },
                new() { Titulo = "Profesores", Valor = totalProfesores.ToString(), Emoji = "👨‍🏫", Color = "success" },
                new() { Titulo = "Estudiantes", Valor = totalEstudiantes.ToString(), Emoji = "🎓", Color = "info" },
                new() { Titulo = "Inscripciones", Valor = totalInscripciones.ToString(), Emoji = "✅", Color = "warning" }
            };

            vm.Cursos = await _context.Cursos.AsNoTracking()
                .OrderByDescending(c => c.CursoId)
                .Take(5)
                .Select(c => new CursoResumenVM
                {
                    CursoId = c.CursoId,
                    Titulo = c.Titulo,
                    Lecciones = c.Lecciones.Count,
                    Inscritos = _context.Inscripciones.Count(i => i.CursoId == c.CursoId),
                    Profesor = _context.Usuarios
                        .Where(u => u.UsuarioId == c.ProfesorId)
                        .Select(u => u.Nombre)
                        .FirstOrDefault()
                }).ToListAsync();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}